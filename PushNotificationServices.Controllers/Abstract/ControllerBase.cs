using NetWorkGroup.Data;
using NetWorkGroup.Entity;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PushNotificationServices.Controllers.Abstract
{
    /// <summary>
    /// 控制器基类
    /// </summary>
    /// <typeparam name="TModel">模型应用层</typeparam>
    public abstract class ControllerBase<TModel> : Controller where TModel : class, IModelID<Int64>, new()
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ControllerBase()
        {
        }
        private DefaultEntity<TModel> _Logic = new DefaultEntity<TModel>();
        /// <summary>
        /// 全局可使用的默认实体类的实例
        /// </summary>
        public DefaultEntity<TModel> Logic
        {
            get
            {
                return this._Logic;
            }
            set
            {
                this._Logic = value;
            }
        }

        private JavaScriptResult _JsResult = new JavaScriptResult();
        /// <summary>
        /// 用于想浏览器输出一个JavaScriptResult
        /// </summary>
        public JavaScriptResult JsResult
        {
            get
            {
                return _JsResult;
            }
            set
            {
                _JsResult = value;
            }
        }
        /// <summary>
        /// 获取或者设置一个值，该值表示默认执行返回值的逻辑
        /// </summary>
        public String DefaultScript
        {
            get;
            set;
        }
        /// <summary>
        /// 出错信息，私有属性
        /// </summary>
        private Exception e { get; set; }
        /// <summary>
        /// 用于想浏览器输出错误信息时使用Alert脚本，只读
        /// </summary>
        public String ExceptionScript
        {
            get
            {
                return "alert('处理数据库逻辑时发生系统错误：" + e.Message.Trim().Replace("\"", "").Replace("'", "").Replace("\r\n", "").Trim() + "')";
            }
        }
        /// <summary>
        /// alert信息提示完毕后系统触发的js方法
        /// </summary>
        public String OtherScript { get; set; }
        /// <summary>
        /// 默认的添加信息行为
        /// </summary>
        /// <returns>ActionResult</returns>
        public virtual ActionResult Append()
        {
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public virtual JsonResult AppendUseJson(TModel model)
        {
            throw new NullReferenceException();
        }
        /// <summary>
        /// 默认的增加信息功能
        /// </summary>
        /// <param name="model">模型类</param>
        /// <returns>JavaScriptResult</returns>
        [HttpPost, ValidateInput(false)]
        public virtual JavaScriptResult Append(TModel model)
        {
            try
            {
                Logic.DbContext = new DbContext<TModel>()
                {
                    Model = model
                };
                Logic.Append();
                if (String.IsNullOrEmpty(this.DefaultScript))
                {
                    if (String.IsNullOrEmpty(OtherScript))
                    {
                        this.JsResult.Script = ("alert('" + Logic.Result.ReturnValue.ToString().Trim() + "');parent.Dialog.Hide();");
                    }
                    else
                    {
                        this.JsResult.Script = ("alert('" + Logic.Result.ReturnValue.ToString().Trim() + "');" + this.OtherScript + "");
                    }
                }
                else
                {
                    this.JsResult.Script = DefaultScript;
                }
                //log.Info("增加【" + model.GetType().ToString()+"】成功！");
            }
            catch (Exception err)
            {
                //log.Error(err);
                this.e = err;
                this.JsResult.Script = this.ExceptionScript;
            }
            return this.JsResult;
        }
        /// <summary>
        /// 默认的更新信息行为
        /// </summary>
        /// <param name="ID">需要查询数据的ID类型为IModel.ID</param>
        /// <returns>ActionResult</returns>
        public virtual ActionResult Update(Int64? ID)
        {
            return View(new DefaultEntity<TModel>().Show().SingleOrDefault(p => p.ID == ID));
        }
        /// <summary>
        /// 默认的信息更新方法
        /// </summary>
        /// <param name="model">模型类</param>
        /// <returns>JavaScriptResult</returns>
        [HttpPost, ValidateInput(false)]
        public virtual JavaScriptResult Update(TModel model)
        {

            JavaScriptResult js = new JavaScriptResult();
            DefaultEntity<TModel> Logic = new DefaultEntity<TModel>()
            {
                DbContext = new DbContext<TModel>()
                {
                    Model = model
                }
            };
            Logic.Update();
            if (String.IsNullOrEmpty(this.DefaultScript))
            {
                if (String.IsNullOrEmpty(OtherScript))
                {
                    this.JsResult.Script = ("alert('" + Logic.Result.ReturnValue.ToString().Trim() + "');parent.Dialog.Hide();");
                }
                else
                {
                    this.JsResult.Script = ("alert('" + Logic.Result.ReturnValue.ToString().Trim() + "');" + this.OtherScript + "");
                }
            }
            else
            {
                JsResult.Script = DefaultScript;
            }
            //log.Info("更新【" + model.GetType().ToString() + "】成功！");
            return JsResult;
        }
        /// <summary>
        /// 执行信息的删除
        /// </summary>
        /// <param name="ID">要删除的信息ID列表</param>
        /// <returns>String</returns>
        [HttpPost]
        public virtual String Delete(String ID)
        {
            var Result = String.Empty;
            String TableName = String.Empty;
            //获取用户是否使用了TableAttribute特性
            var attr = typeof(TModel).GetCustomAttributes(typeof(TableAttribute), false);
            if (attr.Count() > 0)
            {
                //获取表名称
                if (attr.Where(p => p.GetType() == typeof(TableAttribute)).SingleOrDefault().As<TableAttribute>().Name != null)
                {
                    TableName = attr.Where(p => p.GetType() == typeof(TableAttribute)).SingleOrDefault().As<TableAttribute>().Name.Trim();
                }
                else
                {
                    TableName = typeof(TModel).FullName.Replace(typeof(TModel).Namespace + ".", "").Trim();
                }
                String DeleteSql = string.Format("Delete From " + TableName + " where id in ({0})", ID);
                var i = this.Logic.DbContext.ExecuteCommand(DeleteSql);
                if (i > 0)
                {
                    Result = "成功删除了" + i + "条信息！";
                }
                else
                {
                    Result = "数据处理失败，请联系系统管理员！";
                }
                return Result;
            }
            else
            {
                return "实体类型没有实现TableAttribute特性，获取表名称失败！";
            }
        }
        /// <summary>
        /// 默认的列表显示行为
        /// </summary>
        /// <returns>ActionResult</returns>
        public virtual ActionResult List(String id)
        {
            var data = new DefaultEntity<TModel>().Show().OrderBy(p => p.ID);
            int skip = (Request["page"] == null || Request["page"].Trim() == "1") ? 0 : 10 * (Request["page"] == null ? 1 : Convert.ToInt32(Request["page"]) - 1);
            PagerOptions<TModel> paged = new PagerOptions<TModel>()
            {
                ItemCollection = data.Skip(skip).Take(10),
                PageSize = 10,
                MaxRecord = data.Count()
            };
            return View(paged);
        }
        public virtual ActionResult Search(string view = "List")
        {
            if (Request.QueryString == null)
            {
                return View("Error");
            }
            Dictionary<String, String> dict = new Dictionary<string, string>();
            foreach (string request in Request.QueryString)
            {
                if (string.IsNullOrEmpty(Request.QueryString[request]) || request.Equals("page", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                dict.Add(request.ToUpper(), Request.QueryString[request]);
            }
            var data = this.Logic.Show().AsEnumerable();
            List<TModel> searchRet = new List<TModel>();
            foreach (var d in data)
            {
                //仅取得需要查询的属性
                var properties = d.GetType().GetProperties().Where(p => dict.Keys.Contains(p.Name.ToUpper()));
                var correctNum = 0;
                foreach (var p in properties)
                {
                    foreach (var query in dict)
                    {
                        var value = p.GetValue(d, null);
                        if (value != null && value.ToString().Contains(query.Value))
                        {
                            correctNum++;
                            //禁止不必要的循环
                            break;
                        }
                    }
                }
                //如果查询正确计数和字典的值一致，则认为是需要查询的数据
                if (correctNum == dict.Count)
                {
                    searchRet.Add(d);
                }
            }

            var pagedData = searchRet.OrderByDescending(p => p.ID);
            int skip = (Request["page"] == null || Request["page"].Trim() == "1") ? 0 : 10 * (Request["page"] == null ? 1 : Convert.ToInt32(Request["page"]) - 1);
            PagerOptions<TModel> paged = new PagerOptions<TModel>()
            {
                ItemCollection = pagedData.Skip(skip).Take(10),
                PageSize = 10,
                MaxRecord = pagedData.Count()
            };
            ViewBag.IsShowBack = true;
            return View(view, paged);
        }
        /// <summary>
        /// 用户AJAX方发，获取绝对路径
        /// </summary>
        /// <param name="Path">相对路径</param>
        /// <returns>String</returns>
        [HttpPost]
        public String AbsolutePath(String Path)
        {
            return Url.Content(Path);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.Logic != null)
            {
                this.Logic.Dispose();
                GC.ReRegisterForFinalize(Logic);
                GC.Collect();
                this.Logic = null;
            }
            base.Dispose(disposing);
        }
    }
}
