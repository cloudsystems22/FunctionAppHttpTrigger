using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System;
using FunctionAppHttpTrigger.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FunctionAppHttpTrigger
{
    public static class Function1
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "favoriteBooksHub")] SignalRConnectionInfo connectionInfo, ILogger log)
        {
            log.LogInformation($"Return connection: {connectionInfo.Url} - {connectionInfo.AccessToken}"); 
            return connectionInfo;
        }

        [FunctionName("GetFavoriteBooks")]
        public static void Run([TimerTrigger("0 * * * * *")] TimerInfo timerInfo,
            [SignalR(HubName = "favoriteBooksHub")] IAsyncCollector<SignalRMessage> signalRMessage,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function: {DateTime.Now}");

            var myFavoriteBooks = GoodReadService.GetBooksFromGoodReads(log, "171980909", "recomended");

            signalRMessage.AddAsync(
                new SignalRMessage
                {
                    Target = "myFavoriteBooks",
                    Arguments = new[] { myFavoriteBooks }
                });
        }

        [FunctionName("booklist")]
        public static async Task<IActionResult> BookList(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [SignalR(HubName = "favoriteBooksHub")] IAsyncCollector<SignalRMessage> signalRMessage,
            ILogger log)
        {
            log.LogInformation($"C# Action trigger function: {DateTime.Now}");

            var myFavoriteBooks = GoodReadService.GetBooksFromGoodReads(log, "171980909", "recomended");

            await signalRMessage.AddAsync(
                new SignalRMessage
                {
                    Target = "myFavoriteBooks",
                    Arguments = new[] { myFavoriteBooks }
                });

            return new OkObjectResult($"Ok ótimo!");
        }


    }
}
