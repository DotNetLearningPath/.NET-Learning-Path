using SupportAPI.Domain.ValueObjects;

namespace SupportAPI.Domain.Entities
{
    public class Request
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string RequestContent { get; private set; } = string.Empty;
        public RequestStatus Status { get; private set; }
        
        private Request()
        {

        }

        public static Request Create(Guid userId, string title, string requestContent)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
            ArgumentException.ThrowIfNullOrWhiteSpace(requestContent, nameof(requestContent));
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("UserId must be provided when creating a request");
            }

            return new Request
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Title = title.Trim(),
                RequestContent = requestContent.Trim(),
                Status = RequestStatus.Open
            };
        }

        public void ChangeStatus(RequestStatus status)
        {
            if (!Enum.IsDefined(typeof(RequestStatus), status))
            {
                throw new ArgumentException("Invalid status provided");
            }
            Status = status;

            //TODO, plan a concrete set of rules regarding the status change
        }
    }
}
