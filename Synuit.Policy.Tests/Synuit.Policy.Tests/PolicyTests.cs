//
//  Synuit.Policy.Client - Synuit Policy Platform (Authorization as a Service)
//  Copyright © 2018-2019 Synuit Software. All Rights Reserved.
//
// Portions:
// Copyright (c) Brock Allen, Dominick Baier, Michele Leroux Bustamante. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
//
using FluentAssertions;
using Synuit.Platform.Auth.Policy.Models;
using Synuit.Platform.Auth.Policy.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Synuit.Policy.Tests
{
   using Policy = Synuit.Platform.Auth.Policy.Models.Policy;
   public class PolicyTests
   {
      private readonly Policy _policy;

      public PolicyTests()
      {
         _policy = new Policy();
      }

      [Fact]
      public void Evaluate_should_require_user()
      {
         Func<Task> a = () => LocalPolicyService.EvaluateAsync(null, _policy);
         a.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public async Task Evaluate_should_return_matched_roles()
      {
         _policy.Roles.AddRange(new[] {
                new Role{ Name = "c", Subjects = { "1" } },
                new Role{ Name = "a", Subjects = { "1" } },
                new Role{ Name = "b", Subjects = { "2" } },
            });

         var user = TestUser.Create("1");

         var result = await LocalPolicyService.EvaluateAsync(user, _policy);

         result.Roles.Should().BeEquivalentTo(new[] { "a", "c" });
      }

      [Fact]
      public async Task Evaluate_should_not_return_unmatched_roles()
      {
         _policy.Roles.AddRange(new[] {
                new Role{ Name = "c", Subjects = { "2" } },
                new Role{ Name = "a", Subjects = { "3" } },
                new Role{ Name = "b", Subjects = { "2" } },
            });

         var user = TestUser.Create("1");

         var result = await LocalPolicyService.EvaluateAsync(user, _policy);

         result.Roles.Should().BeEmpty();
      }

      [Fact]
      public async Task Evaluate_should_return_remove_duplicate_roles()
      {
         _policy.Roles.AddRange(new[] {
                new Role{ Name = "a", Subjects = { "1" } },
                new Role{ Name = "a", Subjects = { "1" } },
            });

         var user = TestUser.Create("1");

         var result = await LocalPolicyService.EvaluateAsync(user, _policy);

         result.Roles.Should().BeEquivalentTo(new[] { "a" });
      }

      [Fact]
      public async Task Evaluate_should_return_matched_permissions()
      {
         _policy.Roles.AddRange(new[] {
                new Role{ Name = "role", Subjects = { "1" } },
                new Role{ Name = "xoxo", Subjects = { "2" } },
            });
         _policy.Permissions.AddRange(new[] {
                new Permission{ Name = "a", Roles = { "role" } },
                new Permission{ Name = "c", Roles = { "role" } },
                new Permission{ Name = "b", Roles = { "xoxo" } },
            });

         var user = TestUser.Create("1");

         var result = await LocalPolicyService.EvaluateAsync(user, _policy);

         result.Permissions.Should().BeEquivalentTo(new[] { "a", "c" });
      }

      [Fact]
      public async Task Evaluate_should_not_return_unmatched_permissions()
      {
         _policy.Roles.AddRange(new[] {
                new Role{ Name = "role", Subjects = { "1" } },
            });
         _policy.Permissions.AddRange(new[] {
                new Permission{ Name = "a", Roles = { "xoxo" } },
                new Permission{ Name = "c", Roles = { "xoxo" } },
                new Permission{ Name = "b", Roles = { "xoxo" } },
            });

         var user = TestUser.Create("1");

         var result = await LocalPolicyService.EvaluateAsync(user, _policy);

         result.Permissions.Should().BeEmpty();
      }

      [Fact]
      public async Task Evaluate_should_remove_duplicate_permissions()
      {
         _policy.Roles.AddRange(new[] {
                new Role{ Name = "role", Subjects = { "1" } },
            });
         _policy.Permissions.AddRange(new[] {
                new Permission{ Name = "a", Roles = { "role" } },
                new Permission{ Name = "a", Roles = { "role" } },
            });

         var user = TestUser.Create("1");

         var result = await LocalPolicyService.EvaluateAsync(user, _policy);

         result.Permissions.Should().BeEquivalentTo(new[] { "a" });
      }

      [Fact]
      public async Task Evaluate_should_not_allow_identity_roles_to_match_permissions()
      {
         _policy.Permissions.AddRange(new[] {
                new Permission{ Name = "perm", Roles = { "role" } },
            });

         var user = TestUser.Create("1", roles: new[] { "role" });

         var result = await LocalPolicyService.EvaluateAsync(user, _policy);

         result.Permissions.Should().BeEmpty();
      }
   }
}