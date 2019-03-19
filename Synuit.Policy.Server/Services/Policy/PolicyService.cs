using EasyCaching.Core;
using Serilog;
using Synuit.Policy.Services.Storage;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Synuit.Policy.Services
{
   /// <summary>
   ///
   /// </summary>
   public class PolicyService : IPolicyService
   {
      private ILogger _logger;
      private IPolicyRepository _policyRepository;

      private IEasyCachingProvider _cacheProvider;
     

      /// <summary>
      ///
      /// </summary>
     
      /// <param name="policyRepository"></param>
      /// <param name="logger"></param>
      /// <param name="cacheProvider"></param>
      public PolicyService(IPolicyRepository policyRepository, ILogger logger, IEasyCachingProvider cacheProvider)
      {
         _logger = logger;
         _cacheProvider = cacheProvider;
         _policyRepository = policyRepository;
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <param name="policy"></param>
      /// <returns></returns>
      public async Task<bool> PutPolicy(string id, Platform.Policy.Models.Policy policy)
      {
         var set = false;
         string methodName = $"{MethodBase.GetCurrentMethod().DeclaringType.FullName.Split('+')[0]}.{MethodBase.GetCurrentMethod().DeclaringType.FullName.Split('+')[1].Split('<', '>')[1]}";

         if (policy != null)
         {
            set = await _policyRepository.PutPolicy(id, policy);

            if (set)
            {
               await _cacheProvider.SetAsync<Platform.Policy.Models.Policy>(id, policy, TimeSpan.FromMinutes(15));
            }
            else
            {
               _logger.Warning($"{methodName}. The metadata/Json passed for policy {id} was invalid");
            }
         }

         return set;
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public async Task<Platform.Policy.Models.Policy> GetPolicy(string id)
      {
         Platform.Policy.Models.Policy json;

         string methodName = $"{MethodBase.GetCurrentMethod().DeclaringType.FullName.Split('+')[0]}.{MethodBase.GetCurrentMethod().DeclaringType.FullName.Split('+')[1].Split('<', '>')[1]}";

         var cachedValue = await _cacheProvider.GetAsync<Platform.Policy.Models.Policy>(id);

         if (cachedValue.Value == null)
         {
            json = await _policyRepository.GetPolicy(id);

            if (json != null)
            {
               await _cacheProvider.SetAsync<Platform.Policy.Models.Policy>(id, json, TimeSpan.FromMinutes(15)); //$!!$ make expiry configurable
            }
            else
            {
               _logger.Warning($"{methodName}. Policy {id} does not exist in the repository", 404);
            }
         }
         else
         {
            json = cachedValue.Value;
         }

         return json;
      }
   }
}