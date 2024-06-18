using System.ComponentModel.DataAnnotations;

namespace SampleAPI.Entities
{
    public sealed class Order
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name length can't be more than 100.")]
        public string? Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Description length can't be more than 100.")]
        public string? Description { get; set; }

        public DateTime EntryDate { get; set; }

        public bool IsInvoiced { get; set; } = true;

        public bool IsDeleted { get; set; } = false;
    }
}
