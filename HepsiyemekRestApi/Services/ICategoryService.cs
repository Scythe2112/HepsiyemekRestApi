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
        public Category Create(Category category);
        public Category Update(string id, Category category);
        public Category Delete(string id);
    }
}
