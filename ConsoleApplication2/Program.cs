using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplePushServices;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<String,object> alerts=new Dictionary<string,object>();
            alerts.Add("action-loc-key", "打开我的应用");
            alerts.Add("body","这个是一条测试数据!");
            alerts.Add("title", "测试标题");

            PayloadContextExtensions exten = new PayloadContextExtensions()
            {
                alert = alerts,
                badge = 100,
                server = new { serverId = 1, name = "测试服务器" },
                sound = "default"
            };
            Payload<PayloadContextExtensions> payloads = new Payload<PayloadContextExtensions>()
            {
                aps = exten
            };

            string certificatepath = "push.p12";
            string p12Filename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, certificatepath);
            Dictionary<String, String> Cer = new Dictionary<string, string>();
            Cer.Add(p12Filename, "wcw840525");
            ApplePushServices.Generic.ApplePushServer<PayloadContextExtensions> services = new ApplePushServices.Generic.ApplePushServer<PayloadContextExtensions>(new ApplePushServicesInfo()
            {
                CertificateWithPassword = Cer,
                ConnectionString = new NetWorkGroup.Data.DbConnectionString()
                {
                    DataBase = "qds161729659_db",
                    DataSource = "qds161729659.my3w.com",
                    DbType = NetWorkGroup.Data.DataBaseType.SqlServer,
                    EncodingName = String.Empty,
                    IsPooling = true,
                    Password = "wcw840525",
                    Port = 1433,
                    UserID = "qds161729659"
                },
                ColumnName = "Tokens",
                SavedTokenTableName = "TB_NotificationDevices"
            }, payloads, PushType.Sub);
            services.Push();
            //ApplePushServer server = new ApplePushServer(new ApplePushServicesInfo()
            //{
            //    CertificateWithPassword = Cer,
            //    ConnectionString = new NetWorkGroup.Data.DbConnectionString()
            //    {
            //        DataBase = "qds161729659_db",
            //        DataSource = "qds161729659.my3w.com",
            //        DbType = NetWorkGroup.Data.DataBaseType.SqlServer,
            //        EncodingName = String.Empty,
            //        IsPooling = true,
            //        Password = "wcw840525",
            //        Port = 1433,
            //        UserID = "qds161729659"
            //    },
            //    ColumnName = "Tokens",
            //    SavedTokenTableName = "TB_NotificationDevices"
            //}, new Payload<PayloadContext>()
            //{
            //    aps = new PayloadContext()
            //    {
            //        alert = "系统测试",
            //        badge = 1,
            //        sound = "pushmsg.caf"
            //    }
            //}, PushType.Sub);
            //server.OnPushComplete += server_OnPushComplete;
            //server.Push();
            Console.ReadKey();
        }

        static void server_OnPushComplete(PushCompleteEventArgs e)
        {
            Console.WriteLine(e.Result.ToStringTrim());
        }
        internal class PayloadContextExtensions
        {
            public object alert { get; set; }

            public Int32 badge { get; set; }

            public string sound { get; set; }

            public object server { get; set; }
        }
    }
}
