using Dapper;

using MediatR;

using Raffle.Core.Events;
using Raffle.Core.Models;
using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Commands
{
    public class CompleteRaffleOrderCommand : INotification
    {
        public int OrderId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Confirmed21 { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public bool IsInternational { get; set; }
        public string InternationalAddress { get; set; }
    }

    public class CompleteRaffleOrderCommandHandler : INotificationHandler<CompleteRaffleOrderCommand>
    {
        readonly string connectionString;
        readonly IMediator mediator;

        public CompleteRaffleOrderCommandHandler(
            RaffleDbConfiguration config,
            IMediator mediator)
        {
            this.mediator = mediator;
            connectionString = config.ConnectionString;
        }

        public async Task Handle(CompleteRaffleOrderCommand notification, CancellationToken cancellationToken)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                const string updateOrder = "UPDATE RaffleOrders SET " +
                    "Confirmed21 = @Confirmed21, " +
                    "Customer_FirstName = @FirstName, " +
                    "Customer_LastName = @LastName, " +
                    "Customer_PhoneNumber = @PhoneNumber, " +
                    "Customer_Email = @Email, " +
                    "Customer_AddressLine1 = @AddressLine1, " +
                    "Customer_AddressLine2 = @AddressLine2, " +
                    "Customer_Address_City = @City, " +
                    "Customer_Address_State = @State, " +
                    "Customer_Address_Zip = @Zip, " +
                    "Customer_IsInternational = @IsInternational, " +
                    "Customer_IAddressText = @InternationalAddress, " +
                    "CompletedDate = @CompletedDate " +
                    "WHERE Id = @OrderId";

                await conn.ExecuteAsync(updateOrder, new
                {
                    notification.OrderId,
                    notification.Confirmed21,
                    notification.FirstName,
                    notification.LastName,
                    notification.PhoneNumber,
                    notification.Email,
                    notification.IsInternational,
                    notification.AddressLine1,
                    notification.AddressLine2,
                    notification.City,
                    notification.State,
                    notification.Zip,
                    notification.InternationalAddress,
                    CompletedDate = DateTime.UtcNow
                });

                var order = await GetOrder(conn, notification.OrderId);
                await mediator.Publish(new RaffleOrderCompleteEvent
                    {
                        Order = order
                    });
            }
        }

        public async Task<RaffleOrder> GetOrder(SqlConnection conn, int orderId)
        {
            const string getOrder = "SELECT Id = Ro.Id, ro.TicketNumber, ro.IsOrderConfirmed, " +
                    "ro.Confirmed21, " +
                    "Email = ro.Customer_Email, FirstName = ro.Customer_FirstName, LastName = ro.Customer_LastName, " +
                    "PhoneNumber = Customer_PhoneNumber, " +
                    "AddressLine1 = Customer_AddressLine1, " +
                    "AddressLine2 = Customer_AddressLine2, " +
                    "City = Customer_Address_City, " +
                    "State = Customer_Address_State, " +
                    "Zip = Customer_Address_Zip, " +
                    "IsInternational = Customer_IsInternational, " +
                    "InternationalAddress = Customer_IAddressText " +
                    " FROM RaffleOrders ro WHERE Id  =  @id";
            const string getOrderLineItems = "SELECT " +
                    "li.RaffleOrderId, " +
                    "li.RaffleItemId, " +
                    "RaffleItemNumber = ri.ItemNumber, " +
                    "li.Name, " +
                    "li.Price, " +
                    "li.Count " +
                    "FROM RaffleOrderLineItems li JOIN RaffleItems ri ON li.RaffleItemId = ri.Id " +
                    "WHERE RaffleOrderId = @id AND Count > 0 " +
                    "ORDER BY ri.ItemNumber;";

            var order = (await conn.QueryAsync<RaffleOrder, Customer, RaffleOrder>(
                            getOrder,
                            (raffleOrder, customer) =>
                                {
                                    raffleOrder.Customer = customer;
                                    return raffleOrder;
                                },
                            new { id = orderId },
                            splitOn: "Email"))
                            .Distinct()
                            .SingleOrDefault();

            order.Lines = (await conn.QueryAsync<RaffleOrderLine>(getOrderLineItems, new { id = orderId }))
                            .OrderBy(x=>x.RaffleItemNumber)
                            .ToList();
            return order;
        }

    }
}
