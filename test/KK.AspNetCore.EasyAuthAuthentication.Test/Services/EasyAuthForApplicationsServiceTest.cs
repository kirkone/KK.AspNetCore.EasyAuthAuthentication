namespace KK.AspNetCore.EasyAuthAuthentication.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KK.AspNetCore.EasyAuthAuthentication.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public class EasyAuthForApplicationsServiceTest
    {
        private readonly ILoggerFactory loggerFactory = new NullLoggerFactory();
        private readonly string testJwt = @"Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkN0ZlFDOExlLThOc0M3b0MyelFrWnBjcmZPYyIsImtpZCI6IkN0ZlFDOExlLThOc0M3b0MyelFrWnBjcmZPYyJ9.eyJhdWQiOiIwN2Q2ZDE1YS1jZTg5LTQ4MmMtOTcxYi01NDMxYjc1MTkxNjciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lOTgxZGNhZi05MTk3LTQ3N2YtYmQwNi0wZTU3MmIwYjMzNDcvIiwiaWF0IjoxNTYyNjk4ODc2LCJuYmYiOjE1NjI2OTg4NzYsImV4cCI6MTU2MjcwMjc3NiwiYWlvIjoiNDJaZ1lPQmZzRzd0ZEg1ZVBpSHZvNU9QdDdyT0J3QT0iLCJhcHBpZCI6ImQzMTViZmFmLTYzMDQtNGY5Zi04MjFjLTU0NmJkYzAwYjViMCIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0L2U5ODFkY2FmLTkxOTctNDc3Zi1iZDA2LTBlNTcyYjBiMzM0Ny8iLCJvaWQiOiJmNWY5ZmE4Ni00NDE0LTQ0YzctODBmOC1mNzgwYWUwYWJmMjEiLCJyb2xlcyI6WyJTeXN0ZW1BZG1pbiJdLCJzdWIiOiJmNWY5ZmE4Ni00NDE0LTQ0YzctODBmOC1mNzgwYWUwYWJmMjEiLCJ0aWQiOiJlOTgxZGNhZi05MTk3LTQ3N2YtYmQwNi0wZTU3MmIwYjMzNDciLCJ1dGkiOiJoQ0p1M29oN3dVZVphaTVRSk9ZQUFBIiwidmVyIjoiMS4wIn0.fni_mCHCFWVXn1RtOvTWC5OgNU3xD_xyG38Bc1BfdcdoWP1p2N69HsW76rkk-IruDMdsiJaYKekK6RwUbBvbYii_S-fcT1FbdXCtYdFWm892Z4VGk8UFmS5HIApJd6WK4iHHwBv_R8n2juXyHKWfpZNaOldgaU0bRePSe3wu9_ZGOZ5et0_bbs1Y0UrwgaycZWwBSkah5s7fnLRBtMXmsEQlWPqnEzvwjieoqYs-YIndvau39ZE_0HT55kpbgE2HFJ2E62jyzJnMf60l8LlA_aXN6naNNm-SBepDoWVkUjZ-uQZbAdAr-MS-BIP4wS-1jrOvAfD6m7qOVcDaGYIv5A";
        private readonly string testJwtAppId = "d315bfaf-6304-4f9f-821c-546bdc00b5b0";

        [Fact]
        public void IfTheAuthorizationHeaderIsSetTheCanUseMethodMustReturnTrue()
        {
            // Arrange
            var handler = new EasyAuthForApplicationsService(this.loggerFactory.CreateLogger<EasyAuthForApplicationsService>());
            var httpcontext = new DefaultHttpContext();
            httpcontext.Request.Headers.Add("Authorization", "Bearer sgölkfsögölfsg");
            // Act
            var result = handler.CanHandleAuthentification(httpcontext); 
            // Arrange
            Assert.True(result);
        }

        [Fact]
        public void IfTheAuthorizationHeaderIsNotAJWTTokenTheCanUseMethodMustReturnFalse()
        {
            // Arrange
            var handler = new EasyAuthForApplicationsService(this.loggerFactory.CreateLogger<EasyAuthForApplicationsService>());
            var httpcontext = new DefaultHttpContext();
            httpcontext.Request.Headers.Add("Authorization", "sgölkfsögölfsg");
            // Act
            var result = handler.CanHandleAuthentification(httpcontext); 
            // Arrange
            Assert.False(result);
        }

        [Fact]
        public void IfTheAuthorizationHeaderIsNotSetTheCanUseMethodMustReturnFalse()
        {
            // Arrange
            var handler = new EasyAuthForApplicationsService(this.loggerFactory.CreateLogger<EasyAuthForApplicationsService>());
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
            var handler = new EasyAuthForApplicationsService(this.loggerFactory.CreateLogger<EasyAuthForApplicationsService>());
            var httpcontext = new DefaultHttpContext();
            httpcontext.Request.Headers.Add("Authorization", this.testJwt);
            // Act
            var result = handler.AuthUser(httpcontext);
            // Arrange
            Assert.True(result.Succeeded);
            Assert.Equal(this.testJwtAppId, result.Principal.Identity.Name);
        }
    }
}
