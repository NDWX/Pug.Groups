using System.Runtime.Serialization;

namespace Pug.Groups.Models
{
	[DataContract]
	public record GroupDefinition
	{
		[DataMember(IsRequired = true)]
		public string Domain { get; set; }
		
		[DataMember(IsRequired = true)]
		public string Name { get; set; }
		
		[DataMember(IsRequired = true)]
		public string Description { get; set; }
	}
}