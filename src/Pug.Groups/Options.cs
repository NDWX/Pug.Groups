namespace Pug.Groups.Common
{
	public class Options
	{
		public string AdministratorUser
		{
			get;
#if NET5_0_OR_GREATER
			init;
#else
			set;
#endif
		}

		public string AdministratorGroup
		{
			get;
#if NET5_0_OR_GREATER
			init;
#else
			set;
#endif
		}
	}
}