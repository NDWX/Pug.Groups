using System.Data;
using System.Data.Common;
using Pug.Application.Data;
using Pug.Groups.Common;

namespace Pug.Groups.PostgresData
{
	public class DapperData : ApplicationData<IDataSession>
	{
		public DapperData( string location, DbProviderFactory dataProvider ) : base( location, dataProvider )
		{
		}

		protected override IDataSession CreateApplicationDataSession( IDbConnection databaseSession, DbProviderFactory dataAccessProvider )
		{
			return new DapperDataSession( databaseSession );
		}
	}
}