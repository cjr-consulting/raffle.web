using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Raffle.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Raffle.Web.Services
{
    public class AzureBlobStorageService : IStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobStorageService(
            string connectionString,
            string containerName)
        {
            _containerName = containerName;
            _connectionString = connectionString;
        }

        public async Task<string> SaveFile(StorageFile file)
        {
            var client = new BlobContainerClient(_connectionString, _containerName);
            if (!client.Exists())
            {
                await client.CreateAsync(PublicAccessType.BlobContainer);
            }

            var extension = new FileInfo(file.Name).Extension.Replace(".", "");

            string newFileName = $"{Guid.NewGuid():N}.{extension}";
            string fileName = string.Join("/", file.Path.Skip(1).Union(new[] { file.Name, newFileName }));

            var blob = client.GetBlobClient(newFileName);
            await blob.UploadAsync(new MemoryStream(file.Data), new BlobHttpHeaders
            {
                ContentType = $"image/{extension}"
            });

            return blob.Uri.ToString();
        }

        public string GetFullPath(string relativePath)
        {
            // return $"{_storageAcc.BlobStorageUri.PrimaryUri}{relativePath}";
            return "";
        }
    }

}
