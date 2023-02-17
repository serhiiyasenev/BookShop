namespace BusinessLayer.Models.Files
{
    public class ImageStorageSettings
    {
        public string AllowedExtensions { get; set; }
        public Localstorage LocalStorage { get; set; }
        public Blobstorage BlobStorage { get; set; }
    }

    public class Localstorage
    {
        public string StoragePath { get; set; }
    }

    public class Blobstorage
    {
        public string StoragePath { get; set; }
        public string containerName { get; set; }
        public Account Account { get; set; }
    }

    public class Account
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
