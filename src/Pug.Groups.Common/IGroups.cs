using System.Collections.Generic;
using System.Threading.Tasks;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	public interface IGroups
	{
		Task<IEnumerable<GroupInfo>> GetGroupsAsync(string domain, Subject subject = null, bool recursive = false);

		Task AddToGroupsAsync(Subject subject, string domain, IEnumerable<string> groups);

		Task<bool> SubjectIsMemberAsync(Subject subject, string domain, string group);
	}

	public interface IGroup
	{
		Task<GroupInfo> GetInfoAsync();
		
		Task<bool> HasMemberAsync(Subject subject, bool recursive = false);

		Task AddMembersAsync(IEnumerable<Subject> subject);
	}
}