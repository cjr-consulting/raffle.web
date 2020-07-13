using Dapper;

using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;

namespace Raffle.Core.Commands
{
    public class AddRaffleItemCommand : ICommand
    {
        public int ItemNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; }
        public int Order { get; set; }
        public string ItemValue { get; set; }
        public string Sponsor { get; set; }
        public int Cost { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool ForOver21 { get; set; }
        public bool LocalPickupOnly { get; set; }
    }

    public class AddRaffleItemCommandHandler : ICommandHandler<AddRaffleItemCommand>
    {
        readonly string connectionString;
        public AddRaffleItemCommandHandler(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void Handle(AddRaffleItemCommand command)
        {
            const string query = "INSERT INTO [RaffleItems] (ItemNumber, Title, Description, ImageUrl, Category, Sponsor, ItemValue, Cost, IsAvailable, ForOver21, LocalPickupOnly) VALUES " +
                "(@ItemNumber, @Title, @Description, @ImageUrl, @Category, @Sponsor, @ItemValue, @Cost, @IsAvailable, @ForOver21, @LocakPickupOnly)";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Execute(query, command);
            }
        }
    }
}
