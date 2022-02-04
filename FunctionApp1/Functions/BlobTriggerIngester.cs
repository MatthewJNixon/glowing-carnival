using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApp1.Functions;

public static class BlobTriggerIngester
{
    [FunctionName("CsvBlobTrigger")]
    public static async Task RunAsync([BlobTrigger("samples-workitems/{name}", Connection = "")] Stream myBlob, string name, ILogger log)
    {
        // TODO: work for the CSV feature

        log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
    }
}
