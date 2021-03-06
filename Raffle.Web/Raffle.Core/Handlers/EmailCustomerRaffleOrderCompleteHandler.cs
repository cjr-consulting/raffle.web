﻿using MediatR;

using Microsoft.Extensions.Logging;

using Raffle.Core.Events;
using Raffle.Core.Models;
using Raffle.Core.Shared;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raffle.Core.Handlers
{
    class CompletedOrderEmailCustomerHandler : INotificationHandler<RaffleOrderCompleteEvent>
    {
        readonly IRaffleEmailSender emailSender;
        readonly EmbeddedResourceReader reader;
        readonly EmailAddress managerEmail;
        readonly ILogger<CompletedOrderEmailCustomerHandler> logger;

        public CompletedOrderEmailCustomerHandler(
            ILogger<CompletedOrderEmailCustomerHandler> logger,
            IRaffleEmailSender emailSender,
            EmbeddedResourceReader reader,
            EmailAddress managerEmail)
        {
            this.logger = logger;
            this.managerEmail = managerEmail;
            this.reader = reader;
            this.emailSender = emailSender;
        }

        public async Task Handle(RaffleOrderCompleteEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var order = notification.Order;
                var body = BuildTemplate(reader.GetContents("Raffle.Core.EmailTemplates.OrderComplete.html"),
                        order);

                var text = BuildTextTemplate(notification.Order);
                await SendCustomerReceiptEmail(order, body, text);
                await SendOrderReceivedEmail(order, body, text);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Failed to send emails after order completed");
            }
        }

        private async Task SendOrderReceivedEmail(RaffleOrder order, string body, string text)
        {
            await emailSender.SendEmailAsync(
                            managerEmail.Email,
                            managerEmail.Name,
                            $"Receipt for Darts For Dreams 15 Raffle Order# {order.Id}",
                            text,
                            body);
        }

        private async Task SendCustomerReceiptEmail(RaffleOrder order, string body, string text)
        {
            await emailSender.SendEmailAsync(
                            order.Customer.Email,
                            $"{order.Customer.FirstName} {order.Customer.LastName}",
                            $"Receipt for Darts For Dreams 15 Raffle Order# {order.Id}",
                            text,
                            body);
        }

        private string BuildTextTemplate(RaffleOrder order)
        {
            var text = $"Dart for Dreams - Raffle Receipt" + Environment.NewLine;
            text += $"{order.Customer.FirstName} {order.Customer.LastName}" + Environment.NewLine;
            text += $"{order.Customer.PhoneNumber} " + Environment.NewLine;
            text += $"{order.Customer.Email}" + Environment.NewLine;
            text += $"{order.Customer.AddressLine1}" + Environment.NewLine;
            text += $"{order.Customer.AddressLine2}" + Environment.NewLine;
            text += $"{order.Customer.City}, {order.Customer.State}  {order.Customer.Zip}" + Environment.NewLine;
            text += $"" + Environment.NewLine + Environment.NewLine;

            foreach (var line in order.Lines)
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

        private string BuildTemplate(string template, RaffleOrder order)
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
                .Replace("${donor.email}", order.Customer.Email)
                .Replace("${name.first}", order.Customer.FirstName)
                .Replace("${name.last}", order.Customer.LastName)
                .Replace("${phoneNumber}", order.Customer.PhoneNumber)
                .Replace("${address.line1}", order.Customer.AddressLine1)
                .Replace("${address.line2}", order.Customer.AddressLine2)
                .Replace("${raffle.tickets}", ticketDetail)
                .Replace("${raffle.price}", order.TotalPrice.ToString());

            if (order.Customer.IsInternational)
            {
                result = result.Replace("${address.city_state_zip}", order.Customer.InternationalAddress.Replace("\r\n", "<br />"));
            }
            else
            {
                result = result.Replace("${address.city_state_zip}", $"{order.Customer.City}, {order.Customer.State} {order.Customer.Zip}");
            }

            return result;
        }
    }
}
