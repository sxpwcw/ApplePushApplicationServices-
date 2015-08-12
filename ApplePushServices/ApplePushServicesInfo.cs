using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetWorkGroup.Data;
using NetWorkGroup.Data.SqlServer;
using System.Data;

namespace ApplePushServices
{
    /// <summary>
    /// 表示应用程序通知所使用的属性类
    /// </summary>
    public class ApplePushServicesInfo
    {
        /// <summary>
        /// 获取或者设置一个值，该值表示证书路径和密码的键值对
        /// </summary>
        public Dictionary<String, String> CertificateWithPassword { get; set; }
        /// <summary>
        /// 获取或者设置一个值，该值表示设备标识列表
        /// </summary>
        public List<String> DevicesTokens
        {
            get
            {
                List<String> _token = new List<string>();
                SqlServerDb db = new SqlServerDb(ConnectionString);
                db.cmdText = "select " + ColumnName + " from " + SavedTokenTableName + "";
                var resultTable = db.Execute<DataTable>(CommandType.Text);
                foreach (DataRow item in resultTable.Rows)
                {
                    _token.Add(item[0].ToString().Replace("<", "").Replace(">", "").Replace(" ", "").ToStringTrim());
                }
                return _token;
            }
        }
        public String SingleDeviceToken { get; set; }
        /// <summary>
        /// 获取或者设置一个值，该值表示通知到期时间
        /// </summary>
        public DateTime? Expiration { get; set; }
        /// <summary>
        /// 获取或者设置一个值，该值表示数据库连接字符串
        /// </summary>
        public DbConnectionString ConnectionString { get; set; }

        public String SavedTokenTableName { get; set; }

        public String ColumnName { get; set; }
    }
    /// <summary>
    /// 表示一个通知的类型
    /// </summary>
    public class Payload<T> where T : new()
    {
        /// <summary>
        /// 获取或者设置一个值，该值表示通知主题内容信息
        /// </summary>
        public T aps { get; set; }
    }
    /// <summary>
    /// 表示一个值通知内容的主体
    /// </summary>
    public class PayloadContext
    {
        /// <summary>
        /// 获取或者设置一个值，该值表示通知内容
        /// </summary>
        public String alert { get; set; }
        /// <summary>
        /// 获取或者设置一个值，该值表示应用程序角标的数字
        /// </summary>
        public Int64 badge { get; set; }
        /// <summary>
        /// 获取或者设置一个值，该值表示通知到客户端时应该播放的声音
        /// </summary>
        public String sound { get; set; }
    }
}
