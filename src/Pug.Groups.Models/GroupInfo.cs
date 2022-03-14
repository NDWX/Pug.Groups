using System.Runtime.Serialization;

namespace Pug.Groups.Models
{
	[DataContract]
	public record GroupInfo
	{
		[DataMember(IsRequired = true)]
		public string Identifier { get; set; }

		[DataMember(IsRequired = true)]
		public GroupDefinition Definition { get; set;  }
	}
}