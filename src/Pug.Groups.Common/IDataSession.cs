using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	public interface IDataSession : Application.Data.IApplicationDataSession
	{
		void Insert(GroupInfo groupInfo);
		
		Task InsertAsync(GroupInfo groupInfo);

		Task<IEnumerable<GroupInfo>> GetGroupsAsync(string domain, string name);
		
		Task<GroupInfo> GetGroupInfoAsync(string identifier);
		
		GroupInfo GetGroupInfo(string identifier);
		
		Task<IEnumerable<DirectMembership>> GetMembershipsAsync(string identifier);
		
		IEnumerable<DirectMembership> GetMemberships(string identifier);

		Task<IEnumerable<DirectMembership>> GetMembershipsAsync(Subject subject, string domain = null);

		IEnumerable<DirectMembership> GetMemberships(Subject subject, string domain = null);

		Task InsertAsync(DirectMembership directMembership);

		Task DeleteAsync(string group, Subject subject);

		Task DeleteAsync(string group);
	}
}