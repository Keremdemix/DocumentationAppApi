using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DocumentationAppApi.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadAsync(IFormFile file, string folder);
    }
}
