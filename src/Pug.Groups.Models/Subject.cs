using System.Runtime.Serialization;

namespace Pug.Groups.Models
{
	[DataContract]
	public record Subject
	{
		[DataMember(IsRequired = true)]
		public string Type { get; set; }
		
		[DataMember(IsRequired = true)]
		public string Identifier { get; set; }
	}
}