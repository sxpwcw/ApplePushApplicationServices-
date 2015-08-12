using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ApplePushServices
{
    public class PushCompleteEventArgs : EventArgs
    {
        public PushCompleteEventArgs() { }

        public Int64 Result { get; set; }

        public Exception Error { get; set; }

        public List<String> CompleteToken { get; set; }

    }
    public delegate void PushCompleteEventHandler(PushCompleteEventArgs e);
    /// <summary>
    /// 推送通知类型
    /// </summary>
    public enum PushType
    {
        /// <summary>
        /// 测试环境
        /// </summary>
        Developer = 0,
        /// <summary>
        /// 正式环境
        /// </summary>
        Sub = 1,
        /// <summary>
        /// 无
        /// </summary>
        None = -1
    }
    /// <summary>
    /// 公共类型，包含静态属性
    /// </summary>
    public abstract class Public
    {
        /// <summary>
        /// 获取或者设置一个值，该值表示推送通知类型
        /// </summary>
        public static PushType PushType { get; set; }
        /// <summary>
        /// 获取一个值，表示苹果推送服务器地址
        /// </summary>
        public static String HostServerAddress
        {
            get
            {
                String Url = String.Empty;
                switch (PushType)
                {
                    case ApplePushServices.PushType.Developer:
                        Url = "gateway.sandbox.push.apple.com";
                        break;
                    case ApplePushServices.PushType.Sub:
                        Url = "gateway.push.apple.com";
                        break;
                    case ApplePushServices.PushType.None:
                        throw new Exception("没有设置通知类型，这样会造成无法获取推送服务器地址！");
                        break;
                }
                return Url;
            }
        }
        /// <summary>
        /// 获取一个值，该值表示苹果推送服务器端口号
        /// </summary>
        public static Int32 Port { get { return 2195; } }
        /// <summary>
        /// 获取一个值，该值表示客户端令牌的二进制大小
        /// </summary>
        public static Int32 DeviceTokenBinarySize { get { return 32; } }
        /// <summary>
        /// 获取一个值，该值表示客户端令牌的字符串长度
        /// </summary>
        public static Int32 DeviceTokenStringSize { get { return 64; } }
        /// <summary>
        /// 获取一个值，该值表示通知包最大字节大小
        /// </summary>
        public static Int32 PayloadMaxSize { get { return 256; } }

        public static DateTime DoNotStore { get { return DateTime.MinValue; } }

        public static DateTime UNIX_EPOCH { get { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); } }
    }
}
