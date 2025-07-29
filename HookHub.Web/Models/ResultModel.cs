using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace HookHub.Web.Models
{
    public class ResultModel
    {
            [JsonProperty("Success")]
            public bool Success { get; set; } = false;

            [JsonProperty("Data")]
            public object Data { get; set; } = "";

            [JsonProperty("CurrentTime")]
            public DateTime CurrentTime { get; } = DateTime.Now;

            [JsonProperty("Errors")]
            public List<string> Errors { get; set; } = new List<string>();
    }
}
