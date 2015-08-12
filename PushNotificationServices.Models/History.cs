using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;

namespace PushNotificationServices.Models
{
	/// <summary>
	/// 表示TB_History类
	/// </summary>
	[Table(Name = "TB_History")]
	public class History:IModelID<Int64>
	{
		/// <summary>
		/// 获取或者设置一个值，该值表示ID
		/// </summary>
		[Display(Name = "ID")]
		[Column(IsDbGenerated = true, IsPrimaryKey = true, CanBeNull = false)]
		public Int64 ID { get; set; }
		/// <summary>
		/// 获取或者设置一个值，该值表示Msg
		/// </summary>
		[Display(Name = "Msg")]
		[Column]
		[Required]
		public String Msg { get; set; }
		/// <summary>
		/// 获取或者设置一个值，该值表示HistoryData
		/// </summary>
		[Display(Name = "HistoryData")]
		[Column]
		[Required]
		public DateTime HistoryData { get; set; }
	}
}
