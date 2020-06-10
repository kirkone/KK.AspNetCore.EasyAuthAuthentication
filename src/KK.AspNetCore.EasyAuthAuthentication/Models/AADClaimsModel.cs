namespace KK.AspNetCore.EasyAuthAuthentication.Models
{
    using System;
    using Newtonsoft.Json;

    internal class AADClaimsModel
    {
        [JsonProperty("typ")]
        public string Typ { get; set; } = string.Empty;

        [JsonProperty("val")]
        public string Values { get; set; } = string.Empty;
    }
}
