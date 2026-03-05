using System;
using Microsoft.Extensions.DependencyInjection;
using Pug.Application.Data;
using Pug.Application.Security;

namespace Pug.Groups.Common
{
	public static class IServiceCollectionExtensions
	{
		/// <summary>
		/// Registers instance of IUSerRoleProvider service required by Pug.Application.Security.SecurityManager.
		/// </summary>
		/// <param name="serviceCollection"></param>
		/// <param name="options"></param>
		/// <param name="identifierGenerator"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
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
		
		/// <summary>
		/// Registers instance of Groups management service. 
		/// </summary>
		/// <param name="serviceCollection"></param>
		/// <param name="options"></param>
		/// <param name="identifierGenerator"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
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