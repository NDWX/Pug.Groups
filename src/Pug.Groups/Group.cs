using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Authorized;
using Pug.Groups.Models;

namespace Pug.Groups.Common
{
	public class Group :  IGroup
	{
		private readonly string _identifier;
		private readonly IApplicationData<IDataSession> _applicationDataProvider;
		private readonly IAuthorized _authorized;

		public Group(string identifier, ISessionUserIdentityAccessor sessionUserIdentityAccessor, IApplicationData<IDataSession> applicationDataProvider, IAuthorized authorized)
		{
			if(string.IsNullOrWhiteSpace(identifier))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(identifier));
			
			_identifier = identifier;
			_applicationDataProvider = applicationDataProvider ?? throw new ArgumentNullException(nameof(applicationDataProvider));
			_authorized = authorized;
		}
		
		public async Task<GroupInfo> GetInfoAsync()
		{
			return await _applicationDataProvider.ExecuteAsync(
							async (dataSession, context) =>
							{
								return await dataSession.GetGroupInfoAsync(context.@this._identifier)
														.ConfigureAwait(false);
							},
							context: new {@this = this},
							TransactionScopeOption.Required,
							new TransactionOptions(){IsolationLevel = IsolationLevel.ReadCommitted}
							).ConfigureAwait(false);
		}
		
		public async Task<IEnumerable<DirectMembership>> GetMemberships()
		{
			return await _applicationDataProvider.ExecuteAsync(
							async (dataSession, context) =>
							{
								return await dataSession.GetMembershipsAsync(context.@this._identifier)
														.ConfigureAwait(false);
							},
							context: new {@this = this},
							TransactionScopeOption.Required,
							new TransactionOptions(){IsolationLevel = IsolationLevel.ReadCommitted}
						).ConfigureAwait(false);
		}

		public async Task<bool> HasMemberAsync(Subject subject, bool recursive = false)
		{
			return await _applicationDataProvider.ExecuteAsync(
							async (dataSession, context) =>
							{
								return await Helpers.HasMemberAsync(context.@this._identifier, context.subject,
																	context.recursive, dataSession).ConfigureAwait(false);
							},
							context: new {@this = this, subject, recursive},
							TransactionScopeOption.Required,
							new TransactionOptions(){IsolationLevel = IsolationLevel.ReadCommitted}
						).ConfigureAwait(false);
		}

		public async Task AddMembersAsync(IEnumerable<Subject> subject)
		{
			throw new System.NotImplementedException();
		}
	}
}