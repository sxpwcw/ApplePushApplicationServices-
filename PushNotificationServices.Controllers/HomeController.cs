using NetWorkGroup.Entity;
using PushNotificationServices.Controllers.Abstract;
using PushNotificationServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PushNotificationServices.Controllers
{
    public class HomeController : ControllerBase<NotificationDevice>
    {
        public ActionResult Index()
        {
            return base.List("");
        }
        public ActionResult Put() { return View(); }
        public ActionResult History()
        {
            var data = new DefaultEntity<History>().Show().OrderByDescending(p => p.ID);
            int skip = (Request["page"] == null || Request["page"].Trim() == "1") ? 0 : 10 * (Request["page"] == null ? 1 : Convert.ToInt32(Request["page"]) - 1);
            PagerOptions<History> paged = new PagerOptions<History>()
            {
                ItemCollection = data.Skip(skip).Take(10),
                PageSize = 10,
                MaxRecord = data.Count()
            };
            return View(paged);
        }
        [HttpPost]
        public JsonResult UploadFile(String FileExtensions)
        {
            System.IO.File.AppendAllText(Server.MapPath("~/Log.Log"), "开始写入文件" + DateTime.Now.ToString() + "\r\n\r\n");
            var SaveFileName = Server.MapPath("~/Uploaded/" + Guid.NewGuid().ToString() + "." + FileExtensions);
            var InputStream = Request.InputStream;
            System.IO.File.AppendAllText(Server.MapPath("~/Log.Log"), "文件大小" + Request.InputStream.Length + "\r\n\r\n");
            byte[] readByte = new byte[InputStream.Length];
            FileStream fs = null;
            long FileLength = 0;
            using (fs = new FileStream(SaveFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                InputStream.Read(readByte, 0, readByte.Length);
                fs.Write(readByte, 0, readByte.Length);
                fs.Flush();
                FileLength = fs.Length;
            }
            if (FileLength > 0)
            {
                return new JsonResult()
                {
                    Data = new { ret = true, msg = SaveFileName },
                    ContentEncoding = System.Text.Encoding.UTF8,
                };
            }
            else
            {
                return new JsonResult()
                {
                    Data = new { ret = true, msg = "NoFile" },
                    ContentEncoding = System.Text.Encoding.UTF8,
                };
            }
        }
        public ActionResult Push()
        {
            return View();
        }
    }
}
