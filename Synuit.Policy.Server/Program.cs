using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

using Synuit.Toolkit.Infra.Composition.Types;
using Synuit.Toolkit.Infra.Helpers;
using System;

namespace Synuit.Policy.Server
{
   /// <summary>
   /// 
   /// </summary>
   public class Program
   {
      private static void Main(string[] args)
      {
         Func<IHostBuilder> builderFunc() => () => CreateHostBuilder(args);

         Func<bool> bootstrapFunc(IHost host, IServiceScope scope, ILogger logger) => () =>
         {
            // migrate the database.  Best practice = in Main, using service scope

            ////using (var context = scope.ServiceProvider.GetRequiredService<IFactory<CaasContext>>().Create())
            ////{
            ////   // for demo purposes, delete the database & migrate on startup so
            ////   // we can start with a clean slate
            ////   // $!!$ tac - for dev / test
            ////   context.Database.EnsureDeleted();

            ////   context.Database.Migrate();
            ////}

            return true;
         };

         ProgramMainHelper.Bootstrap<Program>
           (args,
           appTitle: "Enterprise Policy as a Service (EPaaS)",
           customBuilder: builderFunc()
           //customBootstrap: bootstrapFunc
           );
      }
      /// <summary>
      /// 
      /// </summary>
      /// <param name="args"></param>
      /// <returns></returns>
      public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .ConfigureWebHostDefaults(options =>
              {
                
                 options.UseSerilog();
                 options.ConfigureServices(p => p.AddSingleton(Log.Logger));
                 options.UseStartup<Startup>();
              });
   }
}