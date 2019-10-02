using System;

namespace Login
{
    public class LoginResult
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTimeOffset ExpireAt { get; set; }
        public LoginState LoginState { get; set; }
        public string ErrorString { get; set; }
    }

    public enum LoginState
    {
        Failed,
        Canceled,
        Success
    }
}