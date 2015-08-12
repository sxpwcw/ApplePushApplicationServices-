using ApplePushServices;
using NetWorkGroup.Data;
using PushNotificationServices.Controllers.Abstract;
using PushNotificationServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PushNotificationServices.Controllers
{
    public class ApiController : ControllerBase<NotificationDevice>
    {
        public JsonResult AllTokens()
        {
            JsonResult j = new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            var logic = base.Logic.Show().OrderByDescending(p => p.ID);
            var result = from x in logic
                         select new
                         {
                             ID = x.ID,
                             Token = x.Tokens.Replace(">", "").Replace("<", "").Replace(" ", "").ToString().Trim()
                         };
            j.Data = result;
            return j;
        }
        [HttpPost]
        public override JsonResult AppendUseJson(NotificationDevice model)
        {
            if (!model.IsHavedTokens())
            {
                base.Logic.DbContext = new DbContext<NotificationDevice>()
                {
                    Model = model
                };
                base.Logic.Append("True");
                String Ret = base.Logic.Result.ReturnValue;
                if (Boolean.Parse(Ret))
                {
                    return new JsonResult()
                    {
                        ContentEncoding = System.Text.UTF8Encoding.UTF8,
                        JsonRequestBehavior = JsonRequestBehavior.DenyGet,
                        Data = new ResultData<String>()
                        {
                            ret = true,
                            msg = "增加数据库成功！",
                            data = String.Empty,
                            code = 200,
                        },
                    };
                }
                else
                {
                    return new JsonResult()
                    {
                        ContentEncoding = System.Text.UTF8Encoding.UTF8,
                        JsonRequestBehavior = JsonRequestBehavior.DenyGet,
                        Data = new ResultData<String>()
                        {
                            ret = false,
                            msg = "增加数据库失败！",
                            data = String.Empty,
                            code = 500
                        }
                    };
                }
            }
            else
            {
                return new JsonResult()
                {
                    ContentEncoding = System.Text.UTF8Encoding.UTF8,
                    JsonRequestBehavior = JsonRequestBehavior.DenyGet,
                    Data = new ResultData<String>()
                    {
                        ret = false,
                        msg = "增加数据库失败，服务器存在同样的Tokens！",
                        data = String.Empty,
                        code = 500
                    }
                };
            }
        }

        [HttpGet]
        public void PostNotification(String pType, String Msg, String Badge, String AudioName)
        {
            base.Logic.DbContext.ExecuteCommand("insert into TB_History values('开始准备发送推送！','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')");
            Dictionary<String, String> Cer = new Dictionary<string, string>();
            PushType type = PushType.None;
            switch (pType)
            {
                case "d":
                    type = PushType.Developer;
                    Cer.Add(Server.MapPath("~/Cer/pushCer.p12"), "wcw840525");
                    break;
                case "s":
                    type = PushType.Sub;
                    Cer.Add(Server.MapPath("~/Cer/push.p12"), "wcw840525");
                    break;
                default:
                    type = PushType.None;
                    break;
            }
            ApplePushServer server = new ApplePushServices.ApplePushServer(new ApplePushServices.ApplePushServicesInfo()
            {
                CertificateWithPassword = Cer,
                ColumnName = "Tokens",
                SavedTokenTableName = "TB_NotificationDevices",
                ConnectionString = base.Logic.ConnectionString
            }, new ApplePushServices.Payload<PayloadContext>()
            {
                aps = new PayloadContext()
                {
                    alert = Msg,
                    badge = Badge.ToInt64(),
                    sound = AudioName
                }
            }, type);
            server.OnPushComplete += server_OnPushComplete;
            server.Push();

        }
        [HttpPost]
        public void PostNotificationWithOptions(String DeviceToken, String pType, String Msg, String Badge, String AudioName)
        {
            if (DeviceToken == "All")
            {
                this.PostNotification(pType, Msg, Badge, AudioName);
            }
            else
            {
                Dictionary<String, String> Cer = new Dictionary<string, string>();
                PushType type = PushType.None;
                switch (pType)
                {
                    case "d":
                        type = PushType.Developer;
                        Cer.Add(Server.MapPath("~/Cer/pushCer.p12"), "wcw840525");
                        break;
                    case "s":
                        type = PushType.Sub;
                        Cer.Add(Server.MapPath("~/Cer/push.p12"), "wcw840525");
                        break;
                    default:
                        type = PushType.None;
                        break;
                }
                SingleApplePushServer logic = new SingleApplePushServer(new ApplePushServicesInfo()
                {
                    CertificateWithPassword = Cer,
                    SingleDeviceToken = DeviceToken
                }, new Payload<PayloadContext>()
                {
                    aps = new PayloadContext()
                    {
                        alert = Msg,
                        badge = Badge.ToInt64(),
                        sound = AudioName
                    }
                }, type);
                logic.OnPushComplete += server_OnPushComplete;
                logic.Push();
            }
        }

        void server_OnPushComplete(PushCompleteEventArgs e)
        {
            var db = new NetWorkGroup.Entity.DefaultEntity<NotificationDevice>();
            if (e.Error != null)
            {
                db.DbContext.ExecuteCommand("insert into TB_History values('" + e.Error.Message.Trim() + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')");
            }
            if (e.Result < 1)
            {
                db.DbContext.ExecuteCommand("insert into TB_History values('所有设备均未能发送成功推送！','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')");
            }
            else
            {
                String results = string.Empty;
                foreach (var item in e.CompleteToken)
                {
                    results += item + ",";
                }
                db.DbContext.ExecuteCommand("insert into TB_History values('共成功推送了" + e.Result + "个设备！成功的数据为：" + results.CutLastString(",") + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')");
            }
        }
    }
    internal class ResultData<T> where T : class
    {
        public string msg { get; set; }

        public bool ret { get; set; }

        public T data { get; set; }

        public Int64 code { get; set; }
    }
}
