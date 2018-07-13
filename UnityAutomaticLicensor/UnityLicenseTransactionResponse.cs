using Newtonsoft.Json;

namespace UnityAutomaticLicensor
{
    public class UnityLicenseTransactionResponse
    {
        [JsonProperty("transaction")]
        public UnityLicenseTransaction Transaction { get; set; }
    }

    public class UnityLicenseTransaction
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("machine_id")]
        public string MachineId { get; set; }

        [JsonProperty("rx")]
        public string Rx { get; set; }

        [JsonProperty("serial_id")]
        public string SerialId { get; set; }

        [JsonProperty("survey")]
        public UnityLicenseSurvey Survey { get; set; }

        [JsonProperty("survey_answer")]
        public bool SurveyAnswer { get; set; }

        [JsonProperty("unity_version")]
        public string UnityVersion { get; set; }
    }

    public class UnityLicenseSurvey
    {
        [JsonProperty("answered")]
        public bool Answered { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

}