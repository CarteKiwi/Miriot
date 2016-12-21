using Miriot.Common.Model;

namespace Miriot.Common
{
    public class ServiceResponse
    {
        public ErrorType? Error { get; set; }

        public User User { get; set; }

        public ServiceResponse(User user, ErrorType? error)
        {
            User = user;
            Error = error;
        }
    }
}
