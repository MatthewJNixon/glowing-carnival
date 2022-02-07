using FunctionApp1.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable CS0657
[assembly: FunctionsStartup(typeof(FunctionApp1.Program))]
#pragma warning restore CS0657
namespace FunctionApp1;

public class Program : FunctionsStartup
{

    public override void Configure(IFunctionsHostBuilder builder)
    {

        // builder.Services.AddTransient<IDataverseWebApiService>(_ => new DataverseWebApiService());
        builder.Services.AddTransient<IDataverseWebApiService, DataverseWebApiService>();
    }
}
