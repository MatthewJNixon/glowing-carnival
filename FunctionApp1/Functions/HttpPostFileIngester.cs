using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace FunctionApp1.Functions;

public class HttpPostFileIngester
{
    private readonly ILogger<Function1> _log;
    private readonly IConfiguration _configuration;

    public HttpPostFileIngester(ILogger<Function1> log, IConfiguration configuration)
    {
        _log = log;
        _configuration = configuration;
    }

    // TODO: Do we want this inline?
    // TODO: also doesn't allow people to upload through Swagger FWIW
    [FunctionName("HttpBlobIngester")]
    [OpenApiOperation(operationId: "run", tags: new[] {"HttpBlobIngester"}, Summary = "Transfer document through multipart/formdata", Description = "This transfers document through multipart/formdata.",
        Visibility = OpenApiVisibilityType.Advanced)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(MultiPartFormDataModel), Required = true, Description = "Image data")]
    // testing this - NO
    [OpenApiParameter("file", Type = typeof(MultiPartFormDataModel), In = ParameterLocation.Query, Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/html", bodyType: typeof(byte[]), Summary = "Document Details", Description = "This returns the details of the document", Deprecated = false)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "uploadFile")] HttpRequest req)
    {
        // useful class? Doesn't seem to be supported for v4 !!
        // Microsoft.OpenApi.Models.OpenApiRequestBody

        try
        {
            var formdata = await req.ReadFormAsync();
            var file = req.Form.Files["file"];

            // TODO: Something useful
            return new OkObjectResult(file.FileName + " - " + file.Length.ToString());
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }
    }

    // Stackoverflow :D
    public class MultiPartFormDataModel
    {
        public byte[] FileUpload { get; set; }
    }
}
