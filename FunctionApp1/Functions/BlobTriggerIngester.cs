using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApp1.Functions;

public static class BlobTriggerIngester
{
    [FunctionName("CsvBlobTrigger")]
    public static async Task RunAsync([BlobTrigger("samples-workitems/{name}", Connection = "")] Stream myBlob, string name, ILogger log)
    {
        using (var reader = new StreamReader(myBlob))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<FooMap>();
            var records = csv.GetRecords<Foo>();

            foreach (var record in records)
            {
                log.LogInformation($"Record: {record.Id} with name {record.Name}");
            }
        }

        log.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Size: {myBlob.Length} Bytes");
    }

    public class Foo
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string DateOfBirth { get; set; }
    }

    sealed public class FooMap : ClassMap<Foo>
    {
        public FooMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.Name).Name("ColumnB");
            Map(m => m.DateOfBirth).Name("Date Of Birth");
        }
    }
}
