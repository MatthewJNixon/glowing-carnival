using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using FunctionApp1.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace FunctionApp1.Functions;

public class BlobTriggerIngester
{
    private readonly IDataverseWebApiService _dataverseWebApiService;

    public BlobTriggerIngester(IDataverseWebApiService dataverseWebApiService)
    {
        _dataverseWebApiService = dataverseWebApiService;
    }

    [FunctionName("CsvBlobTrigger")]
    public async Task RunAsync(
        [BlobTrigger("samples-workitems/{name}", Connection = "")] Stream myBlob, string name,
        [SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")] IAsyncCollector<SendGridMessage> messageCollector,
        ILogger log)
    {
        using (var reader = new StreamReader(myBlob))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            // used with mapping
            // csv.Context.RegisterClassMap<FooMap>();
            var records = csv.GetRecords<Foo>();

            foreach (var record in records)
            {
                log.LogInformation($"Record: {record.Id} with name {record.Name}");
            }
        }
        // UpdateData(log);

        // FetchData(log);

        log.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Size: {myBlob.Length} Bytes");

        var message = new SendGridMessage();
        message.AddTo("matthew.nixon@arkahna.io");
        message.AddContent("text/html", $"test email: C# Blob trigger function Processed blob\n Name: {name} \n Size: {myBlob.Length} Bytes");
        message.SetFrom(new EmailAddress("matthew.nixon@arkahna.io"));
        message.SetSubject("test email");

        await messageCollector.AddAsync(message);
    }

    public async void FetchData(ILogger log)
    {
        var entities = await _dataverseWebApiService.GetAsync<object>("cr70e_testtables");
        log.LogInformation("Some entities: {entities}", entities);
    }

    public async void UpdateData(ILogger log) =>
        await _dataverseWebApiService.PostCreateAsync("cr70e_testtables", new TestTableEntity {
            EpisodeId = "5",
            EpisodeName = "test name"
        });

    public class Foo
    {
        [Name("Id")]
        public int Id { get; set; }

        [Name("ColumnB")]
        public string Name { get; set; }

        [Name("Date Of Birth")]
        public string DateOfBirth { get; set; }
    }

    public class TestTableEntity
    {
        // Note: I seem to have created a string in my demo, in reality this probably won't be a string
        [JsonProperty("cr70e_episodeid")] public string EpisodeId { get; set; }

        [JsonProperty("cr70e_episodename")] public string EpisodeName { get; set; }
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


    public class OutgoingEmail
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
