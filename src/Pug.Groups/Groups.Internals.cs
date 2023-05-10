using System.Threading.Tasks;
using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Groups.Common;

namespace Pug.Groups
{
	public partial class Groups : GroupsBase
	{
		private readonly IdentifierGenerator _identifierGenerator;

		private static async Task<IGroup> _GetGroupAsync(IDataSession dataSession, string @group, IApplicationData<IDataSession> applicationData, ISecurityManager securityManager)
		{
			if(await dataSession.GetGroupInfoAsync(@group) == null)
				throw new UnknownGroupException(@group);

			IGroup grp = new Group(@group, applicationData, securityManager);
			
			return grp;
		}

		private static IGroup _GetGroup( IDataSession dataSession, string @group, IApplicationData<IDataSession> applicationData,
										ISecurityManager securityManager )
		{
			if( dataSession.GetGroupInfo( @group ) == null )
				throw new UnknownGroupException( @group );

			IGroup grp = new Group( @group, applicationData, securityManager );

			return grp;
		}

		private IGroup _GetGroup(string identifier)
		{
			return ApplicationDataProvider.Execute(
					function: (dataSession, context) =>
					{
						return _GetGroup(dataSession, context.identifier, context.@this.ApplicationDataProvider,
										context.@this.SecurityManager);
					},
					new { identifier, @this = this }
				);
		}
	}
}