using System.Collections.Generic;
using System.Transactions;
using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	internal class InternalUserRoleProvider : IUserRoleProvider
	{
		private readonly IApplicationData<IDataSession> _applicationData;

		public InternalUserRoleProvider(IApplicationData<IDataSession> applicationData)
		{
			_applicationData = applicationData;
		}
		
		public bool UserIsInRole(string user, string role)
		{
			return _applicationData.Execute(
					(dataSession, context) =>
					{
						return Helpers.GroupHasMember(
							context.role,
							new Subject()
								{ Identifier = context.user, Type = Authorized.SubjectTypes.User },
							true, dataSession);
					},
					new { user, role },
					TransactionScopeOption.Required,
					new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
				);
		}

		public bool UserIsInRoles(string user, ICollection<string> roles)
		{
			return _applicationData.Execute(
					(dataSession, context) =>
					{
						foreach(string role in context.roles)
						{
							if(!Helpers.GroupHasMember(
									role,
									new Subject()
										{ Identifier = context.user, Type = Authorized.SubjectTypes.User },
									true, dataSession))
								return false;
						}

						return true;
					},
					new { user, roles },
					TransactionScopeOption.Required,
					new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
				);
		}
		
		public ICollection<string> GetUserRoles(string user, string domain)
		{
			return _applicationData.Execute(
					(dataSession, context) =>
					{
						return Helpers.GetMemberships(
							new Subject() { Identifier = context.user, Type = SubjectTypes.USER }, context.domain,
							dataSession);
					},
					new { user, domain },
					TransactionScopeOption.Required,
					new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }
				);
		}
	}
}