using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Raffle.Web.Models.Raffle
{
    public class CompleteRaffleModel : IValidatableObject
    {
        [Required]
        [Display(Name = "First Name")]
        public string CustomerFirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string CustomerLastName { get; set; }

        [Required]
        [Phone]
        [Display(Name="Phone Number")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string PhoneNumber { get; set; }
        
        [Required]
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Required]
        [Display(Name = "Repeat Email Address")]
        [EmailAddress]
        public string CustomerEmailRepeat { get; set; }

        [Required]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [Display(Name = "Zip")]
        public string Zip { get; set; }

        public int TotalPrice { get; set; }
        public int TotalTickets { get; set; }

        public List<CompleteRaffleLineItemModel> Items { get; set; } = new List<CompleteRaffleLineItemModel>();

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

    public class CompleteRaffleLineItemModel
    {
        public int RaffleItemId { get; set; }
        public string Name { get; set; }
        public int Cost { get { return Amount * Price; } }
        public int Amount { get; set; }
        public int Price { get; set; }
    }
}
