using Microsoft.Extensions.DependencyInjection;
using Pug.Application.Security;

namespace Pug.Groups.Common
{
	public interface ISecurityProviderFactory
	{
		ISecurityManager AddSecurityManager(IServiceCollection serviceCollection,
											IUserRoleProvider userRoleProvider);
	}
}