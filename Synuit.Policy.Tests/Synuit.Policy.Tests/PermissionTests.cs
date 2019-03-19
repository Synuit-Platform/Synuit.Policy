//
//  Synuit.Policy.Client - Synuit Policy Platform (Authorization as a Service)
//  Copyright © 2018-2019 Synuit Software. All Rights Reserved.
//
// Portions:
// Copyright (c) Brock Allen, Dominick Baier, Michele Leroux Bustamante. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
//
using FluentAssertions;
using Synuit.Platform.Policy.Models;
using Synuit.Platform.Policy.Services;
using System;
using Xunit;

namespace Synuit.Policy.Tests
{
   public class PermissionTests
   {
      private Permission _perm;

      public PermissionTests()
      {
         _perm = new Permission();
      }

      [Fact]
      public void Evaluate_should_require_roles()
      {
         Action a = () => LocalPermissionService.Evaluate(_perm, null);
         a.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void Evaluate_should_fail_for_invalid_roles()
      {
         var result = LocalPermissionService.Evaluate(_perm, new[] { "foo" });
         result.Should().BeFalse();
      }

      [Fact]
      public void Evaluate_should_succeed_for_valid_roles()
      {
         _perm.Roles.Add("foo");
         var result = LocalPermissionService.Evaluate(_perm, new[] { "foo" });
         result.Should().BeTrue();
      }
   }
}