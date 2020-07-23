using Dapper;
using Raffle.Core.Models;
using Raffle.Core.Shared;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Raffle.Core.Queries
{
    public class GetRaffleOrdersQuery : IQuery<GetRaffleOrdersResult>
    {
    }

    public class GetRaffleOrdersResult
    {
        public IReadOnlyList<GetRaffleOrder> Orders { get; set; }
    }

    public class GetRaffleOrder
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public Customer Customer { get; set; }
        public int TotalTickets { get; set; }
        public int TotalPoints { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class GetRaffleOrdersQueryHandler : IQueryHandler<GetRaffleOrdersQuery, GetRaffleOrdersResult>
    {
        readonly string connectionString;
        public GetRaffleOrdersQueryHandler(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public GetRaffleOrdersResult Handle(GetRaffleOrdersQuery query)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                const string getOrder = "SELECT Id=Ro.Id, " +
                    "ro.TicketNumber, " +
                    "ro.IsOrderConfirmed, " +
                    "TotalTickets = (SELECT SUM(Count) FROM RaffleOrderLineItems WHERE RaffleOrderId = ro.Id), " +
                    "TotalPoints = (SELECT SUM(Count * Price) FROM RaffleOrderLineItems WHERE RaffleOrderId = ro.Id), " +
                    "StartDate," +
                    "CompletedDate," +

                    "Email = ro.Customer_Email, " +
                    "FirstName = ro.Customer_FirstName, " +
                    "LastName = ro.Customer_LastName, " +
                    "PhoneNumber = Customer_PhoneNumber, " +
                    "AddressLine1 = Customer_AddressLine1, " +
                    "AddressLine2 = Customer_AddressLine2, " +
                    "City = Customer_Address_City, " +
                    "State = Customer_Address_State, " +
                    "Zip = Customer_Address_Zip " +
                    "FROM RaffleOrders ro " +
                    "WHERE CompletedDate IS NOT NULL;";

                var orders = conn.Query<GetRaffleOrder, Customer, GetRaffleOrder>(
                    getOrder,
                    (raffleOrder, customer) =>
                    {
                        raffleOrder.Customer = customer;
                        return raffleOrder;
                    },
                    null,
                    splitOn: "Email")
                    .Distinct()
                    .ToList();

                return new GetRaffleOrdersResult
                    {
                        Orders = orders
                    };
            }
        }
    }
}
