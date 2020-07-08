using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Raffle.Web.Models.Raffle
{
    public class CompleteRaffleModel : IValidatableObject
    {
        [Required]
        public string CustomerFirstName { get; set; }
        [Required]
        public string CustomerLastName { get; set; }
        [EmailAddress]
        public string CustomerEmail { get; set; }
        [EmailAddress]
        public string CustomerEmailRepeat { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public int TotalPrice { get; set; }
        public int TotalTickets { get; set; }

        public List<CompleteRaffleItemModel> Items { get; set; } = new List<CompleteRaffleItemModel>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(CustomerEmail != CustomerEmailRepeat)
            {
                yield return new ValidationResult(
                    $"Emails don't match.",
                    new[] { nameof(CustomerEmailRepeat) });
            }
        }
    }

    public class CompleteRaffleItemModel
    {
        public int RaffleItemId { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public int Amount { get; set; }
        public int Price { get; set; }
    }
}
