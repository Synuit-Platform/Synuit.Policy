using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Serilog;
using Synuit.Platform.Auth.Types;
using Synuit.Policy.Data.Services;
using Synuit.Policy.Data.Services.Storage;
using Synuit.Toolkit.Infra.Extensions;
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
      /// <summary>
      ///
      /// </summary>
      public static IConfiguration Configuration { get; private set; }

      /// <summary>
      ///
      /// </summary>
      [Obsolete]
      public static IHostingEnvironment Environment { get; private set; }

      /// <summary>
      ///
      /// </summary>
      /// <param name="environment"></param>

      [Obsolete]
      public Startup(IHostingEnvironment environment)
      {
         Environment = environment;
         Configuration = BuildConfiguration();
      }

      /// <summary>
      /// This method gets called by the runtime. Use this method to add services to the container.
      /// </summary>
      /// <param name="services"></param>
      [Obsolete]
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddControllersWithViews(setupAction =>
         {
            setupAction.ReturnHttpNotAcceptable = true;
         })
            .AddNewtonsoftJson(setupAction =>
            {
               setupAction.SerializerSettings.ContractResolver =
                  new CamelCasePropertyNamesContractResolver();
            })
         .AddXmlDataContractSerializerFormatters()
         .AddMessagePackFormatters()
         .AddApiExplorer()
         .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
         //
         services.AddApiVersioning();
         //

         services.AddAuthorization(); // added 11/21/18 tac

         // --> Add authentication
         if (Configuration["ApiAuthConfig:AuthType"] == "Oidc")
         {
            services.AddAuthenticationForApi(Configuration, Environment);
         }
         // --> In-Memory Cache
         services.AddEasyCaching(setupAction =>
         {
            setupAction.UseInMemory();
         });

         var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
         // Set the comments path for the Swagger JSON and UI.
         var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
         var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

         //
         services.AddSwaggerGen(c =>
         {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Synuit Policy Server", Version = "v1" });

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
      [Obsolete]
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
         //
         if (Configuration["ApiAuthConfig:AuthType"] == "Oidc")
         {
            app.UseAuthentication();
         }
         //
         if (!env.IsProduction())
         {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
               c.SwaggerEndpoint("/swagger/v1/swagger.json", "Synuit Policy Server Api - V1");
               c.RoutePrefix = string.Empty;
            });
         }
         //
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
         app.UseStaticFiles();

         app.UseHttpsRedirection();

         app.UseRouting();

         app.UseAuthorization();

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
         });
      }

      /// <summary>
      /// This method gets called by the runtime. Use this method to configure the global Serilog logger.
      /// </summary>
      [Obsolete]
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