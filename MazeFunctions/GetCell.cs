using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MazeFunctions
{
    public static class GetCell
    {
        [FunctionName("GetCell")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetCell/{Id}")]
            HttpRequest req,
            [CosmosDB(databaseName: "mazecosmosdb-dbid",
                collectionName: "container",
                PartitionKey = "/mazepartitionkey",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM container c where c.Id = {Id}")]
            IEnumerable<MazeData> mazeDatas,
            ILogger log)
        {
            int x;
            int y;
            if (int.TryParse(req.Query["x"], out x) && int.TryParse(req.Query["y"], out y))
            {
                var mazeData = mazeDatas.FirstOrDefault();
                if (mazeData == null)
                {
                    return new BadRequestErrorMessageResult($"No maze found for the given Id.");
                }

                if (mazeData.IsExpired())
                {
                    return new BadRequestErrorMessageResult(
                        $"Maze has expired. MazeId={mazeData.Id}, ServerTime={DateTime.Now:G}, ExpiryTime={mazeData.ExpiryTime?.ToString("G")}");
                }

                if (x >= mazeData.Dimensions.width || y >= mazeData.Dimensions.height)
                {
                    return new BadRequestErrorMessageResult($"Dimensions exceeded. (Width, Height)=({mazeData.Dimensions.width}, {mazeData.Dimensions.height}), (x, y)=({x}, {y})");
                }

                var isLand = mazeData.Map[x, y];

                var result = new
                {
                    X = x,
                    Y = y,
                    IsLand = isLand
                };

                return new OkObjectResult(JsonConvert.SerializeObject(result));
            }

            return new BadRequestErrorMessageResult("Bad request");
        }
    }
}