using Dapper;

using MediatR;

using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Commands
{
    public class UpdateRaffleItemCommand : INotification
    {
        public int Id { get; set; }
        public int ItemNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; }
        public int Order { get; set; }
        public string ItemValue { get; set; }
        public string Sponsor { get; set; } = string.Empty;
        public int Cost { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool ForOver21 { get; set; } = true;
        public bool LocalPickupOnly { get; set; } = true;
        public int NumberOfDraws { get; set; } = 1;
        public string WinningTickets { get; set; }
    }

    public class UpdateRaffleItemCommandHandler : INotificationHandler<UpdateRaffleItemCommand>
    {
        readonly string connectionString;
        public UpdateRaffleItemCommandHandler(RaffleDbConfiguration config)
        {
            connectionString = config.ConnectionString;
        }

        public async Task Handle(UpdateRaffleItemCommand notification, CancellationToken cancellationToken)
        {
            const string query = "UPDATE [RaffleItems] SET" +
                " ItemNumber = @ItemNumber, " +
                " Title = @Title," +
                " Description = @Description," +
                " ImageUrl = @ImageUrl," +
                " Category = @Category," +
                " Sponsor = @Sponsor," +
                " ItemValue = @ItemValue," +
                " Cost = @Cost," +
                " IsAvailable = @IsAvailable, " +
                " ForOver21 = @ForOver21, " +
                " LocalPickupOnly = @LocalPickupOnly," +
                " NumberOfDraws = @NumberOfDraws," +
                " WinningTickets = @WinningTickets " +
                "WHERE Id = @Id";
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(query, notification);
            }
        }
    }
}
