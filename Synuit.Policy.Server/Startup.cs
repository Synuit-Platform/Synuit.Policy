using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Synuit.Platform.Auth.Types;
using Synuit.Policy.Data.Services;
using Synuit.Policy.Data.Services.Storage;
using Synuit.Toolkit.Infra.Helpers;
using System;
using System.IO;
using System.Reflection;

namespace Synuit.Policy.Server
{
   /// <summary>
   ///
   /// </summary>
   public class Startup
   {
      private readonly IConfiguration _configuration;
      private readonly IWebHostEnvironment _environment;
      private ILogger<Startup> _logger;
      private IServiceProvider _provider;
 
      private StartupConfig _startupConfig;

      public IConfiguration Configuration { get { return _configuration; } }

      public Startup(IWebHostEnvironment environment, IConfiguration configuration)
      {
         _configuration = configuration;
         _environment = environment;
         //
         _startupConfig = StartupHelper.LoadStartupConfig(_configuration);
      }

      /// <summary>
      /// This method gets called by the runtime. Use this method to add services to the container.
      /// </summary>
      /// <param name="services"></param>
      public void ConfigureServices(IServiceCollection services)
      {
         var provider = services.BuildServiceProvider();
         _provider = provider;
         _logger = provider.GetRequiredService<ILogger<Startup>>();

         _logger.LogDebug("Adding Synuit.Context.Server DI container services in " + nameof(Startup));


         // --> add controllers and common Api setup (Synuit.Toolkit)
         services.AddControllersAndCommonServices
            (
               _configuration, 
               _startupConfig, 
               apiTitle: "Synuit Policy Server", 
               apiVersion: "v1", 
               withViews: true
               );

         //services = (_startupConfig.ExecutionEngine) ? services.AddExecutionEngine(_configuration) : services; services in " + nameof(Startup));

        

         ////// --> add database / storage (SEE EXTENSIONS BELOW)
         ////services.ConfigureDatabases(this._configuration, provider);

         ////// --> add main services for api (SEE EXTENSIONS BELOW)
         ////services.AddServices(this._configuration);



         // --> Add authentication
         if (Configuration["ApiAuthConfig:AuthType"] == "Oidc")
         {
            services.AddAuthenticationForApi(Configuration, _environment);
         }
         // --> In-Memory Cache
         services.AddEasyCaching(setupAction =>
         {
            setupAction.UseInMemory();
         });

      
 

         services.AddSingleton<IPolicyRepository, PolicyFileStorageRepository>();
         services.AddSingleton<IPolicyService, PolicyService>();
         

         _logger.LogDebug("Completed adding Synuit.Policy.Server DI container services in " + nameof(Startup));
      }

      /// <summary>
      /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      /// </summary>
      /// <param name="app"></param>
      /// <param name="env"></param>
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         
         //
         if (Configuration["ApiAuthConfig:AuthType"] == "Oidc")
         {
            app.UseAuthentication();
         }

         app.ConfigureApplication(env, _configuration, _startupConfig, _logger);

        

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
         });
      }

     
   }
}