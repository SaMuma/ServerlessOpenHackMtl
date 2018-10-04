using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace BFYOC.Ratings
{
    public static class RatingsFunction
    {
        [FunctionName("CreateRating")]
        public static async Task<HttpResponseMessage> CreateRating(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req,
            [DocumentDB(
                databaseName:"bfyoc", 
                collectionName:"ratings", 
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Ratings> ratingsOut, TraceWriter log)

        {
            log.Info("C# HTTP trigger function processed a request.");

            var client = new HttpClient();

            //parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            //get request body
            string requestBody = await req.Content.ReadAsStringAsync();
            var rating = JsonConvert.DeserializeObject<Ratings>(requestBody);

            //check User 
            var validUser = ValidUser(client, rating.userId);
            if (!validUser)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Bad user");
            }

            //check product
            var validProduct = ValidProduct(client, rating.productId);
            if (!validProduct)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Bad Product");
            }

            //create a guid for the rating
            rating.id = Guid.NewGuid().ToString();

            //create a timestamp for the rating
            rating.timestamp = DateTime.UtcNow.ToString();

            //add Rating to the DB
            await ratingsOut.AddAsync(rating);

            //return a message of successful add 
            return req.CreateResponse(HttpStatusCode.OK, "Rating successfully submitted");
        }

        //Valid User Method
        private static bool ValidUser(HttpClient client, string userId)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = client.GetAsync($"{Settings.GetUserUrl}?userId={userId}").Result;
            return result.StatusCode == HttpStatusCode.OK;
        }

        //ValidProduct Method
        private static bool ValidProduct(HttpClient client, string productId)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = client.GetAsync($"{Settings.GetProductUrl}?productId={productId}").Result;

            return result.StatusCode == HttpStatusCode.OK;

        }

        //[FunctionName("GetAllRatings")]
        //public static HttpResponseMessage GetAllRatings(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllRatings")]HttpRequestMessage req,
        //    [DocumentDB(
        //        databaseName: "bfyoc",
        //        collectionName: "ratings",
        //        ConnectionStringSetting = "CosmosDBConnection",
        //        SqlQuery = "SELECT * FROM c")] IEnumerable<Ratings> ratings, TraceWriter log)
        //{
        //    log.Info("C# HTTP trigger function processed a request.");

        //    return req.CreateResponse(HttpStatusCode.OK, ratings, "application/json");
        //}

        [FunctionName("GetRatings")]
        public static HttpResponseMessage GetRatings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetRatings/{userId}")]HttpRequestMessage req,
        [DocumentDB(
                databaseName: "bfyoc",
                collectionName: "ratings",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c WHERE c.UserId={userId}")] IEnumerable<Ratings> rating, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            return rating == null
                ? req.CreateResponse(HttpStatusCode.NotFound, "Rating not Found", "application/Json")
                : req.CreateResponse(HttpStatusCode.OK, rating, "application/json");

        }

        [FunctionName("GetRating")]
        public static HttpResponseMessage GetRating(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetRating/{id}")]HttpRequestMessage req,
            [DocumentDB(
                databaseName: "bfyoc",
                collectionName: "ratings",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "{id}")] Ratings rating, TraceWriter log)
        {
            log.Info("#C HTTP Trigger Function Processed a Request");
            return rating == null
                ? req.CreateResponse(HttpStatusCode.NotFound, "Rating not Found", "application/Json")
                : req.CreateResponse(HttpStatusCode.OK, rating, "application/json");


        }
    }
}