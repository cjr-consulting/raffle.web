using Dapper;

using Raffle.Core.Shared;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Raffle.Core.Commands
{
    public class UpdateOrderCommand : ICommand
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

    public class UpdateOrderCommandHandler : ICommandHandler<UpdateOrderCommand>
    {
        readonly string connectionString;

        public UpdateOrderCommandHandler(RaffleDbConfiguration config)
        {
            this.connectionString = config.ConnectionString;
        }

        public void Handle(UpdateOrderCommand command)
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

                    if (command.ReplaceAll)
                    {
                        conn.Execute(clearLineItemsQuery, new { RaffleOrderId = command.OrderId }, transaction);
                    }

                    foreach (var lineItem in command.RaffleOrderItems)
                    {
                        if (lineItem.Count > 0)
                        {
                            conn.Execute(
                                upsertOrderItemQuery,
                                new
                                {
                                    RaffleOrderId = command.OrderId,
                                    lineItem.RaffleItemId,
                                    lineItem.Name,
                                    lineItem.Price,
                                    lineItem.Count
                                },
                                transaction);
                        }
                        else
                        {
                            conn.Execute(
                                clearLineItemQuery,
                                new
                                {
                                    RaffleOrderId = command.OrderId,
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
