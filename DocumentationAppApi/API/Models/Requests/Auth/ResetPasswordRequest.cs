namespace DocumentationAppApi.API.Models.Requests.Auth
{
    public class ResetPasswordRequest
    {
        public string Mail { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordControl { get; set; }
    }

}
