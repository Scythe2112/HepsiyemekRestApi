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
        public void Create(Product category);
        public void Update(string id, Product category);
        public void Delete(string id);
    }
}
