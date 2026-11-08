using System.ComponentModel.DataAnnotations;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Mvc;

namespace DiscussionBoard.Pages.Discussions
{
    public class UserInputModel
    {
        [MaxLength(160, ErrorMessage = "Title cannot exceed 160 characters")]
        [MinLength(2, ErrorMessage = "Title cannot be smaller than 2 characters")]
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; } = string.Empty;

        public string ThreadName { get; set; } = string.Empty;

        public int? ParentPostId { get; set; }
    }
}
