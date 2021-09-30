using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HepsiyemekRestApi.Models;
using HepsiyemekRestApi.Services;
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
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("{_id}")]
        public JsonResult Get(string _id)
        {
            var category = _categoryService.Get(_id);

            return new JsonResult(category);
        }

        [HttpPost]
        public JsonResult Post(Category category)
        {
            _categoryService.Create(category);

            return new JsonResult("Added Successfully");
        }

        [HttpPut("{_id}")]
        public JsonResult Put(string _id, Category category)
        {
            _categoryService.Update(_id, category);

            return new JsonResult("Updated Successfully");
        }

        [HttpDelete("{_id}")]
        public JsonResult Delete(string _id)
        {
            _categoryService.Delete(_id);

            return new JsonResult("Deleted Successfully");
        }
    }
}
