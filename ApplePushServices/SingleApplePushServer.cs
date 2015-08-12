using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ApplePushServices
{
    public class SingleApplePushServer
    {
        private ApplePushServicesInfo info;

        private Payload<PayloadContext> PushPayload;

        private X509Certificate cer;

        private X509CertificateCollection CerCollection;


        private TcpClient apnsClient = new TcpClient();

        private static System.Threading.Thread thread;

        private List<KeyValuePair<byte[], String>> SendMessages;
        /// <summary>
        /// 推送完成之后的时间处理函数
        /// </summary>
        public event PushCompleteEventHandler OnPushComplete;

        private Exception err;
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SingleApplePushServer()
        {
            CerCollection = new X509CertificateCollection();
            SendMessages = new List<KeyValuePair<byte[], string>>();
            thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart((apnsStream) =>
            {
                List<String> sucess = new List<string>();
                long x = 0;
                var readStream = apnsClient.GetStream();
                foreach (var item in this.SendMessages)
                {
                    try
                    {
                        ((SslStream)apnsStream).Write(item.Key);
                        x++;
                        sucess.Add(item.Value);
                    }
                    catch (Exception e)
                    {
                        x--;
                        this.err = e;
                    }
                    System.Threading.Thread.Sleep(3000);
                }
                try
                {
                    thread.Abort();
                }
                catch { }
                finally
                {
                    if (readStream.DataAvailable)
                    {
                        var buffers = new byte[512];
                        var i = readStream.Read(buffers, 0, buffers.Length);
                        this.err = new Exception("发送消息失败，服务器返回错误！" + UTF8Encoding.UTF8.GetString(buffers));
                    }
                    if (OnPushComplete != null)
                    {
                        OnPushComplete(new PushCompleteEventArgs() { Result = x, Error = err, CompleteToken = sucess });
                    }
                    apnsClient.Close();
                    ((SslStream)apnsStream).Dispose();
                    apnsStream = null;
                }
                thread = null;
            }));
        }
        /// <summary>
        /// 使用指定的参数初始化推送实例
        /// </summary>
        /// <param name="_info">推送服务属性类</param>
        /// <param name="_payload">推送主体内容</param>
        /// <param name="type">推送通知的类型</param>
        public SingleApplePushServer(ApplePushServicesInfo _info, Payload<PayloadContext> _payload, PushType type)
            : this()
        {
            this.info = _info;
            this.PushPayload = _payload;
            Public.PushType = type;
        }
        private void init()
        {
            foreach (var item in this.info.CertificateWithPassword)
            {
                cer = new X509Certificate2(System.IO.File.ReadAllBytes(item.Key), item.Value, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                CerCollection.Add(cer);
            }
            apnsClient.Connect(Public.HostServerAddress, Public.Port);
        }
        private void initMessage()
        {
            int identifier = 0;
            byte[] identifierBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(identifier));
            int expiryTimeStamp = -1;
            if (info.Expiration != Public.DoNotStore)
            {
                DateTime concreteExpireDateUtc = (info.Expiration ?? DateTime.UtcNow.AddSeconds(20)).ToUniversalTime();
                TimeSpan epochTimeSpan = concreteExpireDateUtc - Public.UNIX_EPOCH;
                expiryTimeStamp = (int)epochTimeSpan.TotalSeconds;
            }
            byte[] expiry = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(expiryTimeStamp));
            byte[] deviceToken = new byte[this.info.SingleDeviceToken.Length / 2];
            for (int i = 0; i < deviceToken.Length; i++)
                deviceToken[i] = byte.Parse(this.info.SingleDeviceToken.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            if (deviceToken.Length != Public.DeviceTokenBinarySize)
            {
                throw new Exception("客户端表示长度错误！");
            }
            byte[] deviceTokenSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(deviceToken.Length)));
            string str = JsonConvert.SerializeObject(PushPayload);
            Console.WriteLine(str);
            byte[] payload = Encoding.UTF8.GetBytes(str);
            byte[] payloadSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(payload.Length)));
            List<byte[]> notificationParts = new List<byte[]>();
            //1 Command 
            notificationParts.Add(new byte[] { 0x01 }); // Enhanced notification format command 
            notificationParts.Add(identifierBytes);
            notificationParts.Add(expiry);
            notificationParts.Add(deviceTokenSize);
            notificationParts.Add(deviceToken);
            notificationParts.Add(payloadSize);
            notificationParts.Add(payload);
            SendMessages.Add(new KeyValuePair<byte[], string>(this.BuildBufferFrom(notificationParts), this.info.SingleDeviceToken));
        }
        private byte[] BuildBufferFrom(IList<byte[]> bufferParts)
        {
            int bufferSize = 0;
            for (int i = 0; i < bufferParts.Count; i++)
                bufferSize += bufferParts[i].Length;
            byte[] buffer = new byte[bufferSize];
            int position = 0;
            for (int i = 0; i < bufferParts.Count; i++)
            {
                byte[] part = bufferParts[i];
                Buffer.BlockCopy(bufferParts[i], 0, buffer, position, part.Length);
                position += part.Length;
            }
            return buffer;
        }
        /// <summary>
        /// 发送推送通知
        /// </summary>
        public void Push()
        {
            try
            {
                this.init();
                SslStream apnsStream = new SslStream(apnsClient.GetStream(), false, new RemoteCertificateValidationCallback(validateServerCertificate), new LocalCertificateSelectionCallback(selectLocalCertificate));
                try
                {
                    //APNs已不支持SSL 3.0  
                    apnsStream.AuthenticateAsClient(Public.HostServerAddress, CerCollection, System.Security.Authentication.SslProtocols.Tls, false);
                }
                catch (System.Security.Authentication.AuthenticationException ex)
                {
                    throw new Exception(ex.Message);
                }
                if (!apnsStream.IsMutuallyAuthenticated)
                {
                    throw new Exception("SSL加密流认证失败！");
                }
                if (!apnsStream.CanWrite)
                {
                    throw new Exception("加密流不可写！");
                }
                initMessage();
                thread.Start(apnsStream);
            }
            catch (Exception e)
            {
                this.err = e;
            }
        }
        private bool validateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        private X509Certificate selectLocalCertificate(object sender, string targetHost, X509CertificateCollection localCertificates,
         X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return cer;
        }
    }
}
