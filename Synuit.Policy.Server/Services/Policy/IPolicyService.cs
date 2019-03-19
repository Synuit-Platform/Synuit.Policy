using System.Threading.Tasks;

namespace Synuit.Policy.Services
{
   /// <summary>
   ///
   /// </summary>
   public interface IPolicyService
   {
      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <returns>Models.Policy</returns>
      Task<Platform.Policy.Models.Policy> GetPolicy(string id);

      ///// <summary>
      /////
      ///// </summary>
      ///// <param name="id"></param>
      ///// <returns>string</returns>
      //Task<string> GetPolicyJson(string id);

      /// <summary>
      ///
      /// </summary>
      /// <param name="id"></param>
      /// <param name="policy"></param>
      /// <returns>bool</returns>
      Task<bool> PutPolicy(string id, Platform.Policy.Models.Policy policy);

      ///// <summary>
      /////
      ///// </summary>
      ///// <param name="id"></param>
      ///// <param name="json"></param>
      ///// <returns>bool</returns>
      //Task<bool> PutPolicyJson(string id, string json);
   }
}