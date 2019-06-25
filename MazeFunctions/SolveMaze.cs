using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            string steps = req.Query["steps"];

            if (steps == null)
            {
                return new BadRequestErrorMessageResult("No steps provided");
            }

            var mazeData = mazeDatas.FirstOrDefault();
            if (mazeData == null)
            {
                return new BadRequestErrorMessageResult($"No maze found for the given Id.");
            }

            var (solved, message) = Solve(mazeData, steps);

            var result = new
            {
                Solved = solved,
                Message = message
            };

            return new OkObjectResult(JsonConvert.SerializeObject(result));
        }

        public static (bool solved, string errorMessage) Solve(MazeData mazeData, string steps)
        {
            var stepCount = 0;
            var x = 0;
            var y = 0;


            foreach (var c in steps)
            {
                stepCount++;

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
                    else
                    {
                        return (false, $"Last step ran into a wall. LastX={x}, LastY={y}, StepsTaken={steps.Substring(0, stepCount)}, StepsSubmitted={steps}");
                    }
                }
                else
                {
                    return (false, $"Last step exceeded dimensions of the maze. LastX={x}, LastY={y}, StepsTaken={steps.Substring(0, stepCount)}, StepsSubmited={steps}");
                }
            }

            var solved = x == mazeData.Dimensions.width - 1 && y == mazeData.Dimensions.height - 1;
            if (solved)
            {
                var password = Environment.GetEnvironmentVariable("MAZE_PASSWORD");
                return (true, password);
            }
            else
            {
                return (false,
                    $"Executed all steps but did not arrive at desired location of X={mazeData.Dimensions.width - 1}, Y={mazeData.Dimensions.height - 1}. LastX={x}, LastY={y}, StepsTaken={steps}");
            }
        }
    }
}