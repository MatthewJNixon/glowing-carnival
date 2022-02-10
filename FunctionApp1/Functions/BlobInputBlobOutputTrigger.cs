using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApp1.Functions;

public class BlobInputBlobOutputTrigger
{
    [FunctionName("CopyBlobToBlobStore")]
    public void RunAsync([BlobTrigger("samples-workitems/{name}", Connection = "")] Stream inputBlob, string name,
        [Blob("sample-workitems-output/{name}", FileAccess.Write)] Stream outputBlob,
        ILogger log)
    {
        // no async
        inputBlob.CopyTo(outputBlob);
        log.LogInformation("Blob Copy Complete");
    }
}
