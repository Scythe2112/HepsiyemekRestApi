using HepsiyemekRestApi.Helpers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HepsiyemekRestApi.Models
{
    public class Product
    {
        [JsonConverter(typeof(StringToObjectId))]
        public ObjectId id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string categoryId { get; set; }
        [BsonIgnore]
        public Category category { get; set; }
        public float price { get; set; }
        public string currency { get; set; }
    }
}
