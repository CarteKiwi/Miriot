using Miriot.Common.Model;

namespace Miriot.Common
{
    public class ServiceResponse
    {
        public ErrorType? Error { get; set; }

        public User[] Users { get; set; }

        public ServiceResponse(User[] users, ErrorType? error)
        {
            Users = users;
            Error = error;
        }
    }
}
