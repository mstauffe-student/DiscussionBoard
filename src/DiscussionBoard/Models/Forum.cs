using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models
{
    /**
     * Represents a forum for a discussion board.
     */
    public class Forum : EntityBase
    {
        [MaxLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        [MinLength(2, ErrorMessage = "Name cannot be smaller than 2 characters.")]
        public required string Name { get; set; } = null!;
        public string Description { get; set; }

        public ICollection<DiscussionThread> DiscussionThreads { get; set; } = new List<DiscussionThread>();
        //Many to 1 with DiscussionThread^^^
    }
}
