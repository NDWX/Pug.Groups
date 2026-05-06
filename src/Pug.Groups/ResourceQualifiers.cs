using Pug.Application.Security;

namespace Pug.Groups.Common;

public static class ResourceQualifiers
{
	public static Noun AnySubject => new Noun()
	{
		Type = SecurityObjectTypes.Subject
	};

	public static NounQualifier AnyGroup() => new NounQualifier()
	{
			Type = SecurityObjectTypes.Group
	};

	public static NounQualifier AnyGroup(string domain) => new NounQualifier()
	{
		Domain = domain,
			Type = SecurityObjectTypes.Group
	};
}