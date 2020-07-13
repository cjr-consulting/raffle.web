using Raffle.Core.Models;
using Raffle.Core.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.Linq;

namespace Raffle.Core.Data
{
    public class RaffleItemRepository : IRaffleItemRepository
    {
        readonly string connectionString;

        public RaffleItemRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IReadOnlyList<RaffleItem> GetAll()
        {
            using (var conn = new SqlConnection(connectionString))
            {

                return conn.Query<RaffleItem>("SELECT * FROM RaffleItems ORDER BY [ItemNumber]").ToList();
            }
        }

        public RaffleItem GetById(int id)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<RaffleItem>("SELECT * FROM RaffleItems WHERE Id = @id" , new { id }).SingleOrDefault();
            }
        }
    }
}
