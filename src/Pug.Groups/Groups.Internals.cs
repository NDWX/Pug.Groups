using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Groups.Common;
using Pug.Groups.Models;

namespace Pug.Groups
{
	public partial class Groups : GroupsBase
	{
		private readonly IdentifierGenerator _identifierGenerator;

		private static async Task<IGroup> _GetGroupAsync(IDataSession dataSession, string @group, IApplicationData<IDataSession> applicationData, ISecurityManager securityManager)
		{
			GroupInfo groupInfo = (await dataSession.GetGroupInfoAsync(group).ConfigureAwait( false ));
			
			if(groupInfo is null)
				throw new UnknownGroupException(@group);

			IGroup grp = new Group(group, groupInfo.Definition.Domain, applicationData, securityManager);
			
			return grp;
		}
	}
}