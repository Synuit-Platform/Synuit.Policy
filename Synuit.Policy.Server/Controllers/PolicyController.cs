using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
//
using Synuit.Policy.Services;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Synuit.Policy.Controllers
{
   /// <summary>
   ///
   /// </summary>
   [Route("api/v{version:apiversion}/Policy")]
   [ApiVersion("1.0")]
   [Authorize(AuthenticationSchemes = "Bearer")]
   public class PolicyController : Controller
   {
      private ILogger _logger;
      private IPolicyService _policyService;

      /// <summary>
      ///
      /// </summary>
      /// <param name="logger"></param>
      /// <param name="policyService"></param>

      public PolicyController(ILogger logger, IPolicyService policyService)
      {
         _logger = logger;
         _policyService = policyService;
      }

      /// <summary>
      /// Gets the policy object and renders the content as a Json document.
      /// </summary>

      /// <param name="id">Policy id/name.</param>
      /// <returns>Returns policy Json document</returns>
      /// <response code="200">Returns the policy successfully</response>
      /// <response code="404">If the policy is not found</response>
      /// <response code="500">An Exception has occured</response>
      [HttpGet("{id}", Name = "GetPolicy")]
      [ProducesResponseType((int)HttpStatusCode.OK)]                           // --> 200
      [ProducesResponseType((int)HttpStatusCode.NotFound)]                     // --> 404
      [ProducesResponseType((int)HttpStatusCode.InternalServerError)]          // --> 500
      public async Task<IActionResult> GetPolicy(string id)
      {
         try
         {
            if (!string.IsNullOrEmpty(id))
            {
               Platform.Policy.Models.Policy json;

               _logger.Information($"{ControllerContext.ActionDescriptor.ControllerName}Controller.{ControllerContext.ActionDescriptor.ActionName}. Request Policy: {id}", 2001);

               json = await _policyService.GetPolicy(id);

               if (json != null)
               {
                  _logger.Information($"{ControllerContext.ActionDescriptor.ControllerName}Controller.{ControllerContext.ActionDescriptor.ActionName}. Policy for {id} returned successfully", 200);
                  return Ok(json);
               }
               else
               {
                  _logger.Warning($"{ControllerContext.ActionDescriptor.ControllerName}Controller.{ControllerContext.ActionDescriptor.ActionName}. Policy {id} does not exist in the Policy Repository", 404);
                  return NotFound("Policy does not exist in the Policy Repository.");
               }
            }
            else
            {
               _logger.Warning($"{ControllerContext.ActionDescriptor.ControllerName}Controller.{ControllerContext.ActionDescriptor.ActionName}. Policy Id // Name not recieved", 400);
               return BadRequest("Policy Id//Name is required to search for the policy.");
            }
         }
         catch (Exception ex)
         {
            _logger.Error($"{ControllerContext.ActionDescriptor.ControllerName}Controller.{ControllerContext.ActionDescriptor.ActionName}. " + ex.ToString(), 500);
            return StatusCode(500);
         }
      }

     
      /// <summary>
      /// Post new/updated policy as identified by id to  policy repsository.
      /// </summary>
      /// <remarks>
      /// Sample request:
      ///
      ///     POST / policy
      ///     {
      ///        "id": "adminUI",
      ///        "version": 1
      ///     }
      /// </remarks>
      /// <param name="id"> Policy id/name.
      /// i.e.  adminUI
      /// i.e. synuit.context.server.permissions</param>
      /// <param name="policy"></param>
      /// <returns>Newly created unique identifier for policy</returns>
      /// <response code="201">Returns the ID of the newly created content</response>
      /// <response code="500">An Exception has occured</response>
      [HttpPost("{id}", Name = "PutPolicy"), DisableRequestSizeLimit]
      [ProducesResponseType((int)HttpStatusCode.InternalServerError)]          // --> 500
      public async Task<IActionResult> PutPolicy(string id, [FromBody] Platform.Policy.Models.Policy policy)
      {
         var posted = await _policyService.PutPolicy(id, policy);
         if (posted)
         {
            return Ok();
         }
         else
         {
            return StatusCode(500);
         }
      }
   }
}