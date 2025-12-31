using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;

namespace ChordalWebApp.Models
{
    public class JsonProgressionFile
    {
        [JsonProperty("progressionTitle")]
        public string ProgressionTitle { get; set; }

        [JsonProperty("keyRoot")]
        public int KeyRoot { get; set; }

        [JsonProperty("isKeyMajor")]
        public bool IsKeyMajor { get; set; }

        [JsonProperty("tempo")]
        public int? Tempo { get; set; } // Nullable if optional in JSON

        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; } // Nullable if optional

        [JsonProperty("chordalVersion")]
        public string ChordalVersion { get; set; }

        [JsonProperty("chordEvents")]
        public List<JsonChordEvent> ChordEvents { get; set; }
    }

    
    public class JsonChordEvent
    {
        [JsonProperty("startTime")]
        public double StartTime { get; set; }

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("chordInfo")]
        public JsonChordInfo ChordInfo { get; set; }
    }

    public class JsonChordInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("notes")]
        public List<int> Notes { get; set; } // List of MIDI note numbers

        [JsonProperty("rootNote")]
        public int RootNote { get; set; } // Chroma 0-11

        [JsonProperty("quality")]
        public string Quality { get; set; }

        [JsonProperty("extensions")]
        public string Extensions { get; set; }

        [JsonProperty("romanNumeral")]
        public string RomanNumeral { get; set; }

        [JsonProperty("function")]
        public string Function { get; set; }
    }
}