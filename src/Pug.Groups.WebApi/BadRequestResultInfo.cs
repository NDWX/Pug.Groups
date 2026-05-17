using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pug.Groups.WebApi;

[DataContract]
public record BadRequestResultInfo
{
	public BadRequestResultInfo()
	{
	}

	public BadRequestResultInfo( string type, string requestArgument )
	{
		Type = type;
		RequestArgument = requestArgument;
		Message = string.Empty;
	}

	[Required(AllowEmptyStrings = true), DataMember]
	public string Message { get; init; }

	[Required(AllowEmptyStrings = true), DataMember]
	public string Type { get; init; }

	[Required(AllowEmptyStrings = true), DataMember]
	public string RequestArgument { get; init; }
}