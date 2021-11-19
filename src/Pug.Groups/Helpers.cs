using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	internal static class Helpers
	{
		internal static ICollection<string> GetMemberships(Subject subject, string domain, IDataSession dataSession, List<string> evaluatedGroups = null)
		{
			if(evaluatedGroups == null)
				evaluatedGroups = new List<string>();
			
			List<string> roles = new List<string>();
			
			IEnumerable<DirectMembership> memberships = dataSession.GetMemberships(subject, null);
			
			foreach(DirectMembership membership in memberships)
			{
				if(evaluatedGroups.Contains(membership.Group))
					continue;
				
				GroupInfo groupInfo = dataSession.GetGroupInfo(membership.Group);

				if(groupInfo.Domain == domain)
				{
					roles.Add(groupInfo.Identifier);
				}
				
				evaluatedGroups.Add(groupInfo.Identifier);
				
				roles.AddRange(
						GetMemberships(new Subject() {Type = SubjectTypes.GROUP, Identifier = groupInfo.Identifier}, domain, dataSession, evaluatedGroups)
					);
			}

			return roles;
		}

		internal static async Task<bool> HasMemberAsync(string groupIdentifier, Subject subject, bool recursive, IDataSession dataSession, List<string> inspectedMemberGroups = null)
		{
			IEnumerable<DirectMembership> memberships = 
				await dataSession.GetMembershipsAsync(groupIdentifier)
								.ConfigureAwait(false);

			DirectMembership subjectMembership =
				memberships.FirstOrDefault(x => x.Subject == subject);

			if(subjectMembership != null)
				return true;

			if(!recursive)
				return false;

			if(inspectedMemberGroups == null)
				inspectedMemberGroups = new List<string>();

			IEnumerable<DirectMembership> groupMembers =
				memberships.Where(x => x.Subject.Type == SubjectTypes.GROUP);

			foreach(var groupMember in groupMembers)
			{
				if(inspectedMemberGroups.Contains(groupMember.Subject.Identifier))
					continue;
				
				// prevent current group from being inspected again in recursive inspection
				inspectedMemberGroups.Add(groupMember.Subject.Identifier);

				if(await HasMemberAsync(groupMember.Subject.Identifier, subject, recursive, dataSession,
										inspectedMemberGroups).ConfigureAwait(false))
					return true;
			}

			return false;
		}

		internal static bool GroupHasMember(string groupIdentifier, Subject subject, bool recursive, IDataSession dataSession, List<string> inspectedMemberGroups = null)
		{
			IEnumerable<DirectMembership> memberships =
				dataSession.GetMemberships(groupIdentifier);

			DirectMembership subjectMembership =
				memberships.FirstOrDefault(x => x.Subject == subject);

			if(subjectMembership != null)
				return true;

			if(!recursive)
				return false;

			if(inspectedMemberGroups == null)
				inspectedMemberGroups = new List<string>();

			IEnumerable<DirectMembership> groupMembers =
				memberships.Where(x => x.Subject.Type == SubjectTypes.GROUP);

			foreach(var groupMember in groupMembers)
			{
				if(inspectedMemberGroups.Contains(groupMember.Subject.Identifier))
					continue;
				
				// prevent current group from being inspected again in recursive inspection
				inspectedMemberGroups.Add(groupMember.Subject.Identifier);

				if(GroupHasMember(groupMember.Subject.Identifier, subject, recursive, dataSession,
							inspectedMemberGroups))
					return true;
			}

			return false;
		}
	}
}