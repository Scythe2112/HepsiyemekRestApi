using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HepsiyemekRestApi.Models;
using HepsiyemekRestApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HepsiyemekRestApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Enable CORS
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyHeader());
            });

            //JSON Serializer
            services.AddControllers()
            .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
            .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());

            services.AddStackExchangeRedisCache(action =>
            {
                action.Configuration = "localhost:6379";
            });

            services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));
            services.AddSingleton<IDatabaseSettings>(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);

            services.Configure<CategorySettings>(Configuration.GetSection(nameof(CategorySettings)));
            services.AddSingleton<ICategorySettings>(sp =>sp.GetRequiredService<IOptions<CategorySettings>>().Value);

            services.Configure<ProductSettings>(Configuration.GetSection(nameof(ProductSettings)));
            services.AddSingleton<IProductSettings>(sp =>sp.GetRequiredService<IOptions<ProductSettings>>().Value);

            services.AddSingleton<ICategoryService, CategoryService>();
            services.AddSingleton<IProductService, ProductService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Enable CORS
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Handle error codes
            UseStatusCodePages(app);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void UseStatusCodePages(IApplicationBuilder application)
        {
            application.UseStatusCodePages(async context =>
            {
                var response = context.HttpContext.Response;
                var message = string.Empty;
                var code = StatusCodes.Status500InternalServerError;

                if (response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    message = "Bu iþlemi yapmak için yetkili deðilsiniz.";
                    code = (int)StatusCodes.Status403Forbidden;
                }
                else if (response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    message = "Bu iþlemi yapmak için giriþ yapmalýsýnýz.";
                    code = (int)StatusCodes.Status401Unauthorized;
                }
                else if (response.StatusCode == StatusCodes.Status404NotFound)
                {
                    message = "Ýlgili method bulunamadý.";
                    code = (int)StatusCodes.Status404NotFound;
                }
                else if (response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    message = "Hatalý istek.";
                    code = (int)StatusCodes.Status400BadRequest;
                }

                response.ContentType = "application/json";
                await response.WriteAsync(JsonConvert.SerializeObject(new { code, message }));
            });
        }
    }
}
