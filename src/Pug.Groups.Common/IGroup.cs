using System.Collections.Generic;
using System.Threading.Tasks;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	public interface IGroup
	{
		string Identifier { get; }
		
		Task<GroupInfo> GetInfoAsync();
		
		GroupInfo GetInfo();

		Task<IEnumerable<DirectMembership>> GetMembershipsAsync();
		
		Task<bool> HasMemberAsync(Subject subject, bool recursive = false);

		Task AddMembersAsync(IEnumerable<Subject> subjects);
		
		Task RemoveMemberAsync(Subject subject);
	}
}