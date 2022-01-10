using System;
using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Authorized;
using Pug.Authorized.Data;

namespace Pug.Groups.Common
{
	public class DefaultAuthorizationProviderFactory : IAuthorizationProviderFactory
	{
		private readonly IApplicationData<IAuthorizedDataStore> _authorizationDataStore;
		private readonly ISessionUserIdentityAccessor _sessionUserIdentityAccessor;

		public DefaultAuthorizationProviderFactory(
			IApplicationData<Authorized.Data.IAuthorizedDataStore> authorizationDataStore,
			ISessionUserIdentityAccessor sessionUserIdentityAccessor)
		{
			_authorizationDataStore = authorizationDataStore ?? throw new ArgumentNullException(nameof(authorizationDataStore));
			_sessionUserIdentityAccessor = sessionUserIdentityAccessor ?? throw new ArgumentNullException(nameof(sessionUserIdentityAccessor));
		}
		
		public IAuthorizationProvider Create(Authorized.Options options, IUserRoleProvider roleProvider)
		{
			return new Authorized.Authorized(
					options, 
					new DefaultIdentifierGenerator(), 
					_sessionUserIdentityAccessor, 
					roleProvider,
					_authorizationDataStore
				);
		}
	}
}