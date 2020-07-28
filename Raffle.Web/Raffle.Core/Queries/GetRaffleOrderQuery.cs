using Dapper;

using MediatR;

using Raffle.Core.Models;
using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Queries
{
    public class GetRaffleOrderQuery : IRequest<RaffleOrder>
    {
        public int OrderId { get; set; }
    }

    public class GetRaffleOrderQueryHandler : IRequestHandler<GetRaffleOrderQuery, RaffleOrder>
    {
        readonly string connectionString;

        public GetRaffleOrderQueryHandler(RaffleDbConfiguration config)
        {
            connectionString = config.ConnectionString;
        }

        public async Task<RaffleOrder> Handle(GetRaffleOrderQuery request, CancellationToken cancellationToken)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                const string getOrder = "SELECT " +
                    "Id=Ro.Id, " +
                    "ro.TicketNumber, " +
                    "ro.IsOrderConfirmed, " +
                    "StartDate, " +
                    "CompletedDate, " +
                    "UpdatedDate, " +
                    "Email = ro.Customer_Email, FirstName = ro.Customer_FirstName, LastName = ro.Customer_LastName, " +
                    "PhoneNumber = Customer_PhoneNumber, " +
                    "AddressLine1 = Customer_AddressLine1, " +
                    "AddressLine2 = Customer_AddressLine2, " +
                    "City = Customer_Address_City, " +
                    "State = Customer_Address_State, " +
                    "Zip = Customer_Address_Zip " +
                    " FROM RaffleOrders ro WHERE Id = @id";
                const string getOrderLineItems = "SELECT * FROM RaffleOrderLineItems WHERE RaffleOrderId = @id AND Count > 0;";

                var order = (await conn.QueryAsync<RaffleOrder, Customer, RaffleOrder>(
                    getOrder,
                    (raffleOrder, customer) =>
                    {
                        raffleOrder.Customer = customer;
                        return raffleOrder;
                    },
                    new { id = request.OrderId },
                    splitOn: "Email"))
                    .Distinct()
                    .SingleOrDefault();

                order.Lines = (await conn.QueryAsync<RaffleOrderLine>(getOrderLineItems, new { id = request.OrderId })).ToList();
                return order;
            }
        }
    }
}
