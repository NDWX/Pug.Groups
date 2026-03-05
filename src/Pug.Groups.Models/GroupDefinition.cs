using System.Runtime.Serialization;

namespace Pug.Groups.Models
{
	[DataContract]
	public record GroupDefinition
	{
		[DataMember( IsRequired = true )]
		public string Domain
		{
			get;
#if NET5_0_OR_GREATER
			init;
#else
			set;
#endif
		}

		[DataMember(IsRequired = true)]
		public string Name
		{
			get;
#if NET5_0_OR_GREATER
			init;
#else
			set;
#endif
		}
		
		[DataMember(IsRequired = true)]
		public string Description
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