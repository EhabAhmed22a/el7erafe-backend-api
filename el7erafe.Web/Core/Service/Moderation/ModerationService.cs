using ServiceAbstraction.Moderation;
using Shared.DataTransferObject.Moderation;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Service.Moderation
{
    public class ModerationService(HttpClient httpClient) : IModerationService
    {
        public async Task<ModerationResult> CheckMessageAsync(string content)
        {
            var response = await httpClient.PostAsJsonAsync(
                "https://regex-container-app.purpleglacier-eba9becf.polandcentral.azurecontainerapps.io/moderate",
                new { text = content }
            );

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Moderation service failed: {responseText}");

            var result = JsonSerializer.Deserialize<ModerationResult>(
                responseText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (result == null)
                throw new Exception("Invalid moderation response");

            return result;
        }
    }
}
