namespace DiscussionBoard.Models
{
    /**
     * Represents a post by a user in a discussion board.
     */
    public class Post : DiscussionBase
    {
        public int DiscussionThreadId { get; set; } //Foreign key
        public DiscussionThread DiscussionThread { get; set; } = null!; //1 to Many Forums


        public ICollection<Post> Replies { get; set; } = new List<Post>(); //Many to 1 replies

        public int? ParentPostId { get; set; } //Optional parent id
        public Post? ParentPost { get; set; } //Optional parent
    }
}
