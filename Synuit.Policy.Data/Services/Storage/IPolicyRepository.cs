using System.Threading.Tasks;

namespace Synuit.Policy.Data.Services.Storage
{
   using Policy = Synuit.Platform.Auth.Policy.Models.Policy;

   /// <summary>
   ///
   /// </summary>
   public interface IPolicyRepository
   {
      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      Task<bool> PolicyExists(string id);

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <returns>Synuit.Policy.Models.Policy</returns>
      Task<Policy> GetPolicy(string id);

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <returns>string</returns>
      Task<string> GetPolicyJson(string id);

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <param name="policy"></param>
      /// <returns>bool</returns>
      Task<bool> PutPolicy(string id, Policy policy);

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <param name="json"></param>
      /// <returns>bool</returns>
      Task<bool> PutPolicyJson(string id, string json);
   }
}