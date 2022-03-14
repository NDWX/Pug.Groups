using System;
using Microsoft.Extensions.DependencyInjection;
using Pug.Application.Data;
using Pug.Application.Security;

namespace Pug.Groups.Common
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddUserRoleProvider(this IServiceCollection serviceCollection, Options options, IdentifierGenerator identifierGenerator)
		{
			if(options == null) throw new ArgumentNullException(nameof(options));
			
			serviceCollection.AddSingleton(
					provider =>
					{
						IApplicationData<IDataSession> applicationData = 
							provider.GetService<IApplicationData<IDataSession>>();
						
						IUserRoleProvider userRoleProvider = new InternalUserRoleProvider(
								applicationData
							);

						return userRoleProvider;

					}
				);
			
			return serviceCollection;
		}
		
		public static IServiceCollection AddGroups(this IServiceCollection serviceCollection, Options options, IdentifierGenerator identifierGenerator)
		{
			if(options == null) throw new ArgumentNullException(nameof(options));
			
			serviceCollection.AddSingleton(
					// ReSharper disable once HeapView.ClosureAllocation
					// ReSharper disable once HeapView.DelegateAllocation
					provider =>
					{
						IApplicationData<IDataSession> applicationData = 
							provider.GetService<IApplicationData<IDataSession>>();
						
						ISecurityManager securityManager =
							provider.GetService<ISecurityManager>();

						IGroups groups = new Groups(identifierGenerator,
													applicationData, securityManager);

						return groups;

					}
				);
			
			return serviceCollection;
		}
	}
}