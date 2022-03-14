using System.Collections.Generic;
using System.Threading.Tasks;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	public interface IDataSession : Application.Data.IApplicationDataSession
	{
		void Insert(GroupInfo groupInfo);
		
		Task InsertAsync(GroupInfo groupInfo);

		Task<IEnumerable<GroupInfo>> GetGroupsAsync(string domain, string name);
		
		Task<GroupInfo> GetGroupDefinitionAsync(string identifier);
		
		Task<GroupInfo> GetGroupInfoAsync(string identifier);
		
		GroupInfo GetGroupInfo(string identifier);
		
		Task<IEnumerable<Membership>> GetMembershipsAsync(string identifier);
		
		IEnumerable<Membership> GetMemberships(string identifier);

		Task<IEnumerable<Membership>> GetMembershipsAsync(Subject subject, string domain = null);

		IEnumerable<Membership> GetMemberships(Subject subject, string domain = null);

		Task InsertAsync(Membership membership);

		Task DeleteAsync(string group, Subject subject);

		Task DeleteAsync(string group);
	}
}