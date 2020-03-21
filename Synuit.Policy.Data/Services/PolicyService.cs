using EasyCaching.Core;
using Microsoft.Extensions.Logging;
using Synuit.Platform.Auth.Types;
using Synuit.Policy.Data.Services.Storage;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Synuit.Policy.Data.Services
{
   using Policy = Synuit.Platform.Auth.Policy.Models.Policy;

   /// <summary>
   ///
   /// </summary>
   public class PolicyService : IPolicyService
   {
      private ILogger<PolicyService> _logger;
      private IPolicyRepository _policyRepository;

      private IEasyCachingProvider _cacheProvider;

      /// <summary>
      ///
      /// </summary>

      /// <param name="policyRepository"></param>
      /// <param name="logger"></param>
      /// <param name="cacheProvider"></param>
      public PolicyService(IPolicyRepository policyRepository, ILogger<PolicyService> logger, IEasyCachingProvider cacheProvider)
      {
         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
         _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
         _policyRepository = policyRepository ?? throw new ArgumentNullException(nameof(policyRepository));
       
       
         _policyRepository = policyRepository;
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <param name="policy"></param>
      /// <returns></returns>
      public async Task<bool> PutPolicy(string id, Policy policy)
      {
         var set = false;
         string methodName = $"{MethodBase.GetCurrentMethod().DeclaringType.FullName.Split('+')[0]}.{MethodBase.GetCurrentMethod().DeclaringType.FullName.Split('+')[1].Split('<', '>')[1]}";

         if (policy != null)
         {
            set = await _policyRepository.PutPolicy(id, policy);

            if (set)
            {
               await _cacheProvider.SetAsync<Policy>(id, policy, TimeSpan.FromMinutes(15));
            }
            else
            {
               _logger.LogWarning($"{methodName}. The metadata/Json passed for policy {id} was invalid");
            }
         }

         return set;
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public async Task<Policy> GetPolicy(string id)
      {
         Policy json;

         string methodName = $"{MethodBase.GetCurrentMethod().DeclaringType.FullName.Split('+')[0]}.{MethodBase.GetCurrentMethod().DeclaringType.FullName.Split('+')[1].Split('<', '>')[1]}";

         var cachedValue = await _cacheProvider.GetAsync<Policy>(id);

         if (cachedValue.Value == null)
         {
            json = await _policyRepository.GetPolicy(id);

            if (json != null)
            {
               await _cacheProvider.SetAsync<Policy>(id, json, TimeSpan.FromMinutes(15)); //$!!$ make expiry configurable
            }
            else
            {
               _logger.LogWarning($"{methodName}. Policy {id} does not exist in the repository", 404);
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