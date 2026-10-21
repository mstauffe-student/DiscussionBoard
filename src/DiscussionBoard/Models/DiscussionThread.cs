namespace DiscussionBoard.Models
{
    /**
     * This class represents a thread in a discussion board.
     */
    public class DiscussionThread : DiscussionBase
    {
        public int ForumId { get; set; } //Foreign key for Forums
        public Forum Forum { get; set; } = null!; //1 to Many Forums

        public ICollection<Post> Posts { get; set; } = new List<Post>(); //Many to 1 Posts

    }
}
