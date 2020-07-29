using Dapper;

using MediatR;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Queries
{
    public class RaffleOrderExistsQuery : IRequest<bool>
    {
        public int OrderId { get; set; }
    }

    public class RaffleOrderExistsQueryHandler : IRequestHandler<RaffleOrderExistsQuery, bool>
    {
        private readonly string connectionString;

        public RaffleOrderExistsQueryHandler(RaffleDbConfiguration config)
        {
            connectionString = config.ConnectionString;
        }

        public async Task<bool> Handle(RaffleOrderExistsQuery request, CancellationToken cancellationToken)
        {
            using (var conn = new SqlConnection(connectionString))
            {
               return await conn.ExecuteScalarAsync<bool>("SELECT 1 FROM RaffleOrders WHERE ID = @id", new { id = request.OrderId });
            }
        }
    }
}
