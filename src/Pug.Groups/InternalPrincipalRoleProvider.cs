using Pug.Application.Data;
using Pug.Application.Data.Extensions;
using Pug.Application.Security;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	public class InternalPrincipalRoleProvider : IPrincipalRoleProvider
	{
		private readonly IApplicationData<IDataSession> _applicationData;

		public InternalPrincipalRoleProvider(IApplicationData<IDataSession> applicationData)
		{
			_applicationData = applicationData;
		}

		public bool PrincipalIsInRole(string principal, string role)
		{
			return _applicationData.Execute(
					(dataSession, context) =>
					{
						return Helpers.GroupHasMember(
							context.role,
							new Subject()
								{ Identifier = context.principal, Type = SubjectTypes.USER },
							true, dataSession);
					},
					new { principal, role }
				);
		}

		public Task<bool> PrincipalIsInRoleAsync( string principal, string role )
		{
			return _applicationData.ExecuteAsync(
					(dataSession, context) =>
					{
						return Helpers.GroupHasMemberAsync(
							context.role,
							new Subject()
								{ Identifier = context.principal, Type = SubjectTypes.USER },
							true, dataSession);
					},
					new { principal, role }
				);
		}

		public bool PrincipalIsInRoles(string principal, ICollection<string> roles)
		{
			return _applicationData.Execute(
					(dataSession, context) =>
					{
						foreach(string role in context.roles)
						{
							if(!Helpers.GroupHasMember(
									role,
									new Subject()
										{ Identifier = context.principal, Type = SubjectTypes.USER },
									true, dataSession))
								return false;
						}

						return true;
					},
					new { principal, roles }
				);
		}

		public Task<bool> PrincipalIsInRolesAsync( string principal, ICollection<string> roles )
		{
			return _applicationData.ExecuteAsync(
					async (dataSession, context) =>
					{
						foreach(string role in context.roles)
						{
							if( !await Helpers.GroupHasMemberAsync(
									role,
									new Subject()
										{ Identifier = context.principal, Type = SubjectTypes.USER },
									true, dataSession).ConfigureAwait( false ) )
								return false;
						}

						return true;
					},
					new { principal, roles }
				);
		}

		public IEnumerable<string> GetPrincipalRoles(string principal)
		{
			return _applicationData.Execute(
					(dataSession, context) =>
					{
						return Helpers.GetMemberships(
							new Subject() { Identifier = context.principal, Type = SubjectTypes.USER },
							dataSession);
					},
					new { principal }
				);
		}

		public Task<IEnumerable<string>> GetPrincipalRolesAsync( string principal )
		{
			return _applicationData.ExecuteAsync(
					async ( dataSession, context ) =>
					{
						return ( await
									Helpers.GetMembershipsAsync(
										new Subject() { Identifier = context.principal, Type = SubjectTypes.USER },
										null,
										dataSession ).ConfigureAwait( false )
								).Select( x => x.Group )
								.Distinct();
					},
					new { principal }
				);
		}
	}
}