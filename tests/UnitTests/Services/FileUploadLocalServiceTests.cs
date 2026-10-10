using System;
using System.IO;
using System.Threading.Tasks;
using BusinessLayer.Models.Files;
using BusinessLayer.Services;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace UnitTests.Services
{
    public class FileUploadLocalServiceTests
    {
        private string _directory;

        [SetUp]
        public void SetUp() => _directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "BookShop_" + Guid.NewGuid().ToString("N"))).FullName;

        [TearDown]
        public void TearDown() => Directory.Delete(_directory, true);

        [Test]
        public async Task Upload_WritesExactBytesAndReturnsPortablePath()
        {
            var bytes = new byte[] { 0, 1, 2, 127, 255 };
            using var stream = new MemoryStream(bytes);
            var result = await Sender(_directory).UploadFile("cover.jpg", stream);

            Assert.That(result.Item1, Is.True);
            Assert.That(result.Item2, Is.EqualTo(Path.Combine(_directory, "cover.jpg").Replace("\\", "/")));
            Assert.That(await File.ReadAllBytesAsync(Path.Combine(_directory, "cover.jpg")), Is.EqualTo(bytes));
            Assert.That(stream.CanRead, Is.True, "The caller owns the input stream.");
        }

        [Test]
        public async Task Upload_ReturnsFailureWhenDirectoryDoesNotExist()
        {
            using var stream = new MemoryStream(new byte[] { 1 });
            var result = await Sender(Path.Combine(_directory, "missing")).UploadFile("cover.jpg", stream);

            Assert.That(result.Item1, Is.False);
            Assert.That(result.Item2, Is.Not.Empty);
            Assert.That(Directory.GetFiles(_directory), Is.Empty);
        }

        private static FileUploadLocalService Sender(string path) => new FileUploadLocalService(
            Options.Create(new ImageStorageSettings
            {
                LocalStorage = new Localstorage { StoragePath = path }
            }));
    }
}
