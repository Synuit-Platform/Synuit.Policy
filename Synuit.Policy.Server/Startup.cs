using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Synuit.Platform.Auth.Types;
using Synuit.Policy.Data.Services;
using Synuit.Policy.Data.Services.Storage;
using Synuit.Toolkit.Infra.Startup;
using System;
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

      private IStartupManager _startup;

      public IConfiguration Configuration { get { return _configuration; } }

      public Startup(IWebHostEnvironment environment, IConfiguration configuration)
      {
         _configuration = configuration;
         _environment = environment;
         //
         var startup = new StartupManager();
         //
         startup.Configuration = StartupHelper.LoadStartupConfig(_configuration);
         // --> Set the comments path for the Swagger JSON and UI.
         startup.AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
         startup.Path = AppContext.BaseDirectory;
         _startup = startup;
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
         services.AddControllersAndPlatformServices
         (
            _provider, 
            _configuration, 
            _startup, 
            apiTitle: "Synuit Policy Server", 
            apiVersion: "v1", 
            withViews: true
         );

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

         // --> See Synuit.Toolkit
         app.ConfigureApplication(env, _configuration, _startup, _logger);

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
         });
      }
   }
}