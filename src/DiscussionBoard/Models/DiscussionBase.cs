using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models
{
    /**
     * Base class for discussion-related things.
     */
    public class DiscussionBase : EntityBase
    {
        [MaxLength(50, ErrorMessage = "Title cannot be longer than 50 characters.")]
        [MinLength(2, ErrorMessage = "Title cannot be smaller than 2 characters.")]
        public required string Title { get; set; } = null!;
        public string Content { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public string ApplicationUserId { get; set; } //Foreign key for Application Users
        public ApplicationUser ApplicationUser { get; set; } = null!; //1 to Many Application Users
    }
}
