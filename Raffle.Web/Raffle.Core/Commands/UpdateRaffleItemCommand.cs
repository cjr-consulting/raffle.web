using Dapper;

using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;

namespace Raffle.Core.Commands
{
    public class UpdateRaffleItemCommand : ICommand
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
    }

    public class UpdateRaffleItemCommandHandler : ICommandHandler<UpdateRaffleItemCommand>
    {
        readonly string connectionString;
        public UpdateRaffleItemCommandHandler(RaffleDbConfiguration config)
        {
            connectionString = config.ConnectionString;
        }

        public void Handle(UpdateRaffleItemCommand command)
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
                " NumberOfDraws = @NumberOfDraws " +
                "WHERE Id = @Id";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Execute(query, command);
            }
        }
    }
}
