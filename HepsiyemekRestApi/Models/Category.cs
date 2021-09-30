using HepsiyemekRestApi.Helpers;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HepsiyemekRestApi.Models
{
    public class Category
    {
        [JsonConverter(typeof(StringToObjectId))]
        public ObjectId id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}
