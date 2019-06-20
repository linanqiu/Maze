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
    public static class DiagnoseMaze
    {
        [FunctionName("DiagnoseMaze")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", Route = "DiagnoseMaze/{Id}")]
            HttpRequest req,
            [CosmosDB(databaseName: "mazecosmosdb-dbid",
                collectionName: "container",
                PartitionKey = "/mazepartitionkey",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM container c where c.Id = {Id}")]
            IEnumerable<MazeData> mazeDatas,
            ILogger log)
        {
            string direction = req.Query["directions"];

            var mazeData = mazeDatas.FirstOrDefault();
            if (mazeData == null)
            {
                return new BadRequestErrorMessageResult("Something fucked up. Contact Linan");
            }

            if (direction == null)
            {
                return new OkObjectResult(mazeData.ToMapString());
            }

            var solved = SolveMaze.Solve(mazeData, direction);

            return new OkObjectResult(mazeData.ToMapString() + Environment.NewLine + direction + Environment.NewLine + $"Result {solved}");
        }
    }
}