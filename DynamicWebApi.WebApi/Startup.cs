/* Copyright Chetan N Mandhania */
using DynamicWebApi.Common.Helpers;
using DynamicWebApi.DAL;
using DynamicWebApi.IRepository;
using DynamicWebApi.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace DynamicWebApi.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            DBContext.Configuration = configuration;
            Global.Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            this.BootStrapRepositories(services);
            services.AddAuthentication(IISDefaults.AuthenticationScheme);
            services.Configure<IISOptions>(options => options.AutomaticAuthentication = true);
            services.AddCors(options => { options.AddPolicy("CorsPolicy", builder => builder.WithOrigins(Global.Configuration?.GetSection("CorsHost").Value.Split(';')).SetIsOriginAllowedToAllowWildcardSubdomains().AllowCredentials().AllowAnyHeader().AllowAnyMethod()); });
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression(options => { options.Providers.Add<GzipCompressionProvider>(); options.MimeTypes = new[] { "text/plain", "text/css", "application/javascript", "text/html", "application/xml", "text/xml", "application/json", "text/json", "imagesvg+xml" }; });
            services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None; options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize; options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented; options.SerializerSettings.Converters.Add(new StringEnumConverter()); });
        }

        private void BootStrapRepositories(IServiceCollection services)
        {
            _ = services.AddScoped<IDynamicWebApiRepository, DynamicWebApiRepository>();
            _ = services.AddScoped<IDBContext, DBContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = env.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseHsts();
            app.UseMiddleware<ExceptionMiddleware>().UseHttpsRedirection().UseResponseCompression().UseAuthentication().UseRouting().UseCors("CorsPolicy").UseAuthorization().UseEndpoints(e => { e.MapControllers(); });
        }
    }
}
