using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HepsiyemekRestApi.Models;
using HepsiyemekRestApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HepsiyemekRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{_id}")]
        public JsonResult Get(string _id)
        {
            var product = _productService.Get(_id);

            return new JsonResult(product);
        }

        [HttpPost]
        public JsonResult Post(Product product)
        {
            _productService.Create(product);

            return new JsonResult("Added Successfully");
        }

        [HttpPut("{_id}")]
        public JsonResult Put(string _id, Product product)
        {
            _productService.Update(_id, product);

            return new JsonResult("Updated Successfully");
        }

        [HttpDelete("{_id}")]
        public JsonResult Delete(string _id)
        {
            _productService.Delete(_id);

            return new JsonResult("Deleted Successfully");
        }
    }
}
