using Dapper;

using Raffle.Core.Models;
using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;
using System.Linq;

namespace Raffle.Core.Commands
{
    public class GetRaffleOrderQuery : IQuery
    {
        public int OrderId { get; set; }
    }

    public class GetRaffleOrderQueryHandler : IQueryHandler<GetRaffleOrderQuery, RaffleOrder>
    {
        readonly string dbConnectionString;

        public GetRaffleOrderQueryHandler(string dbConnectionString)
        {
            this.dbConnectionString = dbConnectionString;
        }

        public RaffleOrder Handle(GetRaffleOrderQuery query)
        {
            using (var conn = new SqlConnection(dbConnectionString))
            {
                const string getOrder = "SELECT * FROM RaffleOrders WHERE Id = @id";
                const string getOrderLineItems = "SELECT * FROM RaffleOrderLineItems WHERE RaffleOrderId = @id";

                var order = conn.Query<RaffleOrder>(getOrder, new { id = query.OrderId }).SingleOrDefault();
                order.Lines = conn.Query<RaffleOrderLine>(getOrderLineItems, new { id = query.OrderId }).ToList();
                return order;
            }
        }
    }
}
