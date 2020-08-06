using Dapper;

using MediatR;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Commands
{
    public class UpdateOrderCommand : INotification
    {
        public int OrderId { get; set; }
        public bool ReplaceAll { get; set; }
        public List<RaffleOrderItem> RaffleOrderItems { get; set; } = new List<RaffleOrderItem>();

        public class RaffleOrderItem
        {
            public int RaffleItemId { get; set; }
            public string Name { get; set; }
            public int Price { get; set; }
            public int Count { get; set; }
        }
    }

    public class UpdateOrderCommandHandler : INotificationHandler<UpdateOrderCommand>
    {
        readonly string connectionString;

        public UpdateOrderCommandHandler(RaffleDbConfiguration config)
        {
            connectionString = config.ConnectionString;
        }

        public async Task Handle(UpdateOrderCommand notification, CancellationToken cancellationToken)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    const string clearLineItemsQuery = "DELETE FROM RaffleOrderLineItems WHERE RaffleOrderId = @RaffleOrderId;";
                    const string clearLineItemQuery = "DELETE FROM RaffleOrderLineItems WHERE RaffleOrderId = @RaffleOrderId AND RaffleItemId = @RaffleItemId;";
                    const string upsertOrderItemQuery = "IF EXISTS(SELECT 1 FROM RaffleOrderLineItems WHERE RaffleItemId = @RaffleItemId AND RaffleOrderId = @RaffleOrderId) " +
                        "BEGIN " +
                        "  UPDATE RaffleOrderLineItems SET " +
                        "    Count = @Count " +
                        "  WHERE RaffleOrderId = @RaffleOrderId " +
                        "    AND RaffleItemId = @RaffleItemId; " +
                        "END " +
                        "ELSE " +
                        "BEGIN " +
                        "  INSERT INTO RaffleOrderLineItems " +
                        "  (RaffleOrderId, RaffleItemId, Name, Price, Count) VALUES " +
                        "  (@RaffleOrderId, @RaffleItemId, @Name, @Price, @Count); " +
                        "END ";

                    if (notification.ReplaceAll)
                    {
                        await conn.ExecuteAsync(clearLineItemsQuery, new { RaffleOrderId = notification.OrderId }, transaction);
                    }

                    foreach (var lineItem in notification.RaffleOrderItems)
                    {
                        if (lineItem.Count > 0)
                        {
                            await conn.ExecuteAsync(
                                upsertOrderItemQuery,
                                new
                                {
                                    RaffleOrderId = notification.OrderId,
                                    lineItem.RaffleItemId,
                                    lineItem.Name,
                                    lineItem.Price,
                                    lineItem.Count
                                },
                                transaction);
                        }
                        else
                        {
                            await conn.ExecuteAsync(
                                clearLineItemQuery,
                                new
                                {
                                    RaffleOrderId = notification.OrderId,
                                    lineItem.RaffleItemId
                                },
                                transaction);
                        }
                    }

                    transaction.Commit();
                }
            }
        }
    }
}
