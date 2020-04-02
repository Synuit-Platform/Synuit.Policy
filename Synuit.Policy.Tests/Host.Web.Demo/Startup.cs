using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Host.AspNetCorePolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Host
{
   public class Startup
   {
      private readonly IConfiguration _configuration ;
      private readonly IWebHostEnvironment _environment;
      private static bool _remote = false;

      public static bool Remote { get { return _remote; } }

      public IConfiguration Configuration { get { return _configuration; } }

      public Startup(IWebHostEnvironment environment, IConfiguration configuration)
      {
         _configuration = configuration;
         _environment = environment;
      }

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddControllersWithViews(options =>
         {
            // this sets up a default authorization policy for the application
            // in this case, authenticated users are required (besides controllers/actions that have [AllowAnonymous]
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
         });

         var policyConfig = _configuration.GetSection("PolicyClientConfig");
         var serverType = policyConfig.GetValue<string>("ServerType");

         if (serverType.ToLower() != "remote")
         {
            // this sets up authentication - for this demo we simply use a local cookie
            // typically authentication would be done using an external provider
            services.AddAuthentication("Cookies")
                .AddCookie("Cookies");
         }
         else
         {
            _remote = true;
            services.AddIdentityServerClient(_configuration);
         }

         // this sets up the PolicyServer client library and policy provider - configuration is loaded from appsettings.json
         //services.AddPolicyServerClient(Configuration.GetSection("Policy"))
         services.AddPolicyServerClient(Configuration)
             .AddAuthorizationPermissionPolicies();

         // this adds the necessary handler for our custom medication requirement
         services.AddTransient<IAuthorizationHandler, MedicationRequirementHandler>();

      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         app.UseDeveloperExceptionPage();
         
         app.UseAuthentication();
         
         //app.UseHttpsRedirection();
         // add this middleware to make roles and permissions available as claims
         // this is mainly useful for using the classic [Authorize(Roles="foo")] and IsInRole functionality
         // this is not needed if you use the client library directly or the new policy-based authorization framework in ASP.NET Core
         app.UsePolicyServerClaims();
         
         app.UseStaticFiles();

         app.UseRouting();

         app.UseAuthorization();

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
         });
      }
   }
}
