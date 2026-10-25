using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace DiscussionBoard.Models
{
    /**
     * Base class for discussion-related things.
     */
    public class DiscussionBase : EntityBase
    {
        [MaxLength(50, ErrorMessage = "Title cannot be longer than 50 characters.")]
        [MinLength(2, ErrorMessage = "Title cannot be smaller than 2 characters.")]
        public required string Title { get; set; } = null!; //required name
        public string? Content { get; set; } //optional info
        public DateTimeOffset CreatedAt { get; set; } //date it was made
        public string ApplicationUserId { get; set; } //Foreign key for Application Users
        public ApplicationUser ApplicationUser { get; set; } = null!; //1 to Many Application Users
    }
}
