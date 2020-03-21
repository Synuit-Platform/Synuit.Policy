using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Synuit.Policy
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
      public static IServiceProvider Provider { get; set; }

      /// <summary>
      ///
      /// </summary>
      /// <param name="args"></param>
      /// <returns></returns>
      public static void Main(string[] args)
      {
         Console.Title = "Synuit.Policy.Server - Enterprise Policy as a Service (EPaaS)";
         var seed = args.Any(x => x == SeedArgs);
         if (seed) args = args.Except(new[] { SeedArgs }).ToArray();

         var host = BuildWebHost(args);
         // Uncomment this to seed upon startup, alternatively pass in `dotnet run /seed` to seed using CLI
         // await DbMigrationHelpers.EnsureSeedData(host);
         if (seed)
         {
            
         }

         host.Run();
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="args"></param>
      /// <returns></returns>
      public static IWebHost BuildWebHost(string[] args) =>
       WebHost.CreateDefaultBuilder(args)
              .UseKestrel(c => c.AddServerHeader = true)
              .UseStartup<Startup>()
              //.UseSerilog()
              .Build();
   }
}