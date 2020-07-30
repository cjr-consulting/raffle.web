using Dapper;

using MediatR;

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
        readonly IRaffleEmailSender emailSender;
        readonly EmbeddedResourceReader reader;
        readonly EmailAddress managerEmail;

        public CompleteRaffleOrderCommandHandler(
            RaffleDbConfiguration config,
            IRaffleEmailSender emailSender,
            EmbeddedResourceReader reader,
            EmailAddress managerEmail)
        {
            this.managerEmail = managerEmail;
            this.reader = reader;
            this.emailSender = emailSender;
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
                var body = BuildTemplate(reader.GetContents("Raffle.Core.EmailTemplates.OrderComplete.html"),
                    order,
                    notification);

                var text = BuildTextTemplate(order, notification);

                await emailSender.SendEmailAsync(
                    notification.Email,
                    $"{notification.FirstName} {notification.LastName}",
                    $"Receipt for Darts For Dreams 15 Raffle Order# {notification.OrderId}",
                    text,
                    body);

                await emailSender.SendEmailAsync(
                    managerEmail.Email,
                    managerEmail.Name,
                    $"Receipt for Darts For Dreams 15 Raffle Order# {notification.OrderId}",
                    text,
                    body);
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
            const string getOrderLineItems = "SELECT * FROM RaffleOrderLineItems WHERE RaffleOrderId = @id";

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

            order.Lines = (await conn.QueryAsync<RaffleOrderLine>(getOrderLineItems, new { id = orderId })).ToList();
            return order;
        }

        private string BuildTextTemplate(RaffleOrder order, CompleteRaffleOrderCommand command)
        {
            var text = $"Dart for Dreams - Raffle Receipt" + Environment.NewLine;
            text += $"{command.FirstName} {command.LastName}" + Environment.NewLine;
            text += $"{command.PhoneNumber} " + Environment.NewLine;
            text += $"{command.Email}" + Environment.NewLine;
            text += $"{command.AddressLine1}" + Environment.NewLine;
            text += $"{command.AddressLine2}" + Environment.NewLine;
            text += $"{command.City}, {command.State}  {command.Zip}" + Environment.NewLine;
            text += $"" + Environment.NewLine + Environment.NewLine;

            foreach(var line in order.Lines)
            {
                text += $"{line.Name}  {line.Price}p x {line.Count}tix" + Environment.NewLine;
            }

            text += Environment.NewLine;
            text += $"TOTAL POINTS: {order.TotalPrice}" + Environment.NewLine;

            text += Environment.NewLine + Environment.NewLine;

            text += "To complete the order please go to" + Environment.NewLine +
                "http://site.wish.org/goto/DartsforDreams15" + Environment.NewLine +
                $"and enter a donation for ${order.TotalPrice} to complete the purchase." + Environment.NewLine;

            return text;
        }

        private string BuildTemplate(string template, RaffleOrder order, CompleteRaffleOrderCommand command)
        {
            const string token = "${privacy.url}";

            var ticketDetail = "";
            foreach (var line in order.Lines)
            {
                ticketDetail += "<tr>" +
                    "<td style=\"font-family:'Open Sans', Arial, sans-serif; font-size:18px; line-height:22px; color: #fbeb59; letter-spacing:2px; padding-bottom:12px;\" valign=\"top\" align=\"left\" width=\"70%\">" +
                    $"{line.Name}" +
                    "</td>" +
                    "<td style=\"font-family:'Open Sans', Arial, sans-serif; font-size:18px; line-height:22px; color: #fbeb59; letter-spacing:2px; padding-bottom:12px;\" valign=\"top\" align=\"center\">" +
                    $"{line.Price} p" +
                    "</td>" +
                    "<td style=\"font-family:'Open Sans', Arial, sans-serif; font-size:18px; line-height:22px; color: #fbeb59; letter-spacing:2px; padding-bottom:12px;\" valign=\"top\" align=\"center\">" +
                    $"{line.Count} tix" +
                    "</td>" +
                    "</tr>";
            }

            var result = template.Replace(token, $"https://raffle.dartsfordreams.com/home/privacy")
                .Replace("${raffle.orderid}", order.Id.ToString())
                .Replace("${donor.email}", command.Email)
                .Replace("${name.first}", command.FirstName)
                .Replace("${name.last}", command.LastName)
                .Replace("${phoneNumber}", command.PhoneNumber)
                .Replace("${address.line1}", command.AddressLine1)
                .Replace("${address.line2}", command.AddressLine2)
                .Replace("${address.city}", command.City)
                .Replace("${address.state}", command.State)
                .Replace("${address.zip}", command.Zip)
                .Replace("${raffle.tickets}", ticketDetail)
                .Replace("${raffle.price}", order.TotalPrice.ToString());
            return result;
        }
    }
}
