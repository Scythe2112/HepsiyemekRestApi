using HepsiyemekRestApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HepsiyemekRestApi.Services
{
    public interface ICategoryService
    {
        public Category Get(string id);
        public void Create(Category category);
        public void Update(string id, Category category);
        public void Delete(string id);
    }
}
