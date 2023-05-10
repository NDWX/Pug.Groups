using System.Runtime.Serialization;
using Pug.Effable;

namespace Pug.Groups.Models
{
	[DataContract]
	public record GroupInfo
	{
		[DataMember(IsRequired = true)]
		public string Identifier { get; set; }

		[DataMember(IsRequired = true)]
		public GroupDefinition Definition { get; set;  }
		
		[DataMember(IsRequired = true)]
		public ActionContext<string> RegistrationInfo { get; set; }
		
		[DataMember(IsRequired = true)]
		public ActionContext<string> LastUpdateInfo { get; set; }
	}
}