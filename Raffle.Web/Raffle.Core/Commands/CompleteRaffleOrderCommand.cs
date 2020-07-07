using Dapper;

using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;

namespace Raffle.Core.Commands
{
    public class CompleteRaffleOrderCommand : ICommand
    {
        public int OrderId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    public class CompleteRaffleOrderCommandHandler : ICommandHandler<CompleteRaffleOrderCommand>
    {
        readonly string dbConnectionString;

        public CompleteRaffleOrderCommandHandler(string dbConnectionString)
        {
            this.dbConnectionString = dbConnectionString;
        }

        public void Handle(CompleteRaffleOrderCommand command)
        {
            using (var conn = new SqlConnection(dbConnectionString))
            {
                const string updateOrder = "UPDATE RaffleOrders SET " +
                    "Customer_FirstName = @FirstName, " +
                    "Customer_LastName = @LastName, " +
                    "Customer_Email = @Email, " +
                    "Customer_AddressLine1 = @AddressLine1, " +
                    "Customer_AddressLine2 = @AddressLine2, " +
                    "Customer_Address_City = @City, " +
                    "Customer_Address_State = @State, " +
                    "Customer_Address_Zip = @Zip " +
                    "WHERE Id = @OrderId";

                conn.Execute(updateOrder, command);
            }

            // Send Email to customer
            // Send Email to Gary
        }
    }
}
