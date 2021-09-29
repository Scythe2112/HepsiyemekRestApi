using HepsiyemekRestApi.Models;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HepsiyemekRestApi.Services
{
    public class CategoryService
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

        private async Task<Category> Find(string id)
        {
            var cachedCategories = new List<Category>();
            Category category;
            string json;

            var categoriesFromCache = await _distributedCache.GetAsync(_categorySettings.RedisKey);
            if (categoriesFromCache != null)
            {
                json = Encoding.UTF8.GetString(categoriesFromCache);
                cachedCategories = JsonConvert.DeserializeObject<List<Category>>(json);
                category = cachedCategories.FirstOrDefault(x => x.id.ToString() == id);

                if (category == null)
                {
                    var filter = Builders<Category>.Filter.Eq("id", id);
                    category = _mongoCollection.Find<Category>(filter).FirstOrDefault();

                    if (category != null)
                    {
                        cachedCategories.Add(category);

                        await AddToCache(cachedCategories);
                    }
                }
            }
            else
            {
                var filter = Builders<Category>.Filter.Eq("id", id);
                category = _mongoCollection.Find<Category>(filter).FirstOrDefault();

                if (category != null)
                {
                    cachedCategories.Add(category);

                    await AddToCache(cachedCategories);
                }
            }

            return category;
        }

        private async Task AddToCache(IEnumerable<Category> categories)
        {
            string json = JsonConvert.SerializeObject(categories);
            var categoriesBytes = Encoding.UTF8.GetBytes(json);
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(_categorySettings.Expire));
            
            await _distributedCache.SetAsync(_categorySettings.RedisKey, categoriesBytes, options);
        }
    }
}
