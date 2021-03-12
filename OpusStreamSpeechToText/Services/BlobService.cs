using Azure.Storage.Blobs;
using OpusStreamSpeechToText.Config;
using System;
using System.Threading.Tasks;

namespace OpusStreamSpeechToText.Services
{
    public class BlobService
    {
        /// <summary>
        /// Download blob as stream from Azure
        /// </summary>
        /// <param name="fileName">Name of blob</param>
        /// <param name="containerName">Name of Blob container</param>
        /// <param name="saveToLocation"></param>
        /// <returns>Stream</returns>
        public async Task<BlobClient> GetBlobFromContainerAsync(string fileName, string containerName)
        {
            var client = await SetBlobContainerClientAsync(containerName);
            var blobClient = client.GetBlobClient(fileName);

            if (!blobClient.Exists())
                throw new Exception($@"Blob ""{fileName}"" in container ""{containerName}"" does not exist!");

            return blobClient;
        }

        private async Task<BlobContainerClient> SetBlobContainerClientAsync(string containerName)
        {
            try
            {
                // Azure File Storage ONLY accept lowercase names with letters, numbers and dashes (-)
                containerName = containerName.ToLower();

                BlobServiceClient blobServiceClient = new BlobServiceClient(Settings.AzureBlobStorage.CONNECTION_STRING);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                if (!await containerClient.ExistsAsync())
                    throw new Exception($@"Blobcontainer ""{containerName}"" does not exist!");

                return containerClient;
            }
            catch
            {
                throw new Exception("Couldn't connect to Azure Storage, check your connectionstring.");
            }
        }
    }
}
