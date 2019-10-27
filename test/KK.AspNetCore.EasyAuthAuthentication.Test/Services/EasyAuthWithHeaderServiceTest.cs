namespace KK.AspNetCore.EasyAuthAuthentication.Test.Services
{
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

    public class EasyAuthWithHeaderServiceTest
    {
        private readonly ILoggerFactory loggerFactory = new NullLoggerFactory();

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
                Claims = new List<InputClaims>()
                {
                    new InputClaims() {Typ=  "x", Value= "y"},
                    new InputClaims() {Typ=  ClaimTypes.Email, Value= "PrincipalName"}
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
            [JsonProperty("claims")]
            public IEnumerable<InputClaims> Claims { get; set; }
        }

        internal class InputClaims
        {
            [JsonProperty("typ")]
            public string Typ { get; set; }
            [JsonProperty("val")]
            public string Value { get; set; }
        }
    }
}
