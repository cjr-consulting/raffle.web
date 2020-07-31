using Dapper;

using MediatR;

using Raffle.Core.Shared;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Commands
{
    public class UpdateOrderTicketCommand : INotification
    {
        public int OrderId { get; set; }
        public string TicketNumber { get; set; }
        public DateTime? DonationDate { get; set; }
        public string DonationNote { get; set; }
    }

    public class UpdateOrderTicketNumberCommandHandler : INotificationHandler<UpdateOrderTicketCommand>
    {
        readonly string connectionString;
        public UpdateOrderTicketNumberCommandHandler(RaffleDbConfiguration config)
        {
            connectionString = config.ConnectionString;
        }

        public async Task Handle(UpdateOrderTicketCommand notification, CancellationToken cancellationToken)
        {
            const string query = "UPDATE RaffleOrders SET " +
                "TicketNumber = @TicketNumber," +
                "UpdatedDate = @UpdatedDate," +
                "DonationDate = @DonationDate," +
                "DonationNote = @DonationNote " +
                "WHERE Id = @OrderId;";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(query, new
                {
                    notification.OrderId,
                    notification.TicketNumber,
                    UpdatedDate = DateTime.UtcNow,
                    notification.DonationDate,
                    notification.DonationNote
                });
            }
        }
    }
}
