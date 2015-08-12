using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using NetWorkGroup.Entity;

namespace PushNotificationServices.Models
{
	/// <summary>
	/// 表示TB_NotificationDevices类
	/// </summary>
	[Table(Name = "TB_NotificationDevices")]
	public class NotificationDevice:IModelID<Int64>
	{
		/// <summary>
		/// 获取或者设置一个值，该值表示id
		/// </summary>
		[Display(Name = "id")]
		[Column(IsDbGenerated = true, IsPrimaryKey = true, CanBeNull = false)]
		public Int64 ID { get; set; }
		/// <summary>
		/// 获取或者设置一个值，该值表示Tokens
		/// </summary>
		[Display(Name = "Tokens")]
		[Column]
		[Required]
		public String Tokens { get; set; }

        public bool IsHavedTokens()
        {
            DefaultEntity<NotificationDevice> logic = new DefaultEntity<NotificationDevice>();
            return logic.Show(p => p.Tokens.Equals(this.Tokens)).Count() > 0; 
        }
	}
}
