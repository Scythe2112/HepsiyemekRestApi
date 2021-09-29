using HepsiyemekRestApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HepsiyemekRestApi.Services
{
    public interface IProductService
    {
        public Product Get(string id);
        public Product Create(Product category);
        public Product Update(string id, Product category);
        public Product Delete(string id);
    }
}
