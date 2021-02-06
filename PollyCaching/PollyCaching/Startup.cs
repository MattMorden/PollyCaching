using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyCaching
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
            services.AddControllers();

            #region PollyCaching
            services.AddMemoryCache();
            services.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();

            services.AddSingleton<IReadOnlyPolicyRegistry<string>, PolicyRegistry>((serviceProvider) =>
            {
                // Configure DI with the entire PolicyRegistry, with the assumption that there will be multiple policies used.
                PolicyRegistry registry = new PolicyRegistry
                {
                    {
                        "myCachePolicy",
                        Policy.CacheAsync<HttpResponseMessage>(
                        serviceProvider
                        .GetRequiredService<IAsyncCacheProvider>()
                        .AsyncFor<HttpResponseMessage>(),
                        TimeSpan.FromMinutes(5))
                    },
                    {
                        "circuitBreakerPolicy",
                        Policy.Handle<HttpRequestException>()
                        .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1)) // Circuit remains broken for 1 minute
                    }
                };
                return registry;
            });
            #endregion

            services.AddHttpClient("PollyTest", client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }          

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
