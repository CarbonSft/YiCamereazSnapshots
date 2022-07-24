using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System.Threading.Tasks;

namespace CarbonSoftware.YiCamerazSnapshots
{

    public class updateCurrentSnapshot
    {
        const string ContanerName = "periodic-snapshots";
        const string CurrentJpgName = "current.jpg";
            
        [FunctionName("updateCurrentSnapshot")]
        public static async Task Run([BlobTrigger("periodic-snapshots/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
        
            if(CurrentJpgName.Equals(name))return;

            // retrieve the SOURCE and DESTINATION Storage Account Connection Strings
            var sourceConnString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var destConnString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Create SOURCE Blob Client
            var sourceBlobClient = new BlobClient(sourceConnString, "periodic-snapshots", name);

            // Generate SAS Token for reading the SOURCE Blob with a 2 hour expiration
            var sourceBlobSasToken = sourceBlobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.Now.AddHours(2));

            // Create DESTINATION Blob Client
            var destBlobClient = new BlobClient(destConnString, "periodic-snapshots", CurrentJpgName);

            // Initiate Blob Copy from SOURCE to DESTINATION
            await destBlobClient.StartCopyFromUriAsync(sourceBlobSasToken);

            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
