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
            string steps = req.Query["steps"];

            var password = Environment.GetEnvironmentVariable("MAZE_PASSWORD");

            var mazeData = mazeDatas.FirstOrDefault();
            if (mazeData == null)
            {
                return new BadRequestErrorMessageResult($"No maze found for the given Id.");
            }


            if (steps == null)
            {
                var result = new
                {
                    Solved = false,
                    Password = password,
                    Message = "Did not attempt to solve",
                    MazeData = mazeData
                };

                return new OkObjectResult(JsonConvert.SerializeObject(result));
            }
            else
            {
                var (solved, message) = SolveMaze.Solve(mazeData, steps);

                var result = new
                {
                    Solved = solved,
                    Password = password,
                    Message = message,
                    MazeData = mazeData,
                    MapString = mazeData.ToMapString(),
                };

                return new OkObjectResult(JsonConvert.SerializeObject(result));
            }
        }
    }
}