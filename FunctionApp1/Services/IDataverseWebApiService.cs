namespace FunctionApp1.Services;

public interface IDataverseWebApiService
{
    // do we even need this?
    Uri PostCreate(string entitySetName, object body);
    Task<Uri> PostCreateAsync(string entitySetName, object body);
    Task<T> GetAsync<T>(string path, Dictionary<string, List<string>> headers = null);



}
