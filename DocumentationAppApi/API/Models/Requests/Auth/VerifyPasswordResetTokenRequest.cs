namespace DocumentationAppApi.API.Models.Requests.Auth
{
    public class VerifyPasswordResetTokenRequest
    {
        public string Token {  get; set; }
        public string Mail { get; set; }
    }
}
