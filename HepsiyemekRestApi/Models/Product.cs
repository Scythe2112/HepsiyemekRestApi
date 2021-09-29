using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HepsiyemekRestApi.Models
{
    public class Product
    {
        public ObjectId id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Category categoryId { get; set; }
        public float price { get; set; }
        public string currency { get; set; }
    }
}
