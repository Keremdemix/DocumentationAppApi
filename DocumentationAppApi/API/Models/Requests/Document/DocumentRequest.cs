using Microsoft.AspNetCore.Http;

namespace DocumentationAppApi.Requests.Documents
{
    public class DocumentRequest
    {
        public int ApplicationId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
