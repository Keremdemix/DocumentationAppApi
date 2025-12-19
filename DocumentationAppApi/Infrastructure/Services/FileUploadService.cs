using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DocumentationAppApi.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _env;

        public FileUploadService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Dosya boş veya null");

            // Yükleme dizini
            var uploadPath = Path.Combine(_env.ContentRootPath, "Uploads", folder);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Dosya adını temizle
            var fileName = Path.GetFileName(file.FileName)
                               .Replace(" ", "_")
                               .Replace("ç", "c").Replace("Ç", "C")
                               .Replace("ğ", "g").Replace("Ğ", "G")
                               .Replace("ı", "i").Replace("İ", "I")
                               .Replace("ö", "o").Replace("Ö", "O")
                               .Replace("ş", "s").Replace("Ş", "S")
                               .Replace("ü", "u").Replace("Ü", "U");

            var filePath = Path.Combine(uploadPath, fileName);

            // Aynı isim varsa GUID ekle
            if (File.Exists(filePath))
            {
                var uniqueSuffix = Guid.NewGuid().ToString();
                fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{uniqueSuffix}{Path.GetExtension(fileName)}";
                filePath = Path.Combine(uploadPath, fileName);
            }

            // Dosyayı kaydet
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName; // veya filePath dönebilirsin
        }
    }
}
