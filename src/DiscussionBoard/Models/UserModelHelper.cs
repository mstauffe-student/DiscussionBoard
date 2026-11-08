namespace DiscussionBoard.Models
{
    /**
     * Helper for Users page
     */
    public class UserModelHelper
    {
        public bool IsAdmin { get; set; }
        public string SelectedUserId { get; set; }
        public ApplicationUser ChosenUser { get; set; }
    }
}
