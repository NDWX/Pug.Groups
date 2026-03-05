using System.Runtime.Serialization;

namespace Pug.Groups.Models
{
	[DataContract]
	public record Subject
	{
		[DataMember(IsRequired = true)]
		public string Type
		{
			get;
#if NET5_0_OR_GREATER
			init;
#else
			set;
#endif
		}
		
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
	}
}