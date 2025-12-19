using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DocumentationAppApi.Infrastructure.Services.FileUploadService
{
    public interface IFileUploadService
    {
        Task<string> UploadAsync(IFormFile file, string folder);
    }
}
