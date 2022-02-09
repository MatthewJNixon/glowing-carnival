using FunctionApp1;
using FunctionApp1.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Program))]
namespace FunctionApp1;

public class Program : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpClient("DataverseWebApi",client =>
        {
            // See: https://docs.microsoft.com/powerapps/developer/data-platform/webapi/compose-http-requests-handle-errors#web-api-url-and-versions
            // TODO: Should be a configuration item once known
            client.BaseAddress = new Uri("https://org97007471.api.crm6.dynamics.com" + "/api/data/v9.2/");
            // consider lowering this timeout
            client.Timeout = new TimeSpan(0, 2, 0);
        });

        builder.Services.AddTransient<IDataverseWebApiService, DataverseWebApiService>();
    }
}
