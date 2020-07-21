using Dapper;

using Raffle.Core.Shared;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Raffle.Core.Commands
{
    public class UpdateOrderTicketCommand : ICommand
    {
        public int OrderId { get; set; }
        public string TicketNumber { get; set; }
    }

    public class UpdateOrderTicketNumberCommandHandler : ICommandHandler<UpdateOrderTicketCommand>
    {
        readonly string connectionString;
        public UpdateOrderTicketNumberCommandHandler(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void Handle(UpdateOrderTicketCommand command)
        {
            const string query = "UPDATE RaffleOrders SET " +
                "TicketNumber = @TicketNumber," +
                "UpdatedDate = @TicketNumber " +
                "WHERE Id = @OrderId;";

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Execute(query, new
                {
                    command.OrderId,
                    command.TicketNumber
                });
            }
        }
    }
}
