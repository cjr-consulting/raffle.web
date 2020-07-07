﻿using Dapper;

using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;

namespace Raffle.Core.Commands
{
    public class UpdateRaffleItemCommand : ICommand
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public string ItemValue { get; set; }
        public string Sponsor { get; set; }
        public int Cost { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateRaffleItemCommandHandler : ICommandHandler<UpdateRaffleItemCommand>
    {
        readonly string connectionString;
        public UpdateRaffleItemCommandHandler(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void Handle(UpdateRaffleItemCommand command)
        {
            const string query = "UPDATE [RaffleItems] SET" +
                " Title = @Title," +
                " Description = @Description," +
                " ImageUrl = @ImageUrl," +
                " Category = @Category," +
                " Sponsor = @Sponsor," +
                " ItemValue = @ItemValue," +
                " Cost = @Cost," +
                " IsAvailable = @IsAvailable" +
                "WHERE Id = @Id";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Execute(query, command);
            }
        }
    }
}
