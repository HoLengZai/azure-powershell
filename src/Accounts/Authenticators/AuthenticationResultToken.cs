﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Azure.Core;
using Azure.Identity;

using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Azure.Management.Internal.Network.Version2017_10_01.Models;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.PowerShell.Authenticators
{
    /// <summary>
    /// Access Token using a single authentication result
    /// </summary>
    public class AuthenticationResultToken : IAccessToken
    {
        AuthenticationResult _result;
        public string AccessToken { get; }

        public string UserId { get; }

        public string TenantId { get; }

        public string LoginType => "User";

        public AuthenticationResultToken(AuthenticationResult result)
        {
            _result = result;
            AccessToken = result.AccessToken;
            UserId = result.Account?.Username;
            TenantId = result.TenantId;
        }

        public AuthenticationResultToken(AuthenticationResult result, string userId = null, string tenantId = null)
        {
            _result = result;
            AccessToken = result.AccessToken;
            UserId = result.Account?.Username ?? userId;
            TenantId = result.TenantId ?? tenantId;
        }

        public void AuthorizeRequest(Action<string, string> authTokenSetter)
        {
            var header = _result.CreateAuthorizationHeader();
            authTokenSetter("Bearer", _result.AccessToken);
        }

        public static async Task<IAccessToken> GetAccessTokenAsync(Task<AuthenticationResult> result)
        {
            return new AuthenticationResultToken(await result);
        }

        public static async Task<IAccessToken> GetAccessTokenAsync(Task<AuthenticationResult> result, string userId = null, string tenantId = null)
        {
            return new AuthenticationResultToken(await result, userId, tenantId);
        }

        public static IAccessToken GetAccessToken(AuthenticationResult result)
        {
            return new AuthenticationResultToken(result);
        }

        public static IAccessToken GetAccessToken(AuthenticationResult result, string userId = null, string tenantId = null)
        {
            return new AuthenticationResultToken(result, userId, tenantId);
        }
    }

    public class MsalAccessToken : IAccessToken
    {
        public string AccessToken { get; }

        public string UserId { get; }

        public string TenantId { get; }

        public string LoginType => "User";

        public MsalAccessToken(string token, string tenantId, string userId = null)
        {
            AccessToken = token;
            UserId = userId;
            TenantId = tenantId;
        }

        public void AuthorizeRequest(Action<string, string> authTokenSetter)
        {
            //var header = _result.CreateAuthorizationHeader();
            authTokenSetter("Bearer", AccessToken);
        }

        public static async Task<IAccessToken> GetAccessTokenAsync(ValueTask<AccessToken> result, string tenantId = null, string userId = null)
        {
            var token = await result;
            //TODO: remove eriwan after Azure.Identity change
            return new MsalAccessToken(token.Token, tenantId, userId);//?? "eriwan@microsoft.com");
        }
    }
}
