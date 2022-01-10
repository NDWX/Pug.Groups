using System;
using System.Runtime.Serialization;

namespace Pug.Groups.Common
{
	[Serializable]
	public class UnknownGroupException : Exception
	{
		private const string GroupIdentifierField = "GroupIdentifier";
		public string GroupIdentifier { get; }

		public UnknownGroupException(string groupIdentifier)
		{
			GroupIdentifier = groupIdentifier;
		}

		protected UnknownGroupException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			GroupIdentifier = info.GetString(GroupIdentifierField);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(GroupIdentifierField, GroupIdentifier);
		}
	}
}