using Azure.Storage.Blobs;
using StreamSpeechToText.Config;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StreamSpeechToText.Services
{
    public class BlobService
    {
        /// <summary>
        /// Download blob as stream from Azure
        /// </summary>
        /// <param name="fileName">Name of blob</param>
        /// <param name="containerName">Name of Blob container</param>
        /// <param name="saveToLocation"></param>
        /// <returns>BlobClient</returns>
        public async Task<BlobClient> GetBlobFromContainerAsync(string fileName, string containerName)
        {
            var client = await SetBlobContainerClientAsync(containerName);
            var blobClient = client.GetBlobClient(fileName);

            if (!blobClient.Exists())
                throw new Exception($@"Can't find ""{fileName}"" in blobcontainer ""{containerName}"".");

            return blobClient;
        }

        private async Task<BlobContainerClient> SetBlobContainerClientAsync(string containerName)
        {
            ValidateBlobContainerName(containerName);

            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(Settings.AzureBlobStorage.CONNECTION_STRING);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                if (!await containerClient.ExistsAsync())
                    throw new ArgumentException($@"Blobcontainer ""{containerName}"" does not exist in Azure Storage!");

                return containerClient;
            }
            catch (Exception)
            {
                throw new Exception("Couldn't connect to Azure Storage, check your connectionstring.");
            }
        }

        private void ValidateBlobContainerName(string containerName)
        {
            if (!Regex.IsMatch(containerName, @"^[-a-z]+$"))
                throw new ArgumentException($@"Containername ""{containerName}"" invalid. Azure only accepts lowercase letters, numbers and dashes.");
        }
    }
}
