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
        readonly IRaffleEmailSender emailSender;
        readonly EmbeddedResourceReader reader;
        readonly EmailAddress managerEmail;

        public CompleteRaffleOrderCommandHandler(
            string dbConnectionString,
            IRaffleEmailSender emailSender,
            EmbeddedResourceReader reader,
            EmailAddress managerEmail)
        {
            this.managerEmail = managerEmail;
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

                var order = GetOrder(conn, command.OrderId);
                var body = BuildTemplate(reader.GetContents("Raffle.Core.EmailTemplates.OrderComplete.html"),
                    order,
                    command);

                var text = BuildTextTemplate(order, command);

                emailSender.SendEmailAsync(
                    command.Email,
                    $"{command.FirstName} {command.LastName}",
                    "Darts For Dreams 15 - Raffle Order",
                    text,
                    body);

                emailSender.SendEmailAsync(
                    managerEmail.Email,
                    managerEmail.Name,
                    "Darts For Dreams 15 - Raffle Order",
                    text,
                    body);
            }
        }

        public string BuildTextTemplate(RaffleOrder order, CompleteRaffleOrderCommand command)
        {
            var text = $"Dart for Dreams - Raffle Order" + Environment.NewLine;
            text += $"{command.FirstName} {command.LastName}" + Environment.NewLine;
            text += $"{command.Email}" + Environment.NewLine;
            text += $"{command.AddressLine1}" + Environment.NewLine;
            text += $"{command.AddressLine2}" + Environment.NewLine;
            text += $"{command.City}, {command.State}  {command.Zip}" + Environment.NewLine;
            text += $"" + Environment.NewLine + Environment.NewLine;

            foreach(var line in order.Lines)
            {
                text += $"{line.Name}  {line.Price}p x {line.Count}tix" + Environment.NewLine;
            }

            text += Environment.NewLine + Environment.NewLine;

            text += "To complete the order please go to" + Environment.NewLine +
                "http://site.wish.org/goto/DartsforDreams15" + Environment.NewLine +
                $"and enter a donation for ${order.TotalPrice} to complete the purchase." + Environment.NewLine;

            return text;
        }

        public string BuildTemplate(string template, RaffleOrder order, CompleteRaffleOrderCommand command)
        {
            const string token = "${privacy.url}";

            var ticketDetail = "";
            foreach (var line in order.Lines)
            {
                ticketDetail += "<tr><td style=\"font-family:'Open Sans', Arial, sans-serif; font-size:18px; line-height:22px; color: #fbeb59; letter-spacing:2px; padding-bottom:12px;\" valign=\"top\" align=\"center\">";
                ticketDetail += $"{line.Name} {line.Price}p x {line.Count}tix";
                ticketDetail += "</td></tr>";
            }

            var result = template.Replace(token, $"https://raffle.dartsfordreams.com/home/privacy")
                .Replace("${raffle.orderid}", order.Id.ToString())
                .Replace("${donor.email}", command.Email)
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
