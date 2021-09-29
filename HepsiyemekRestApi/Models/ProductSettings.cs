﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HepsiyemekRestApi.Models
{
    public class ProductSettings: IProductSettings
    {
        public string CollectionName { get; set; }
        public int Expire { get; set; }
        public string RedisKey { get; set; }
    }
}
