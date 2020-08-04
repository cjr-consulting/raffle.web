using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        [Display(Name = "Confirm 21 Years Old")]
        public bool Confirmed21 { get; set; }

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

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "Zip")]
        public string Zip { get; set; }

        [Display(Name = "Address Outside the USA")]
        public bool IsInternational { get; set; }

        [Display(Name = "International Address Text")]
        public string InternationalAddress { get; set; }

        public string HowDidYouHear { get; set; }
        [MaxLength(500)]
        public string HowDidYouHearOther { get; set; }
        public int TotalPrice { get; set; }
        public int TotalTickets { get; set; }

        public List<CompleteRaffleLineItemModel> Items { get; set; } = new List<CompleteRaffleLineItemModel>();

        public List<SelectListItem> HowDidYouHearList { get; set; }

        public CompleteRaffleModel()
        {
            HowDidYouHearList = new List<SelectListItem>
            {
                new SelectListItem("You've been to the event in the past or know people who have",  "You've been to the event in the past or know people who have"),
                new SelectListItem("You found us on FB directly or via a share in a community FB group", "You found us on FB directly or via a share in a community FB group"),
                new SelectListItem("You saw it posted on FB in a darts related group", "You saw it posted on FB in a darts related group"),
                new SelectListItem("You heard it from a friend who... heard it from a friend....", "You heard it from a friend who... heard it from a friend...."),
                new SelectListItem("Other: Please let me know","Other: Please let me know")
            };
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(CustomerEmail != CustomerEmailRepeat)
            {
                yield return new ValidationResult(
                    $"Emails don't match.",
                    new[] { nameof(CustomerEmailRepeat) });
            }

            if (IsInternational)
            {
                if (string.IsNullOrEmpty(InternationalAddress))
                {
                    yield return new ValidationResult(
                        $"International Address is Required.",
                        new[] { nameof(InternationalAddress) });
                }
            }
            else
            {
                if (string.IsNullOrEmpty(City))
                {
                    yield return new ValidationResult(
                        $"City is Required.",
                        new[] { nameof(City) });
                }

                if (string.IsNullOrEmpty(State))
                {
                    yield return new ValidationResult(
                        $"State is Required.",
                        new[] { nameof(State) });
                }

                if (string.IsNullOrEmpty(Zip))
                {
                    yield return new ValidationResult(
                        $"Zip is Required.",
                        new[] { nameof(Zip) });
                }
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
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
