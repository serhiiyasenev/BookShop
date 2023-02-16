using System.IO;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IFileUploadService
    {
        /// <summary>
        /// Save (copy) file to from request to approptiate (based on settings) storage
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileContent"></param>
        /// <returns>Saved status and message as path to file if successful or error message if not</returns>
        Task<(bool IsSaved, string Message)> UploadFile(string fileName, Stream fileContent);
    }
}
