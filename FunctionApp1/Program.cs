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
        builder.Services.AddTransient<IDataverseWebApiService, DataverseWebApiService>();
    }
}
