using Microsoft.AspNetCore.Mvc;
using Synuit.Policy.ViewModels;
using System.Diagnostics;

namespace Synuit.Policy.Controllers
{
   /// <summary>
   ///
   /// </summary>

   public class HomeController : Controller
   {
      /// <summary>
      ///
      /// </summary>
      /// <returns></returns>
      public IActionResult Index()
      {
         return View();
      }

      /// <summary>
      ///
      /// </summary>
      /// <returns></returns>
      public IActionResult About()
      {
         ViewData["Message"] = "Your application description page.";

         return View();
      }

      /// <summary>
      ///
      /// </summary>
      /// <returns></returns>
      public IActionResult Contact()
      {
         ViewData["Message"] = "Your contact page.";

         return View();
      }

      /// <summary>
      ///
      /// </summary>
      /// <returns></returns>
      public IActionResult Privacy()
      {
         return View();
      }

      /// <summary>
      ///
      /// </summary>
      /// <returns></returns>
      [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
      public IActionResult Error()
      {
         return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
      }
   }
}