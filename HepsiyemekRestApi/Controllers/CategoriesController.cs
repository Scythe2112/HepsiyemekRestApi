using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HepsiyemekRestApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace HepsiyemekRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CategoriesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            var dbClient = new MongoClient(_configuration.GetConnectionString("HepsiyemekAppCon"));

            var dbList = dbClient.GetDatabase("HepsiyemekDb").GetCollection<Category>("categories").AsQueryable();

            return new JsonResult(dbList);
        }

        [HttpPost]
        public JsonResult Post(Category category)
        {
            var dbClient = new MongoClient(_configuration.GetConnectionString("HepsiyemekAppCon"));

            dbClient.GetDatabase("HepsiyemekDb").GetCollection<Category>("categories").InsertOne(category);

            return new JsonResult("Added Successfully");
        }

        [HttpPut]
        public JsonResult Put(Category category)
        {
            var dbClient = new MongoClient(_configuration.GetConnectionString("HepsiyemekAppCon"));

            var filter = Builders<Category>.Filter.Eq("id", category.id);
            var update = Builders<Category>.Update.Set("name", category.name).Set("description", category.description);

            dbClient.GetDatabase("HepsiyemekDb").GetCollection<Category>("categories").UpdateOne(filter, update);

            return new JsonResult("Updated Successfully");
        }

        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            var dbClient = new MongoClient(_configuration.GetConnectionString("HepsiyemekAppCon"));

            var filter = Builders<Category>.Filter.Eq("id", id);

            dbClient.GetDatabase("HepsiyemekDb").GetCollection<Category>("categories").DeleteOne(filter);

            return new JsonResult("Deleted Successfully");
        }
    }
}
