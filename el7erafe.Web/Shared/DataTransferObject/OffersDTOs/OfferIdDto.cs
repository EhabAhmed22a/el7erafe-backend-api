using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.OffersDTOs
{
    public class OfferIdDto
    {
        [Required]
        public int offerId { get; set; }
    }
}
