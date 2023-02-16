using BusinessLayer.Interfaces;
using BusinessLayer.Models.Files;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class FileUploadLocalService : IFileUploadService
    {
        private readonly ImageStorageSettings _settings;

        public FileUploadLocalService(IOptions<ImageStorageSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<(bool, string)> UploadFile(string fileName, Stream image)
        {
            try
            {
                var path = Path.Combine(_settings.LocalStorage.StoragePath, fileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return (true, path.Replace("\\", "/"));
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
