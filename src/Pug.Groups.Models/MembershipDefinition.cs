using System.Runtime.Serialization;

namespace Pug.Groups.Models
{
	[DataContract]
	public record MembershipDefinition
	{
		[DataMember(IsRequired = true)]
		public Subject Subject
		{
			get;
#if NET5_0_OR_GREATER
			init;
#else
			set;
#endif
		}
		
		[DataMember(IsRequired = true)]
		public string Group
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