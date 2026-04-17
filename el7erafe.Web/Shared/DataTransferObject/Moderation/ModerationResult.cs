
using System.Text.Json.Serialization;

namespace Shared.DataTransferObject.Moderation
{
    public class ModerationResult
    {
        [JsonPropertyName("is_safe")]
        public bool IsSafe { get; set; }
        [JsonPropertyName("layer")]
        public string Layer { get; set; } = default!;
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = default!;
        [JsonPropertyName("confidence")]
        public double? Confidence { get; set; }
    }
}
