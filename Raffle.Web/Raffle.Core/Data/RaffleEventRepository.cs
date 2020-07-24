using Dapper;

using Raffle.Core.Models.App;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Raffle.Core.Data
{
    public class RaffleEventRepository : IRaffleEventRepository
    {
        readonly string connectionString;

        public RaffleEventRepository(RaffleDbConfiguration config)
        {
            connectionString = config.ConnectionString;
        }

        public RaffleEvent GetById(int id)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.QueryFirst<RaffleEvent>("SELECT * FROM RaffleEvents WHERE Id = @Id", new { Id = id });
            }
        }
    }
}
