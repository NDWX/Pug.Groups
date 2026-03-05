using System.Runtime.Serialization;
using Pug.Effable;

namespace Pug.Groups.Models
{
	[DataContract]
	public record GroupInfo
	{
		[DataMember(IsRequired = true)]
		public string Identifier
		{
			get;
#if NET5_0_OR_GREATER
			init;
#else
			set;
#endif
		}

		[DataMember(IsRequired = true)]
		public GroupDefinition Definition { get; set;  }
		
		[DataMember(IsRequired = true)]
		public ActionContext<string> RegistrationInfo
		{
			get;
#if NET5_0_OR_GREATER
			init;
#else
			set;
#endif
		}
		
		[DataMember(IsRequired = true)]
		public ActionContext<string> LastUpdateInfo
		{
			get;
#if NET5_0_OR_GREATER
			init;
#else
			set;
#endif
		}
	}
}