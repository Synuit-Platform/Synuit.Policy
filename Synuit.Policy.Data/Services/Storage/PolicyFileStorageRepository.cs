using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Synuit.Policy.Data.Services.Storage
{
   using Policy = Synuit.Platform.Auth.Policy.Models.Policy;

   /// <summary>
   ///
   /// </summary>
   public class PolicyFileStorageRepository : IPolicyRepository
   {
      private readonly IConfiguration _configuration;
      private readonly IHostingEnvironment _environment;
      private readonly ILogger<PolicyFileStorageRepository> _logger;

      private readonly string _basePath;
      private readonly string _webRoot ;
      private const string _JSON_EXT = ".json";


      public PolicyFileStorageRepository( IConfiguration configuration, IHostingEnvironment environment, ILogger<PolicyFileStorageRepository> logger)
      {
         _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
         _environment = environment ?? throw new ArgumentNullException(nameof(environment));
         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
         //
         _basePath = _configuration["StorageConfig:FileStorageConfig:RootContext"];
         _webRoot = _environment.ContentRootPath;

      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public async Task<bool> PolicyExists(string id)
      {
         var spath = Path.Combine(_webRoot, _basePath, id);
         return await FileExists(spath);
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      private async Task<bool> FileExists(string path)
      {
         return await Task.Run(() => File.Exists(path));
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>

      /// <returns>Policy.Models.Policy></returns>
      public async Task<Policy> GetPolicy(string id)
      {
         return JsonConvert.DeserializeObject<Policy>(await this.GetPolicyJson(id));
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <returns>string</returns>
      public async Task<string> GetPolicyJson(string id)
      {
         string s = string.Empty;
         string fileName = id + _JSON_EXT;
         var path = Path.Combine(_webRoot, _basePath, fileName);

         bool exists = await FileExists(path);
         //
         if (exists)
         {
            using (StreamReader sr = new StreamReader(path))
            {
               s = await sr.ReadToEndAsync();
            }
         }
         return s;
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <param name="policy"></param>
      /// <returns>bool</returns>
      public async Task<bool> PutPolicy(string id, Policy policy)
      {
         try
         {
            string json = JsonConvert.SerializeObject(policy);

            bool ok = false;
            if (json.Length > 0)
            {
               ok = await this.PutPolicyJson(id, json);
            }
            return ok;
         }
         catch (System.Exception)
         {
            return false;
         }
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <param name="json"></param>
      /// <returns>bool</returns>
      public async Task<bool> PutPolicyJson(string id, string json)
      {
         try
         {
            var path = Path.Combine(_webRoot, _basePath);

            if (!Directory.Exists(path))
            {
               Directory.CreateDirectory(path);
            }
            if (json.Length > 0)
            {
               string fileName = id + _JSON_EXT;
               string fullPath = Path.Combine(path, fileName);

               await File.WriteAllTextAsync(fullPath, json);
            }
            return true;
         }
         catch (System.Exception)
         {
            return false;
         }
      }
   }
}