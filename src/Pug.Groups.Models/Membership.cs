using System;
using System.Runtime.Serialization;

namespace Pug.Groups.Models
{
	[DataContract]
	public record Membership : MembershipDefinition
	{
		[DataMember(IsRequired = true)]
		public DateTime AssignmentTimestamp { get; set; }
		
		[DataMember(IsRequired = true)]
		public string Assignor { get; set; }
	}
}