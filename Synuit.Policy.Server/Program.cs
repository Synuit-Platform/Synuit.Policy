using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Synuit.Policy.Server
{
   /// <summary>
   ///
   /// </summary>
   public class Program
   {
      private const string SeedArgs = "/seed";

      /// <summary>
      ///
      /// </summary>
      /// <param name="args"></param>
      public static void Main(string[] args)
      {
         MainAsync(args).GetAwaiter().GetResult();
      }

      private static async Task MainAsync(string[] args)
      {
         Console.Title = "Synuit.Policy.Server - Enterprise Policy as a Service (EPaaS)";
         var seed = args.Any(x => x == SeedArgs);
         if (seed) args = args.Except(new[] { SeedArgs }).ToArray();

         var host = CreateHostBuilder(args);
         // Uncomment this to seed upon startup, alternatively pass in `dotnet run /seed` to seed using CLI
         // await DbMigrationHelpers.EnsureSeedData(host);
         if (seed)
         {
         }

         await host.RunAsync();
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="args"></param>
      /// <returns></returns>
      public static IWebHost CreateHostBuilder(string[] args) =>
          WebHost.CreateDefaultBuilder(args)
          .UseKestrel(c => c.AddServerHeader = true)
               .UseStartup<Startup>()
              //.UseSerilog()
              .Build();
   }
}