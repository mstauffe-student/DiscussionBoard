using System.ComponentModel.DataAnnotations;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Mvc;

namespace DiscussionBoard.Pages.Discussions
{
    public class UserInputModel
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Title cannot exceed 50 characters")]
        [MinLength(2, ErrorMessage = "Title cannot be smaller than 2 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(2, ErrorMessage = "Content cannot be smaller than 2 characters")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int Id { get; set; }
    }
}
