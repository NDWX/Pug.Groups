using System;
using System.Collections.Generic;
using Pug.Application.Data;
using Pug.Application.Security;
using Pug.Groups.Common;

namespace Pug.Groups
{
	public class GroupsBase
	{
		protected IApplicationData<IDataSession> ApplicationDataProvider { get; }
		protected ISecurityManager SecurityManager { get; }

		protected GroupsBase(IApplicationData<IDataSession> applicationData, ISecurityManager securityManager)
		{
			ApplicationDataProvider = applicationData ??
										throw new ArgumentNullException(nameof(applicationData));
			SecurityManager = securityManager ?? throw new ArgumentNullException(nameof(securityManager));
		}

		protected void CheckAuthorization(string domain, string operation, string objectType, string objectName = "",
										string purpose = "", IDictionary<string, string> context = null)
		{
			if(context == null)
				context = new Dictionary<string, string>(0);
			
			bool isAuthorized = !SecurityManager.CurrentUser.IsAuthorized(context, operation, objectType, objectName, purpose,
																			domain: domain);
			if(!isAuthorized)
			{
				throw new NotAuthorized();
			}
		}
	}
}