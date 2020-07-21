using Dapper;

using Raffle.Core.Models;
using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;
using System.Linq;

namespace Raffle.Core.Queries
{
    public class GetRaffleOrderQuery : IQuery
    {
        public int OrderId { get; set; }
    }

    public class GetRaffleOrderQueryHandler : IQueryHandler<GetRaffleOrderQuery, RaffleOrder>
    {
        readonly string dbConnectionString;

        public GetRaffleOrderQueryHandler(string dbConnectionString)
        {
            this.dbConnectionString = dbConnectionString;
        }

        public RaffleOrder Handle(GetRaffleOrderQuery query)
        {
            using (var conn = new SqlConnection(dbConnectionString))
            {
                const string getOrder = "SELECT Id=Ro.Id, ro.TicketNumber, ro.IsOrderConfirmed, " +
                    "Email = ro.Customer_Email, FirstName = ro.Customer_FirstName, LastName = ro.Customer_LastName, " +
                    "PhoneNumber = Customer_PhoneNumber, " +
                    "AddressLine1 = Customer_AddressLine1, " +
                    "AddressLine2 = Customer_AddressLine2, " +
                    "City = Customer_Address_City, " +
                    "State = Customer_Address_State, " +
                    "Zip = Customer_Address_Zip " +
                    " FROM RaffleOrders ro WHERE Id = @id";
                const string getOrderLineItems = "SELECT * FROM RaffleOrderLineItems WHERE RaffleOrderId = @id";

                var order = conn.Query<RaffleOrder, Customer, RaffleOrder>(
                    getOrder,
                    (raffleOrder, customer) =>
                    {
                        raffleOrder.Customer = customer;
                        return raffleOrder;
                    },
                    new { id = query.OrderId },
                    splitOn: "Email")
                    .Distinct()
                    .SingleOrDefault();

                order.Lines = conn.Query<RaffleOrderLine>(getOrderLineItems, new { id = query.OrderId }).ToList();
                return order;
            }
        }
    }
}
