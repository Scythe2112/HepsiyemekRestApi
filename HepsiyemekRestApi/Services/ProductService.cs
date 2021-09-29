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
    public class ProductService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IProductSettings _productSettings;
        private readonly IDatabaseSettings _databaseSettings;
        private readonly IMongoCollection<Product> _mongoCollection;

        public ProductService(IDistributedCache distributedCache, IProductSettings productSettings, IDatabaseSettings databaseSettings)
        {
            _distributedCache = distributedCache;
            _productSettings = productSettings;
            _databaseSettings = databaseSettings;

            var client = new MongoClient(_databaseSettings.ConnectionString);
            _mongoCollection = client.GetDatabase(_databaseSettings.DatabaseName).GetCollection<Product>(_productSettings.CollectionName);
        }

        private async Task<Product> Find(string id)
        {
            var cachedProducts = new List<Product>();
            Product product;
            string json;

            var productsFromCache = await _distributedCache.GetAsync(_productSettings.RedisKey);
            if (productsFromCache != null)
            {
                json = Encoding.UTF8.GetString(productsFromCache);
                cachedProducts = JsonConvert.DeserializeObject<List<Product>>(json);
                product = cachedProducts.FirstOrDefault(x => x.id.ToString() == id);

                if (product == null)
                {
                    var filter = Builders<Product>.Filter.Eq("id", id);
                    product = _mongoCollection.Find(filter).FirstOrDefault();

                    if (product != null)
                    {
                        cachedProducts.Add(product);

                        await AddToCache(cachedProducts);
                    }
                }
            }
            else
            {
                var filter = Builders<Product>.Filter.Eq("id", id);
                product = _mongoCollection.Find(filter).FirstOrDefault();

                if (product != null)
                {
                    cachedProducts.Add(product);

                    await AddToCache(cachedProducts);
                }
            }

            return product;
        }

        private async Task AddToCache(IEnumerable<Product> products)
        {
            string json = JsonConvert.SerializeObject(products);
            var productsBytes = Encoding.UTF8.GetBytes(json);
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(_productSettings.Expire));

            await _distributedCache.SetAsync(_productSettings.RedisKey, productsBytes, options);
        }
    }
}
