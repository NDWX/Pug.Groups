using Pug.Application.Security;

namespace Pug.Groups.Common
{
	public interface IAuthorizationProviderFactory
	{
		IAuthorizationProvider Create(Authorized.Options options, IUserRoleProvider roleProvider);
	}
}