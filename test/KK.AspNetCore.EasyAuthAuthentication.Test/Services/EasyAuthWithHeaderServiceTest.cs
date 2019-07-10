using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using KK.AspNetCore.EasyAuthAuthentication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace KK.AspNetCore.EasyAuthAuthentication.Test.Services
{
    public class EasyAuthWithHeaderServiceTest
    {
        private ILoggerFactory loggerFactory = new NullLoggerFactory();
        private readonly string TestJwt = @"eyJhdWQiOiIwN2Q2ZDE1YS1jZTg5LTQ4MmMtOTcxYi01NDMxYjc1MTkxNjciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lOTgxZGNhZi05MTk3LTQ3N2YtYmQwNi0wZTU3MmIwYjMzNDcvIiwiaWF0IjoxNTYyNjk4ODc2LCJuYmYiOjE1NjI2OTg4NzYsImV4cCI6MTU2MjcwMjc3NiwiYWlvIjoiNDJaZ1lPQmZzRzd0ZEg1ZVBpSHZvNU9QdDdyT0J3QT0iLCJhcHBpZCI6ImQzMTViZmFmLTYzMDQtNGY5Zi04MjFjLTU0NmJkYzAwYjViMCIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0L2U5ODFkY2FmLTkxOTctNDc3Zi1iZDA2LTBlNTcyYjBiMzM0Ny8iLCJvaWQiOiJmNWY5ZmE4Ni00NDE0LTQ0YzctODBmOC1mNzgwYWUwYWJmMjEiLCJyb2xlcyI6WyJTeXN0ZW1BZG1pbiJdLCJzdWIiOiJmNWY5ZmE4Ni00NDE0LTQ0YzctODBmOC1mNzgwYWUwYWJmMjEiLCJ0aWQiOiJlOTgxZGNhZi05MTk3LTQ3N2YtYmQwNi0wZTU3MmIwYjMzNDciLCJ1dGkiOiJoQ0p1M29oN3dVZVphaTVRSk9ZQUFBIiwidmVyIjoiMS4wIn0=";
        private readonly string TestJwtAppId = "d315bfaf-6304-4f9f-821c-546bdc00b5b0";

        [Fact]
        public void IfTheAADIdTokenHeaderIsSetTheCanUseMethodMustReturnTrue()
        {
            // Arrange
            var handler = new EasyAuthWithHeaderService(this.loggerFactory.CreateLogger<EasyAuthWithHeaderService>());
            var httpcontext = new DefaultHttpContext();
            httpcontext.Request.Headers.Add("X-MS-TOKEN-AAD-ID-TOKEN", "blup");
            // Act
            var result = handler.CanHandleAuthentification(httpcontext);
            // Arrange
            Assert.True(result);
        }

        [Fact]
        public void IfTheAuthorizationHeaderIsNotSetTheCanUseMethodMustReturnFalse()
        {
            // Arrange
            var handler = new EasyAuthWithHeaderService(this.loggerFactory.CreateLogger<EasyAuthWithHeaderService>());
            var httpcontext = new DefaultHttpContext();
            // Act
            var result = handler.CanHandleAuthentification(httpcontext);
            // Arrange
            Assert.False(result);
        }

        [Fact]
        public void IfAValidJwtTokenIsInTheHeaderTheResultIsSuccsess()
        {
            // Arrange
            var handler = new EasyAuthWithHeaderService(this.loggerFactory.CreateLogger<EasyAuthWithHeaderService>());
            var httpcontext = new DefaultHttpContext();
            var inputObject = new InputJson()
            {
                claims = new List<InputClaims>()
                {
                    new InputClaims() {typ=  "x", val= "y"},
                    new InputClaims() {typ=  ClaimTypes.Email, val= "PrincipalName"}
                }
            };
            var json = JsonConvert.SerializeObject(inputObject);
            httpcontext.Request.Headers.Add("X-MS-TOKEN-AAD-ID-TOKEN", "Blup");
            httpcontext.Request.Headers.Add("X-MS-CLIENT-PRINCIPAL-IDP", "providername");
            httpcontext.Request.Headers.Add("X-MS-CLIENT-PRINCIPAL", Base64Encode(json));
            // Act
            var result = handler.AuthUser(httpcontext);
            // Arrange
            Assert.True(result.Succeeded);
            Assert.Equal("PrincipalName", result.Principal.Identity.Name);
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        internal class InputJson
        {
            public IEnumerable<InputClaims> claims { get; set; }
        }

        internal class InputClaims
        {
            public string typ { get; set; }
            public string val { get; set; }
        }
    }
}
