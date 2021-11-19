using System.Collections.Generic;
using System.Threading.Tasks;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	public interface IDataSession : Pug.Application.Data.IApplicationDataSession
	{
		Task<GroupInfo> GetGroupInfoAsync(string identifier);
		
		GroupInfo GetGroupInfo(string identifier);
		
		Task<IEnumerable<DirectMembership>> GetMembershipsAsync(string identifier);
		
		IEnumerable<DirectMembership> GetMemberships(string identifier);

		Task<IEnumerable<DirectMembership>> GetMembershipsAsync(Subject subject, string domain = null);

		IEnumerable<DirectMembership> GetMemberships(Subject subject, string domain = null);
	}
}