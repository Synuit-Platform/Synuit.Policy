using IdentityModel.Client;
using Synuit.Platform.Utils;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Synuit.Policy.Test
{
   public class Program
   {
      public static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

      private static async Task MainAsync(string[] args)
      {
         Console.WriteLine("TEST 1: CALL API WITH AS ANONYMOUS ... FAIL EXPECTED!");

         var baseUrl = "https://localhost:5011/";

         var urlBuilder = new System.Text.StringBuilder();
         urlBuilder.Append(baseUrl != null ? baseUrl.TrimEnd('/') : "").Append("/api/v1/Policy/hospital.demo");
         var url = urlBuilder.ToString();

         var client = new HttpClient();

         using (var request = new HttpRequestMessage())
         {
            request.Method = new HttpMethod("GET");

            request.RequestUri = new System.Uri(url, System.UriKind.RelativeOrAbsolute);

            var response = await client.GetAsync(request.RequestUri);
            if (!response.IsSuccessStatusCode)
            {
               Console.WriteLine(response.StatusCode);
               Console.Write("TEST 1 FAILED - AS EXPECTED!");
               Console.ReadLine();
            }
            else
            {
               Console.WriteLine(JsonUtils.FormatJson(response.Content.ToString()));
               Console.Write("TEST 1 PASSED - NOT AS EXPECTED!");
               Console.ReadLine();
            }

            Console.WriteLine("TEST 2: CALL API WITH AS KNOWN CLIENT  ... PASS EXPECTED!");

            var discoCache = new DiscoveryCache("https://idp.test.syid.io");
            var disco = await discoCache.GetAsync();
            if (disco.IsError)
            {
               Console.Write(disco.Error + " ... press any key to continue ...");
               Console.ReadKey();
               return;
            }

            // request token
            var tokenClient = new HttpClient();
            var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync
               (
                  new ClientCredentialsTokenRequest
                  {
                     Address = disco.TokenEndpoint,
                     //
                     ClientId = "policy.client",
                     ClientSecret = "policy.client.secret.1",
                     Scope = "policy.server"
                  }
               );

            if (tokenResponse.IsError)
            {
               Console.Write(tokenResponse.Error);
               Console.ReadLine();
               return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            using (var request2 = new HttpRequestMessage())
            {
               request2.Method = new HttpMethod("GET");

               request2.RequestUri = new System.Uri(url, System.UriKind.RelativeOrAbsolute);

               var response2 = await client.GetAsync(request2.RequestUri);
               if (!response2.IsSuccessStatusCode)
               {
                  Console.WriteLine(response2.StatusCode);
                  Console.Write("TEST 2 FAILED - NOT AS EXPECTED!");
                  Console.ReadLine();
               }
               else
               {
                  Console.WriteLine(JsonUtils.FormatJson(await response2.Content.ReadAsStringAsync()));
                  Console.Write("TEST 2 PASSED - AS EXPECTED!");
                  Console.ReadLine();
               }
            }
         }
      }
   }
}