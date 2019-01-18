namespace KK.AspNetCore.EasyAuthAuthentication
{
    /// <summary>
    /// This class contains all Authentication type names.
    /// Source of this is: https://docs.microsoft.com/en-us/dotnet/api/system.security.claims.authenticationtypes?view=netframework-4.7.2 .
    /// </summary>
    internal class AuthenticationTypesNames
    {
        public const string Basic = "AuthenticationTypes.Basic";
        public const string Federation = "AuthenticationTypes.Federation";
        public const string Kerberos = "AuthenticationTypes.Kerberos";
        public const string Negotiate = "AuthenticationTypes.Negotiate";
        public const string Password = "AuthenticationTypes.Password";
        public const string Signature = "AuthenticationTypes.Signature";
        public const string Windows = "AuthenticationTypes.Windows";
        public const string X509 = "AuthenticationTypes.X509";
    }
}
