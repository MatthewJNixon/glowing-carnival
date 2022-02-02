using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using static System.String;

namespace FunctionApp1;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly IConfiguration _configuration;

    public Function1(ILogger<Function1> log, IConfiguration configuration)
    {
        _logger = log;
        _configuration = configuration;
    }

    [FunctionName("MattsTestFunction")]
    [OpenApiOperation(operationId: "Run", tags: new[] {"name"})]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request");
        // proving this can be done I guess?
        var env = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");

        // can't seem to import the libraries correctly, although this method of logging should be deprecated
        // var logger = executionContext.GetLogger("HttpFunction");
        // logger.LogInformation("message logged");

        // Logging not working despite the DI working with just these lines

        string name = req.Query["name"];

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        name ??= data?.name;

        var responseMessage = IsNullOrEmpty(name)
            ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            : Format("Hello, {name}. This HTTP triggered function executed successfully.", name);

        // huh not working
        _logger.LogInformation(responseMessage);

        return new OkObjectResult(responseMessage);
    }
}
