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
//
namespace Synuit.Policy.Tests
{
    public class RoleTests
    {
        Role _role;

        public RoleTests()
        {
            _role = new Role();
        }

        [Fact]
        public void Evaluate_should_require_user()
        {
            Action a = ()=> LocalRoleService.Evaluate(null, _role);
            a.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Evaluate_should_fail_for_invalid_subject()
        {
            var user = TestUser.Create("1");
            var result = LocalRoleService.Evaluate(user, _role);

            result.Should().BeFalse();
        }

        [Fact]
        public void Evaluate_should_succeed_for_valid_subject()
        {
            _role.Subjects.Add("1");

            var user = TestUser.Create("1");
            var result = LocalRoleService.Evaluate(user, _role);

            result.Should().BeTrue();
        }

        [Fact]
        public void Evaluate_should_fail_for_invalid_role()
        {
            var user = TestUser.Create("1");
            var result = LocalRoleService.Evaluate(user, _role);

            result.Should().BeFalse();
        }

        [Fact]
        public void Evaluate_should_succeed_for_valid_role()
        {
            _role.IdentityRoles.Add("foo");

            var user = TestUser.Create("1", roles:new[]{ "foo" });
            var result = LocalRoleService.Evaluate(user, _role);

            result.Should().BeTrue();
        }
    }
}