using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.ViewModels
{
    /**
     * Helper for EditThread and EditPost
     */
    public class DiscussionEditViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(160, ErrorMessage ="Max length is 160 characters")]
        [MinLength(3, ErrorMessage = "Min length is 3 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [MaxLength(10000, ErrorMessage ="Max length is 10000 characters")]
        [MinLength(1, ErrorMessage = "Min length is 1 characters.")]
        public string Content { get; set; } = string.Empty;
    }
}
