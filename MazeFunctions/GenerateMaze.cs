using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MazeFunctions
{
    public static class GenerateMaze
    {
        private const int Salt = 69;

        [FunctionName("GenerateMaze")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GenerateMaze")]
            HttpRequest req,
            [CosmosDB(databaseName: "mazecosmosdb-dbid", collectionName: "container", ConnectionStringSetting = "CosmosDBConnection")]
            out dynamic document,
            ILogger log)
        {
            var (maze, mazeData) = Generate((int) DateTime.Now.Ticks, (30, 30));
            log.LogInformation($"Maze created {mazeData.Id}");
            document = mazeData;

            var result = new
            {
                Id = mazeData.Id,
                Width = mazeData.Dimensions.width,
                Height = mazeData.Dimensions.height,
                ServerTime = DateTime.Now.ToString("G"),
                ExpiryTime = mazeData.ExpiryTime?.ToString("G")
            };

            return new OkObjectResult(JsonConvert.SerializeObject(result));
        }

        private static (Maze, MazeData) Generate(int seed, (int width, int height) dimensions)
        {
            int saltySeed;
            unchecked
            {
                saltySeed = seed + Salt;
            }

            var random = new Random(saltySeed);
            var maze = new Maze(dimensions.width, dimensions.height, random);
            var map = maze.ToBools();
            var expirationSeconds = int.Parse(Environment.GetEnvironmentVariable("MAZE_EXPIRATION_SECONDS"));
            var mazeData = new MazeData(Guid.NewGuid().ToString(), (map.GetLength(0), map.GetLength(1)), map, DateTime.Now.AddSeconds(expirationSeconds));
            return (maze, mazeData);
        }
    }
}