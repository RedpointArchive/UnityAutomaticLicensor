using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnityAutomaticLicensor
{
    public class UnitySurvey
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("questions")]
        public UnitySurveyQuestion[] Questions { get; set; }

        [JsonProperty("external_code")]
        public string ExternalCode { get; set; }
    }

    public class UnitySurveyQuestion
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("widget")]
        public string Widget { get; set; }

        [JsonProperty("options")]
        public UnitySurveyQuestionOption[] Options { get; set; }

        [JsonProperty("required_options")]
        public int[] RequiredOptions { get; set; }

        [JsonProperty("external_code")]
        public string ExternalCode { get; set; }
    }

    public class UnitySurveyQuestionOption
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("visibility")]
        public string Visibility { get; set; }
    }
}
