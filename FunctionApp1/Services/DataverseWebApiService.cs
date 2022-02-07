using System.Net;
using System.Net.Http.Headers;
using FunctionApp1.Extensions;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;

namespace FunctionApp1.Services;

// Adapted from CDSWebApiService Microsoft Sample
public class DataverseWebApiService : IDataverseWebApiService
{
    public DataverseWebApiService()
    {
        // TODO: add logging
    }

    public async Task<T> GetAsync<T>(string path, Dictionary<string, List<string>> headers = null)
    {
        return (await GetAsync(path, headers)).ToObject<T>();
    }

    private static HttpClient CreateHttpClient()
    {
        string resource = "https://org97007471.api.crm6.dynamics.com";

        // Azure Active Directory app registration shared by all Power App samples.
        // For your custom apps, you will need to register them with Azure AD yourself.
        // See https://docs.microsoft.com/powerapps/developer/data-platform/walkthrough-register-app-azure-active-directory
        var clientId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
        var redirectUri = "http://localhost"; // Loopback for the interactive login.


        // TODO: different kind of auth here
        var authBuilder = PublicClientApplicationBuilder.Create(clientId)
            .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
            .WithRedirectUri(redirectUri)
            .Build();
        var scope = resource + "/.default";
        string[] scopes = {scope};

        AuthenticationResult token =
            authBuilder.AcquireTokenInteractive(scopes).ExecuteAsync().Result;


        var client = new HttpClient {
            // See https://docs.microsoft.com/powerapps/developer/data-platform/webapi/compose-http-requests-handle-errors#web-api-url-and-versions
            BaseAddress = new Uri(resource + "/api/data/v9.2/"),
            Timeout = new TimeSpan(0, 2, 0) // Standard two minute timeout on web service calls.
        };

        // Default headers for each Web API call.
        // See https://docs.microsoft.com/powerapps/developer/data-platform/webapi/compose-http-requests-handle-errors#http-headers
        HttpRequestHeaders headers = client.DefaultRequestHeaders;
        headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        headers.Add("OData-MaxVersion", "4.0");
        headers.Add("OData-Version", "4.0");
        headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private static async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead,
        int retryCount = 0)
    {
        HttpResponseMessage response;
        try
        {
            using var httpClient = CreateHttpClient();
            //The request is cloned so it can be sent again.
            response = await httpClient.SendAsync(request.Clone(), httpCompletionOption);
        }
        catch (Exception)
        {
            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            if ((int)response.StatusCode != 429)
            {
                //Not a service protection limit error
                throw ParseError(response);
            }
            else
            {
                // Give up re-trying if exceeding the maxRetries
                //if (++retryCount >= config.MaxRetries)
                // TODO: do something about this
                if (++retryCount >= 2)
                {
                    throw ParseError(response);
                }

                int seconds;
                //Try to use the Retry-After header value if it is returned.
                if (response.Headers.Contains("Retry-After"))
                {
                    seconds = int.Parse(response.Headers.GetValues("Retry-After").FirstOrDefault());
                }
                else
                {
                    //Otherwise, use an exponential backoff strategy
                    seconds = (int)Math.Pow(2, retryCount);
                }

                await Task.Delay(TimeSpan.FromSeconds(seconds));

                return await SendAsync(request, httpCompletionOption, retryCount);
            }
        }
        else
        {
            return response;
        }
    }

    public Uri PostCreate(string entitySetName, object body)
    {
        try
        {
            return PostCreateAsync(entitySetName, body).GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Retrieves data from a specified resource asychronously.
    /// </summary>
    /// <param name="path">The path to the resource.</param>
    /// <param name="headers">Any custom headers to control optional behaviors.</param>
    /// <returns>The response to the request.</returns>
    public static async Task<JToken> GetAsync(string path, Dictionary<string, List<string>> headers = null)
    {
        try
        {
            using (var message = new HttpRequestMessage(HttpMethod.Get, path))
            {
                if (headers != null)
                {
                    foreach (KeyValuePair<string, List<string>> header in headers)
                    {
                        message.Headers.Add(header.Key, header.Value);
                    }
                }

                using (HttpResponseMessage response = await SendAsync(message, HttpCompletionOption.ResponseContentRead))
                {
                    if (response.StatusCode != HttpStatusCode.NotModified)
                    {
                        return JToken.Parse(await response.Content.ReadAsStringAsync());
                    }

                    return null;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Uri> PostCreateAsync(string entitySetName, object body)
    {
        try
        {
            using (var message = new HttpRequestMessage(HttpMethod.Post, entitySetName))
            {
                message.Content = new StringContent(JObject.FromObject(body).ToString());
                message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                using (HttpResponseMessage response = await SendAsync(message))
                {
                    return new Uri(response.Headers.GetValues("OData-EntityId").FirstOrDefault());
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private static ServiceException ParseError(HttpResponseMessage response)
    {
        try
        {
            int code = 0;
            string message = "no content returned",
                content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (content.Length > 0)
            {
                var errorObject = JObject.Parse(content);
                message = errorObject["error"]["message"].Value<string>();
                code = Convert.ToInt32(errorObject["error"]["code"].Value<string>(), 16);
            }

            int statusCode = (int)response.StatusCode;
            string reasonPhrase = response.ReasonPhrase;

            return new ServiceException(code, statusCode, reasonPhrase, message);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
