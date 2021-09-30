using HepsiyemekRestApi.Models;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace HepsiyemekRestApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ICategorySettings _categorySettings;
        private readonly IDatabaseSettings _databaseSettings;
        private readonly IMongoCollection<Category> _mongoCollection;

        public CategoryService(IDistributedCache distributedCache, ICategorySettings categorySettings, IDatabaseSettings databaseSettings)
        {
            _distributedCache = distributedCache;
            _categorySettings = categorySettings;
            _databaseSettings = databaseSettings;

            var client = new MongoClient(_databaseSettings.ConnectionString);
            _mongoCollection = client.GetDatabase(_databaseSettings.DatabaseName).GetCollection<Category>(_categorySettings.CollectionName);
        }

        private Category Find(string id)
        {
            var cachedCategories = new List<Category>();
            Category category;
            string json;

            var categoriesFromCache = _distributedCache.Get(_categorySettings.RedisKey);
            if (categoriesFromCache != null)
            {
                json = Encoding.UTF8.GetString(categoriesFromCache);
                cachedCategories = JsonConvert.DeserializeObject<List<Category>>(json);
                category = cachedCategories.FirstOrDefault(x => x.id == new ObjectId(id));

                if (category == null)
                {
                    var filter = Builders<Category>.Filter.Eq("id", new ObjectId(id));
                    category = _mongoCollection.Find(filter).FirstOrDefault();

                    if (category != null)
                    {
                        cachedCategories.Add(category);

                        AddToCache(cachedCategories);
                    }
                }
            }
            else
            {
                var filter = Builders<Category>.Filter.Eq("id", new ObjectId(id));
                category = _mongoCollection.Find(filter).FirstOrDefault();

                if (category != null)
                {
                    cachedCategories.Add(category);

                    AddToCache(cachedCategories);
                }
            }

            return category;
        }

        private void RemoveFromCache(string id)
        {
            var cachedCategories = new List<Category>();
            Category category;
            string json;

            var categoriesFromCache = _distributedCache.Get(_categorySettings.RedisKey);
            if (categoriesFromCache != null)
            {
                json = Encoding.UTF8.GetString(categoriesFromCache);
                cachedCategories = JsonConvert.DeserializeObject<List<Category>>(json);
                category = cachedCategories.FirstOrDefault(x => x.id == new ObjectId(id));

                if (category != null)
                {
                    cachedCategories.Remove(category);

                    AddToCache(cachedCategories);
                }
            }
        }

        private void AddToCache(IEnumerable<Category> categories)
        {
            string json = JsonConvert.SerializeObject(categories);
            var categoriesBytes = Encoding.UTF8.GetBytes(json);
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(_categorySettings.Expire));
            
            _distributedCache.Set(_categorySettings.RedisKey, categoriesBytes, options);
        }

        public Category Get(string id)
        {
            return Find(id);
        }

        public void Create(Category category)
        {
            _mongoCollection.InsertOne(category);
        }

        public void Update(string id, Category category)
        {
            var filter = Builders<Category>.Filter.Eq("id", new ObjectId(id));
            var update = Builders<Category>.Update.Set("name", category.name).Set("description", category.description);

            _mongoCollection.UpdateOne(filter, update);
        }

        public void Delete(string id)
        {
            var filter = Builders<Category>.Filter.Eq("id", new ObjectId(id));

            _mongoCollection.DeleteOne(filter);

            RemoveFromCache(id);
        }
    }
}
