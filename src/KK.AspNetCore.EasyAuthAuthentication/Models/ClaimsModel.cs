namespace KK.AspNetCore.EasyAuthAuthentication.Models
{
    using Newtonsoft.Json;

    internal class ClaimsModel
    {
        [JsonProperty("typ")]
        public string Typ { get; set; }

        [JsonProperty("val")]
        public string Values { get; set; }
    }
}
