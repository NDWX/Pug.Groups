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
		
		Task<GroupDefinition> GetGroupDefinitionAsync(string identifier);
		
		Task<GroupInfo> GetGroupInfoAsync(string identifier);
		
		GroupInfo GetGroupInfo(string identifier);
		
		Task<IEnumerable<Membership>> GetMembershipsAsync(string group);
		
		IEnumerable<Membership> GetMemberships(string group);

		Task<IEnumerable<Membership>> GetMembershipsAsync(Subject subject, string domain = null);

		IEnumerable<Membership> GetMemberships(Subject subject, string domain = null);

		Membership GetMembership( Subject subject, string group );

		Task<Membership> GetMembershipAsync( Subject subject, string group );

		Task InsertAsync(Membership membership);

		Task DeleteAsync(string group, Subject subject);

		Task DeleteAsync(string group);
	}
}