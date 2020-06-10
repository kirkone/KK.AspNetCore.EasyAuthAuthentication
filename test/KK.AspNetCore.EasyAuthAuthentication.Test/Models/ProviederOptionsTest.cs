namespace KK.AspNetCore.EasyAuthAuthentication.Test.Models
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using Xunit;

    public class ProviederOptionsTest
    {
        [Fact]
        public void IfOptionsNullTheChangeModelMethodShouldDoNothing()
        {
            // Arrange
            var options = new ProviderOptions("testProviderName")
            {
                Enabled = true,
                NameClaimType = "klaus",
                RoleClaimType = "hamster"
            };

            // Act
            options.ChangeModel(null);
            // Assert
            Assert.True(options.Enabled);
            Assert.Equal("klaus", options.NameClaimType);
            Assert.Equal("hamster", options.RoleClaimType);
            Assert.Equal("testProviderName", options.ProviderName);
        }

        [Fact]
        public void IfOptionsInputHasADifferntProviderNameChangeModelShouldThrowAnError()
        {
            // Arrange
            var options = new ProviderOptions("testProviderName");

            // Act & Arrange
            _ = Assert.Throws<ArgumentException>(() => options.ChangeModel(new ProviderOptions("test")));
        }

        [Fact]
        public void IfTheOptionsStringAreNullOrWhitespaceTheyShouldNotChanged()
        {
            // Arrange
            var providerName = "testProviderName";
            var options = new ProviderOptions(providerName)
            {
                Enabled = true,
                NameClaimType = "klaus",
                RoleClaimType = "hamster"
            };

            // Act
            options.ChangeModel(new ProviderOptions(providerName) {NameClaimType = "           ", RoleClaimType = "             ", Enabled = true });
            // Assert
            Assert.True(options.Enabled);
            Assert.Equal("klaus", options.NameClaimType);
            Assert.Equal("hamster", options.RoleClaimType);
            Assert.Equal(providerName, options.ProviderName);
        }

        [Fact]
        public void IfTheOptionsAreSetTheyShouldChanged()
        {
            // Arrange
            var providerName = "testProviderName";
            var options = new ProviderOptions(providerName)
            {
                Enabled = true,
                NameClaimType = "klaus",
                RoleClaimType = "hamster"
            };

            // Act
            options.ChangeModel(new ProviderOptions(providerName) { NameClaimType = "Peter", RoleClaimType = "Pferd", Enabled = true });
            // Assert
            Assert.True(options.Enabled);
            Assert.Equal("Peter", options.NameClaimType);
            Assert.Equal("Pferd", options.RoleClaimType);
            Assert.Equal(providerName, options.ProviderName);
        }
    }
}
