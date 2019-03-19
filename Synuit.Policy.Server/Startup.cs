using EasyCaching.InMemory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Synuit.Policy.Services;
using Synuit.Policy.Services.Policy.Storage;
using Synuit.Policy.Services.Storage;
using System;
using System.IO;
using System.Reflection;

namespace Synuit.Policy
{
   /// <summary>
   ///
   /// </summary>
   public class Startup
   {
      /// <summary>
      ///
      /// </summary>
      public static IConfiguration Configuration { get; private set; }

      /// <summary>
      ///
      /// </summary>
      public static IHostingEnvironment Environment { get; private set; }

      /// <summary>
      ///
      /// </summary>
      /// <param name="environment"></param>
      public Startup(IHostingEnvironment environment)
      {
         Environment = environment;
         Configuration = BuildConfiguration();
      }

      /// <summary>
      /// This method gets called by the runtime. Use this method to add services to the DI container.
      /// </summary>
      /// <param name="services"></param>
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddMvcCore()
          .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
          .AddAuthorization() // added 11/21 tac
          .AddApiExplorer()
          .AddJsonFormatters();
         //
         services.AddApiVersioning();

         // --> Add authentication
         if (Configuration["ApiAuthConfig:AuthType"] == "Oidc")
         {
            services.AddAuthenticationForApi(Configuration, Environment);
         }

         //1. In-Memory Cache
         services.AddDefaultInMemoryCache();

         // Set the comments path for the Swagger JSON and UI.
         var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
         var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
         //

         //
         services.AddSwaggerGen(c =>
         {
            c.SwaggerDoc("v1", new Info { Title = "Synuit Policy Server", Version = "v1" });

            c.IncludeXmlComments(xmlPath);
         });
         //
         services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

         services.AddSingleton<IPolicyRepository, PolicyFileStorageRepository>();
         services.AddSingleton<IPolicyService, PolicyService>();
         services.AddSingleton<Serilog.ILogger>(Log.Logger);

         services.Configure<CookiePolicyOptions>(options =>
         {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
         });
      }

      /// <summary>
      /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      /// </summary>
      /// <param name="app"></param>
      /// <param name="env"></param>

      public void Configure(IApplicationBuilder app, IHostingEnvironment env)
      {
         // global exception handler
         app.UseExceptionHandler(appBuilder =>
         {
            appBuilder.Run(async context =>
            {
               var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

               if (exceptionHandlerFeature != null)
               {
                  var logger = Log.Logger;
                  logger.Error(exceptionHandlerFeature.Error.ToString());
               }

               context.Response.StatusCode = 500;
               await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
            });
         });

         if (Configuration["ApiAuthConfig:AuthType"] == "Oidc")
         {
            app.UseAuthentication();
         }

         if (!env.IsProduction())
         {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
               c.SwaggerEndpoint("/swagger/v1/swagger.json", "Synuit Policy Server Api - V1");
               c.RoutePrefix = string.Empty;
            });
         }

         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
            app.UseBrowserLink();
            app.UseDatabaseErrorPage();
         }
         else
         {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
         }

         app.UseHttpsRedirection();
         app.UseStaticFiles();

         app.UseMvc(routes =>
         {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
         });
      }

      /// <summary>
      /// This method gets called by the runtime. Use this method to configure the global Serilog logger.
      /// </summary>
      private IConfiguration BuildConfiguration()
      {
         var builder = new ConfigurationBuilder()
         .SetBasePath(Environment.ContentRootPath)
         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
         .AddJsonFile($"appsettings.{Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
         .AddEnvironmentVariables();
         //
         if (Environment.IsDevelopment())
         {
            builder.AddUserSecrets<Startup>();
         }
         //
         var configuration = builder.Build();
         //
         Log.Logger = new LoggerConfiguration()
             .ReadFrom.Configuration(configuration)
             .CreateLogger();
         //
         AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();

         return configuration;
      }
   }
}