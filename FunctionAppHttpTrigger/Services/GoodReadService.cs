using FunctionAppHttpTrigger.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace FunctionAppHttpTrigger.Services
{
    public class GoodReadService
    {
        public static IList<Book> GetBooksFromGoodReads(ILogger logger, string userId, string shelf)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(shelf))
                logger.LogError("Parametros inválidos!");

            string urlGoodRead = $"https://www.goodreads.com/review/list/{userId}?ref=nav_mybooks&shelf={shelf}";

            HtmlDocument htmlDoc = new HtmlDocument();

            htmlDoc.OptionFixNestedTags = true;
            IList<Book> books = new List<Book>();

            using(HttpClient client = new HttpClient())
            {
                using(HttpResponseMessage responseMessage = client.GetAsync(urlGoodRead).Result)
                {
                    using(HttpContent content = responseMessage.Content)
                    {
                        string result = content.ReadAsStringAsync().Result;
                        htmlDoc.LoadHtml(result);

                        if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                            logger.LogError("Erro de conversão da página");

                        if(htmlDoc.DocumentNode != null)
                            for(int i = 0; i <  htmlDoc.DocumentNode.SelectNodes("//tr[@class='bookalike review']").Count; i++)
                                books.Add(new Book() 
                                { 
                                    ImagePath = htmlDoc.DocumentNode.SelectNodes("//td[@class='field cover']")[i].ChildNodes[1].ChildNodes[1]
                                    .ChildNodes[1].ChildNodes[0].Attributes["src"].Value, 
                                    Title = htmlDoc.DocumentNode.SelectNodes("//td[@class='field title']")[i].ChildNodes[1].ChildNodes[1]
                                    .Attributes["title"].Value, 
                                    Autor = htmlDoc.DocumentNode.SelectNodes("//td[@class='field author']")[i].ChildNodes[1].ChildNodes[1]
                                    .InnerText
                                });

                    }
                }
            }


            return books;
        }
    }
}
