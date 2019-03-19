// Copyright (c) Brock Allen, Dominick Baier, Michele Leroux Bustamante. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Host.AspNetCorePolicy;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

using Microsoft.AspNetCore.Authentication;
using Synuit.Platform.Types;
using Synuit.Platform.Identity.Runtime;

namespace Host
{
   public class Startup
   {
      private readonly IConfiguration _configuration = null;
      private readonly IHostingEnvironment _environment = null;
      private static bool _remote = false;

      public static bool Remote { get { return _remote; } }
      
      public IConfiguration Configuration { get { return _configuration; } }

      public Startup(IHostingEnvironment environment, IConfiguration configuration)
      {
         _configuration = configuration;
         _environment = environment;
      }

      public void ConfigureServices(IServiceCollection services)
      {
         services.AddMvc(options =>
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
            services.AddIdentityServerClient(_configuration, _environment);
         }

         // this sets up the PolicyServer client library and policy provider - configuration is loaded from appsettings.json
         //services.AddPolicyServerClient(Configuration.GetSection("Policy"))
         services.AddPolicyServerClient(Configuration)
             .AddAuthorizationPermissionPolicies();

         // this adds the necessary handler for our custom medication requirement
         services.AddTransient<IAuthorizationHandler, MedicationRequirementHandler>();
      }

      public void Configure(IApplicationBuilder app, IHostingEnvironment env)
      {
         app.UseDeveloperExceptionPage();
         app.UseAuthentication();

         // add this middleware to make roles and permissions available as claims
         // this is mainly useful for using the classic [Authorize(Roles="foo")] and IsInRole functionality
         // this is not needed if you use the client library directly or the new policy-based authorization framework in ASP.NET Core
         app.UsePolicyServerClaims();

         app.UseStaticFiles();
         app.UseMvcWithDefaultRoute();
      }
   }
}