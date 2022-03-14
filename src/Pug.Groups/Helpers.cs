using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	internal static class Helpers
	{
		internal static void ValidateParameter(Subject subject, string paramName)
		{
			if(!SubjectTypes.Contains(subject.Type))
				throw new ArgumentOutOfRangeException(paramName, subject.Type,
													$"Subject type '{subject.Type}' is not valid.");

			if(string.IsNullOrEmpty(subject.Identifier))
				throw new ArgumentException("Subject identifier must be specified", paramName);
		}
		
		internal static ICollection<string> GetMemberships(Subject subject, string domain, IDataSession dataSession, List<string> evaluatedGroups = null)
		{
			if(evaluatedGroups == null)
				evaluatedGroups = new List<string>();
			
			List<string> roles = new List<string>();
			
			IEnumerable<Membership> memberships = dataSession.GetMemberships(subject);
			
			foreach(string group in memberships.Select(x => x.Group))
			{
				if(evaluatedGroups.Contains(group))
					continue;
				
				GroupInfo groupInfo = dataSession.GetGroupInfo(group);

				if(groupInfo.Definition.Domain == domain)
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
		
		internal static async Task<ICollection<Membership>> GetMembershipsAsync(Subject subject, string domain, IDataSession dataSession, List<string> evaluatedGroups = null)
		{
			if(evaluatedGroups == null)
				evaluatedGroups = new List<string>();
			
			List<Membership> roles = new List<Membership>();
			
			IEnumerable<Membership> memberships = await  dataSession.GetMembershipsAsync(subject);
			
			foreach(Membership membership in memberships)
			{
				if(evaluatedGroups.Contains(membership.Group))
					continue;
				
				GroupInfo groupInfo = await dataSession.GetGroupInfoAsync(membership.Group);

				if(groupInfo.Definition.Domain == domain)
				{
					roles.Add(membership);
				}
				
				evaluatedGroups.Add(groupInfo.Identifier);
				
				roles.AddRange(
						await GetMembershipsAsync(new Subject() {Type = SubjectTypes.GROUP, Identifier = groupInfo.Identifier}, domain, dataSession, evaluatedGroups)
					);
			}

			return roles;
		}

		internal static async Task<bool> GroupHasMemberAsync(string groupIdentifier, Subject subject, bool recursive, IDataSession dataSession, List<string> inspectedMemberGroups = null)
		{
			IEnumerable<Membership> memberships = 
				await dataSession.GetMembershipsAsync(groupIdentifier)
								.ConfigureAwait(false);

			Membership subjectMembership =
				memberships.FirstOrDefault(x => x.Subject == subject);

			if(subjectMembership != null)
				return true;

			if(!recursive)
				return false;

			if(inspectedMemberGroups == null)
				inspectedMemberGroups = new List<string>();

			IEnumerable<Membership> groupMembers =
				memberships.Where(x => x.Subject.Type == SubjectTypes.GROUP);

			foreach(Subject member in groupMembers.Select(x => x.Subject))
			{
				if(inspectedMemberGroups.Contains(member.Identifier))
					continue;
				
				// prevent current group from being inspected again in recursive inspection
				inspectedMemberGroups.Add(member.Identifier);

				if(await GroupHasMemberAsync(member.Identifier, subject, recursive, dataSession,
										inspectedMemberGroups).ConfigureAwait(false))
					return true;
			}

			return false;
		}

		internal static bool GroupHasMember(string groupIdentifier, Subject subject, bool recursive, IDataSession dataSession, List<string> inspectedMemberGroups = null)
		{
			IEnumerable<Membership> memberships =
				dataSession.GetMemberships(groupIdentifier);

			Membership subjectMembership =
				memberships.FirstOrDefault(x => x.Subject == subject);

			if(subjectMembership != null)
				return true;

			if(!recursive)
				return false;

			if(inspectedMemberGroups == null)
				inspectedMemberGroups = new List<string>();

			IEnumerable<Membership> groupMembers =
				memberships.Where(x => x.Subject.Type == SubjectTypes.GROUP);

			foreach(Subject member in groupMembers.Select(x => x.Subject))
			{
				if(inspectedMemberGroups.Contains(member.Identifier))
					continue;
				
				// prevent current group from being inspected again in recursive inspection
				inspectedMemberGroups.Add(member.Identifier);

				if(GroupHasMember(member.Identifier, subject, recursive, dataSession,
							inspectedMemberGroups))
					return true;
			}

			return false;
		}
		
	}
}