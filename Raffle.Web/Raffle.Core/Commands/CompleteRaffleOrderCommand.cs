using Dapper;
using Raffle.Core.Models;
using Raffle.Core.Shared;

using System;
using System.Data.SqlClient;
using System.Linq;

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
        readonly IEmailSender emailSender;
        readonly EmbeddedResourceReader reader;

        public CompleteRaffleOrderCommandHandler(
            string dbConnectionString,
            IEmailSender emailSender,
            EmbeddedResourceReader reader)
        {
            this.reader = reader;
            this.emailSender = emailSender;
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

                var body = BuildTemplate(reader.GetContents("Raffle.Core.EmailTemplates.OrderComplete.html"),
                    GetOrder(conn, command.OrderId),
                    command);

                emailSender.SendEmailAsync(
                    command.Email, 
                    $"{command.FirstName} {command.LastName}",
                    "Darts For Dreams - Raffle Order",
                    "Thanks for you order",
                    body);

            }

            // Send Email to customer
            // Send Email to Gary
        }

        public string BuildTemplate(string template, RaffleOrder order, CompleteRaffleOrderCommand command)
        {
            const string token = "${privacy.url}";

            var ticketDetail = "";
            foreach(var line in order.Lines)
            {
                ticketDetail += "<tr><td style=\"font-family:'Open Sans', Arial, sans-serif; font-size:18px; line-height:22px; color: #fbeb59; letter-spacing:2px; padding-bottom:12px;\" valign=\"top\" align=\"center\">";
                ticketDetail += $"{line.Name} ${line.Price} x {line.Count}";
                ticketDetail += "</td></tr>";
            }

            var result = template.Replace(token, $"http://localhost:5001/home/privacy")
                .Replace("${raffle.orderid}", order.Id.ToString())
                .Replace("${name.first}", command.FirstName)
                .Replace("${name.last}", command.LastName)
                .Replace("${address.line1}", command.AddressLine1)
                .Replace("${address.line2}", command.AddressLine2)
                .Replace("${address.city}", command.City)
                .Replace("${address.state}", command.State)
                .Replace("${address.zip}", command.Zip)
                .Replace("${raffle.tickets}", ticketDetail)
                .Replace("${raffle.price}", order.TotalPrice.ToString());
            return result;
        }

        public RaffleOrder GetOrder(SqlConnection conn, int orderId)
        {
            const string getOrder = "SELECT * FROM RaffleOrders WHERE Id = @id";
            const string getOrderLineItems = "SELECT * FROM RaffleOrderLineItems WHERE RaffleOrderId = @id";

            var order = conn.Query<RaffleOrder>(getOrder, new { id = orderId }).SingleOrDefault();
            order.Lines = conn.Query<RaffleOrderLine>(getOrderLineItems, new { id = orderId }).ToList();
            return order;
        }
    }
}
