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

namespace MazeFunctions
{
    public static class SolveMaze
    {
        [FunctionName("SolveMaze")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SolveMaze/{Id}")]
            HttpRequest req,
            [CosmosDB(databaseName: "mazecosmosdb-dbid",
                collectionName: "container",
                PartitionKey = "/mazepartitionkey",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM container c where c.Id = {Id}")]
            IEnumerable<MazeData> mazeDatas,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string direction = req.Query["directions"];

            if (direction == null)
            {
                return new BadRequestErrorMessageResult("No direction provided");
            }

            var mazeData = mazeDatas.FirstOrDefault();
            if (mazeData == null)
            {
                return new BadRequestErrorMessageResult("Something fucked up. Contact Linan");
            }

            var solved = Solve(mazeData, direction);

            return new OkObjectResult(mazeData.ToMapString() + Environment.NewLine + direction + Environment.NewLine + $"Result {solved}");
        }

        public static bool Solve(MazeData mazeData, string direction)
        {
            var x = 0;
            var y = 0;


            foreach (var c in direction)
            {
                if (c == 'N')
                {
                    y++;
                }

                if (c == 'S')
                {
                    y--;
                }

                if (c == 'E')
                {
                    x++;
                }

                if (c == 'W')
                {
                    x--;
                }

                if (x >= 0 && x < mazeData.Dimensions.width && y >= 0 && y < mazeData.Dimensions.height)
                {
                    if (mazeData.Map[x, y])
                    {
                        continue;
                    }
                }

                return false;
            }

            return x == mazeData.Dimensions.width - 1 && y == mazeData.Dimensions.height - 1;
        }
    }
}