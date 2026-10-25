using Microsoft.AspNetCore.Identity;

namespace DiscussionBoard.Models
{
    /**
     * Class that represents the users of this discussion board.
     */
    public class ApplicationUser : IdentityUser
    {
        public ICollection<DiscussionThread> DiscussionThreads { get; set; } = new List<DiscussionThread>();
        //Many to 1 for DiscussionThreads^^
        public ICollection<Post> Posts { get; set; } = new List<Post>(); //Many to 1 for Posts
    }
}
