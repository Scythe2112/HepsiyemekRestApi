using HepsiyemekRestApi.Models;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HepsiyemekRestApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IProductSettings _productSettings;
        private readonly ICategorySettings _categorySettings;
        private readonly IDatabaseSettings _databaseSettings;
        private readonly IMongoCollection<Product> _mongoCollectionProduct;
        private readonly IMongoCollection<Category> _mongoCollectionCategory;

        public ProductService(IDistributedCache distributedCache, IProductSettings productSettings, IDatabaseSettings databaseSettings, ICategorySettings categorySettings)
        {
            _distributedCache = distributedCache;
            _productSettings = productSettings;
            _databaseSettings = databaseSettings;
            _categorySettings = categorySettings;

            var client = new MongoClient(_databaseSettings.ConnectionString);
            _mongoCollectionProduct = client.GetDatabase(_databaseSettings.DatabaseName).GetCollection<Product>(_productSettings.CollectionName);
            _mongoCollectionCategory = client.GetDatabase(_databaseSettings.DatabaseName).GetCollection<Category>(_categorySettings.CollectionName);
        }

        private Product Find(string id)
        {
            var cachedProducts = new List<Product>();
            Product product;
            string json;

            var productsFromCache = _distributedCache.Get(_productSettings.RedisKey);
            if (productsFromCache != null)
            {
                json = Encoding.UTF8.GetString(productsFromCache);
                cachedProducts = JsonConvert.DeserializeObject<List<Product>>(json);
                product = cachedProducts.FirstOrDefault(x => x.id == new ObjectId(id));

                if (product == null)
                {
                    var filter = Builders<Product>.Filter.Eq("id", new ObjectId(id));
                    product = _mongoCollectionProduct.Find(filter).FirstOrDefault();

                    if (product != null)
                    {
                        cachedProducts.Add(product);

                        AddToCache(cachedProducts);
                    }
                }
            }
            else
            {
                var filter = Builders<Product>.Filter.Eq("id", new ObjectId(id));
                product = _mongoCollectionProduct.Find(filter).FirstOrDefault();

                if (product != null)
                {
                    cachedProducts.Add(product);

                    AddToCache(cachedProducts);
                }
            }

            if (product != null)
            {
                var filter = Builders<Category>.Filter.Eq("id", new ObjectId(product.categoryId));

                product.category = _mongoCollectionCategory.Find(filter).FirstOrDefault();
            }

            return product;
        }

        private void AddToCache(IEnumerable<Product> products)
        {
            string json = JsonConvert.SerializeObject(products);
            var productsBytes = Encoding.UTF8.GetBytes(json);
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(_productSettings.Expire));

             _distributedCache.Set(_productSettings.RedisKey, productsBytes, options);
        }

        private void RemoveFromCache(string id)
        {
            var cachedProducts = new List<Product>();
            Product product;
            string json;

            var productsFromCache = _distributedCache.Get(_productSettings.RedisKey);
            if (productsFromCache != null)
            {
                json = Encoding.UTF8.GetString(productsFromCache);
                cachedProducts = JsonConvert.DeserializeObject<List<Product>>(json);
                product = cachedProducts.FirstOrDefault(x => x.id == new ObjectId(id));

                if (product != null)
                {
                    cachedProducts.Remove(product);

                    AddToCache(cachedProducts);
                }
            }
        }

        public Product Get(string id)
        {
            return Find(id);
        }

        public void Create(Product product)
        {
            _mongoCollectionProduct.InsertOne(product);
        }

        public void Update(string id, Product product)
        {
            var filter = Builders<Product>.Filter.Eq("id", new ObjectId(id));
            var update = Builders<Product>.Update
                        .Set("name", product.name)
                        .Set("description", product.description)
                        .Set("categoryId", product.categoryId)
                        .Set("currency", product.currency)
                        .Set("price", product.price);

            _mongoCollectionProduct.UpdateOne(filter, update);
        }

        public void Delete(string id)
        {
            var filter = Builders<Product>.Filter.Eq("id", new ObjectId(id));

            _mongoCollectionProduct.DeleteOne(filter);

            RemoveFromCache(id);
        }
    }
}

