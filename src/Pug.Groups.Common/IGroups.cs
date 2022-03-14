using System.Collections.Generic;
using System.Threading.Tasks;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	public interface IGroups
	{
		Task<string> AddGroupAsync( GroupDefinition definition );
		
		Task<IEnumerable<GroupInfo>> GetGroupsAsync(string domain, string name = null);

		Task<IGroup> GetGroupAsync(string identifier);
		
		Task DeleteGroupAsync(string identifier);
		
		Task<IEnumerable<Membership>> GetMemberships(string domain, Subject subject, bool recursive = false);

		Task AddToGroupsAsync(Subject subject, IEnumerable<string> groups);
	}
}