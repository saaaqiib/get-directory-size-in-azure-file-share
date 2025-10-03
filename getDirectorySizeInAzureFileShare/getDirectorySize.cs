using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace getDirectorySizeInAzureFileShare;

public class getDirectorySize
{
    [Function("getDirectorySizeInAzureFileShare")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {

        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        
        //In my use case, I need to look inside a specific directory name in the file share that is given to this function app as a parameter when it is triggered via a GET request

        var userDir = query["user"];

        if (userDir == null)
        {
            userDir = "";
        }

        string shareName = Environment.GetEnvironmentVariable("FILE_SHARE_NAME");
        string directoryPath = Environment.GetEnvironmentVariable("DIRECTORY_PATH"); //This is the base path, the userDir will be added to it later to complete the path
        string accountName = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME");
        string accountKey = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_KEY");


        string directoryPathFinal = $"{directoryPath}/{userDir}";
        var credential = new StorageSharedKeyCredential(accountName, accountKey);

        // File endpoint 
        string fileServiceUri = $"https://{accountName}.file.core.windows.net/{shareName}";

        var shareClient = new ShareClient(new Uri(fileServiceUri), credential);
        var dirClient = shareClient.GetDirectoryClient(directoryPathFinal);

        long totalBytes = 0;
        await foreach (ShareFileItem item in dirClient.GetFilesAndDirectoriesAsync())
        {
            if (!item.IsDirectory)
            {
                var fileClient = dirClient.GetFileClient(item.Name);
                ShareFileProperties props = await fileClient.GetPropertiesAsync();
                totalBytes += props.ContentLength;
            }
        }

        double megabytes = totalBytes / (1024.0 * 1024.0); // Converting bytes to Megabytes
        int result = (int)megabytes;

        var response = req.CreateResponse(HttpStatusCode.OK); 
        await response.WriteAsJsonAsync(new { totalSizeBytes = result }); //return as JSON
        return response;
    }
}