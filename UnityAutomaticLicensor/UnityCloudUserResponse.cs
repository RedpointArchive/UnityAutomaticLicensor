using Newtonsoft.Json;

namespace UnityAutomaticLicensor
{
    public class UnityCloudUserResponse
    {
        [JsonProperty("foreign_key")]
        public string ForeignKey { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("primary_org")]
        public string PrimaryOrg { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }
}