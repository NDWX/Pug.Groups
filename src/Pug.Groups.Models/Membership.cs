using System.Runtime.Serialization;
using Pug.Effable;

namespace Pug.Groups.Models
{
	[DataContract]
	public record Membership : MembershipDefinition
	{
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
	}
}