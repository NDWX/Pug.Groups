using System.Runtime.Serialization;

namespace Pug.Groups.Models
{
	[DataContract]
	public record MembershipDefinition
	{
		[DataMember(IsRequired = true)]
		public Subject Subject { get; set; }
		
		[DataMember(IsRequired = true)]
		public string Group { get; set; }
	}
}