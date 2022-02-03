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

namespace FunctionApp1;

public class HttpBlobIngester
{
    private readonly ILogger<Function1> _log;
    private readonly IConfiguration _configuration;

    public HttpBlobIngester(ILogger<Function1> log, IConfiguration configuration)
    {
        _log = log;
        _configuration = configuration;
    }

    // TODO: Do we want this inline?
    // TODO: also doesn't allow people to upload through Swagger FWIW
    [OpenApiOperation(operationId: "run", tags: new[] {"multipartformdata"}, Summary = "Transfer image through multipart/formdata", Description = "This transfers an image through multipart/formdata.",
        Visibility = OpenApiVisibilityType.Advanced)]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(MultiPartFormDataModel), Required = true, Description = "Image data")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "image/png", bodyType: typeof(byte[]), Summary = "Image data", Description = "This returns the image", Deprecated = false)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "uploadFile")] HttpRequest req)
    {
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
