using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
					new { user, role }
				);
		}

		public Task<bool> UserIsInRoleAsync( string user, string role )
		{
			return _applicationData.ExecuteAsync(
					(dataSession, context) =>
					{
						return Helpers.GroupHasMemberAsync(
							context.role,
							new Subject()
								{ Identifier = context.user, Type = Authorized.SubjectTypes.User },
							true, dataSession);
					},
					new { user, role }
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
					new { user, roles }
				);
		}

		public Task<bool> UserIsInRolesAsync( string user, ICollection<string> roles )
		{
			return _applicationData.ExecuteAsync(
					async (dataSession, context) =>
					{
						foreach(string role in context.roles)
						{
							if( !await Helpers.GroupHasMemberAsync(
									role,
									new Subject()
										{ Identifier = context.user, Type = Authorized.SubjectTypes.User },
									true, dataSession) )
								return false;
						}

						return true;
					},
					new { user, roles }
				);
		}

		public IEnumerable<string> GetUserRoles(string user, string domain)
		{
			return _applicationData.Execute(
					(dataSession, context) =>
					{
						return Helpers.GetMemberships(
							new Subject() { Identifier = context.user, Type = SubjectTypes.USER }, context.domain,
							dataSession);
					},
					new { user, domain }
				);
		}

		public Task<IEnumerable<string>> GetUserRolesAsync( string user, string domain )
		{
			return _applicationData.ExecuteAsync(
					async ( dataSession, context ) =>
					{
						return ( await
									Helpers.GetMembershipsAsync(
										new Subject() { Identifier = context.user, Type = SubjectTypes.USER }, context.domain,
										dataSession )
								).Select( x => x.Group )
								.Distinct();
					},
					new { user, domain }
				);
		}
	}
}