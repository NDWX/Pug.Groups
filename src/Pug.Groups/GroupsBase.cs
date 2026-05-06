using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Groups.Common;

namespace Pug.Groups
{
	public abstract class GroupsBase
	{
		protected IApplicationData<IDataSession> ApplicationDataProvider { get; }
		protected ISecurityManager SecurityManager { get; }

		protected GroupsBase( IApplicationData<IDataSession> applicationData,
							ISecurityManager securityManager )
		{
			ApplicationDataProvider = applicationData ??
									throw new ArgumentNullException( nameof(applicationData) );

			SecurityManager = securityManager ?? throw new ArgumentNullException( nameof(securityManager) );
		}

		protected async Task CheckAuthorizationAsync( string operation, 
													NounQualifier domainObject, string purpose = "",
													IDictionary<string, string> context = null )
		{
			if( context == null )
				context = new Dictionary<string, string>( 0 );

			bool isAuthorized = await SecurityManager.CurrentUser.IsAuthorizedAsync(
														context,
														operation,
														domainObject,
														purpose
													)
													.ConfigureAwait( false );

			if( !isAuthorized )
				throw new NotAuthorized();

		}
	}
}