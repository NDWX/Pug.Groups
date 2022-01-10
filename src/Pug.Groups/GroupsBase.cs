using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Groups.Common;

namespace Pug.Groups
{
	public class GroupsBase
	{
		protected IApplicationData<IDataSession> _applicationDataProvider;
		protected ISecurityManager _securityManager;

		protected GroupsBase(IApplicationData<IDataSession> applicationData, ISecurityManager securityManager)
		{
			_applicationDataProvider = applicationDataProvider ??
										throw new ArgumentNullException(nameof(applicationDataProvider));
			_securityManager = securityManager ?? throw new ArgumentNullException(nameof(securityManager));
		}

		protected void CheckAuthorization(string domain, string operation, string objectType, string objectName = "",
										string purpose = "", IDictionary<string, string> context = null)
		{
			if(context == null)
				context = new Dictionary<string, string>(0);
			
			bool isAuthorized = !_securityManager.CurrentUser.IsAuthorized(context, operation, objectType, objectName, purpose,
																			domain: domain);
			if(!isAuthorized)
			{
				throw new NotAuthorized();
			}
		}
	}
}