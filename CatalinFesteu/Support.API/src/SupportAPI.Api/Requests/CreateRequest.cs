using System.ComponentModel.DataAnnotations;

namespace SupportAPI.Api.Requests
{
    public sealed record CreateRequest
    {
        [Required, MaxLength(200),]
        public string Title { get; set; } = string.Empty;
        [Required, MaxLength(3000)]
        public string RequestContent { get; set; } = string.Empty;
    }
}