using System.Collections.Generic;

namespace Pug.Groups.Common
{
	public static class SubjectTypes
	{
		public static readonly string GROUP = "GROUP";
		public static readonly string USER = "USER";

		private static readonly List<string> List = new List<string>(new[] { GROUP, USER });

		public static bool Contains(string type)
		{
			return List.Contains(type);
		}
	}
}