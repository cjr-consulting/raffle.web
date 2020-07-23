using Dapper;

using Raffle.Core.Shared;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Raffle.Core.Queries
{
    public class StartRaffleOrderQuery : IQuery<int>
    {
        public List<RaffleOrderItem> RaffleOrderItems { get; set; } = new List<RaffleOrderItem>();

        public class RaffleOrderItem
        {
            public int RaffleItemId { get; set; }
            public string Name { get; set; }
            public int Price { get; set; }
            public int Count { get; set; }
        }
    }

    public class StartRaffleOrderQueryHandler : IQueryHandler<StartRaffleOrderQuery, int>
    {
        readonly string connectionString;

        public StartRaffleOrderQueryHandler(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int Handle(StartRaffleOrderQuery command)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    const string addOrderQuery = "INSERT INTO RaffleOrders (StartDate) VALUES (@StartDate); SELECT SCOPE_IDENTITY();";
                    const string addOrderItemQuery = "INSERT INTO RaffleOrderLineItems " +
                        "(RaffleOrderId, RaffleItemId, Name, Price, Count) VALUES " +
                        "(@RaffleOrderId, @RaffleItemId, @Name, @Price, @Count);";

                    var orderId = conn.ExecuteScalar<int>(addOrderQuery, new { StartDate = DateTime.UtcNow }, transaction);
                    foreach (var lineItem in command.RaffleOrderItems)
                    {
                        conn.Execute(
                            addOrderItemQuery,
                            new
                            {
                                RaffleOrderId = orderId,
                                lineItem.RaffleItemId,
                                lineItem.Name,
                                lineItem.Price,
                                lineItem.Count
                            },
                            transaction);
                    }

                    transaction.Commit();

                    return orderId;
                }
            }
        }
    }
}
