//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json.Serialization;
//using Extensions.Pack;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json;

//namespace RunJit.Cli.Services.Gpt
//{
//    internal static class AddGptSettingsExtension
//    {
//        internal static void AddGptSettings(this IServiceCollection services,
//                                            IConfiguration configuration)
//        {
//            services.AddSingletonOption<GptSettings>(configuration);
//        }
//    }

//    internal record GptSettings
//    {
//        public string ApiKey { get; init; } = string.Empty;
//    }


//    public static class AddGptClientExtension
//    {
//        public static void AddGptClient(this IServiceCollection services,
//                                        IConfiguration configuration)
//        {
//            services.AddGptSettings(configuration);
            
//            services.AddSingletonIfNotExists<GptClient>();
//        }
//    }
    

//    internal class GptClient(IHttpClientFactory httpClientFactory,
//                             GptSettings settings)
//    {
//        private const string Endpoint = "https://api.openai.com/v1/chat/completions";

//        public async Task<string> PostMessageAsync(string prompt)
//        {
//            // Create an HttpClient instance using the injected factory.
//            var client = httpClientFactory.CreateClient();
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

//            // Build the request payload.
//            var requestBody = new
//            {
//                model = "gpt-3.5-turbo",
//                messages = new[]
//                                             {
//                                                 new
//                                                 {
//                                                     role = "user",
//                                                     content = prompt
//                                                 }
//                                             },
//                max_tokens = 1000
//            };

//            var jsonContent = requestBody.ToJsonIntended();
//            var payload = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//            // Send the POST request.
//            var response = await client.PostAsync(Endpoint, payload);
//            var content = await response.Content.ReadAsStringAsync();

//            if (response.IsSuccessStatusCode.IsFalse())
//            {
//                return string.Empty;
//            }

//            // Deserialize the response to extract the generated content.
//            var result = content.FromJsonStringAs<GptResponse>();

//            if (result.IsNotNull() &&
//                result.Choices.IsNotNull() &&
//                result.Choices.Any())
//            {
//                return result.Choices.FirstOrDefault()?.Message?.Content?.Trim() ?? string.Empty;
//            }

//            return string.Empty;
//        }
//    }

//    // Helper classes for deserializing the GPT response.
//    public class GptResponse
//    {
//        [JsonPropertyName("id")]
//        public string? Id { get; set; }

//        [JsonPropertyName("object")]
//        public string? Object { get; set; }

//        [JsonPropertyName("created")]
//        public int Created { get; set; }

//        [JsonPropertyName("model")]
//        public string? Model { get; set; }

//        [JsonPropertyName("choices")]
//        public List<Choice> Choices { get; set; } = new List<Choice>();
//    }

//    public class Choice
//    {
//        [JsonPropertyName("message")]
//        public Message? Message { get; set; }

//        [JsonPropertyName("finish_reason")]
//        public string? FinishReason { get; set; }

//        [JsonPropertyName("index")]
//        public int Index { get; set; }
//    }

//    public class Message
//    {
//        [JsonPropertyName("role")]
//        public string? Role { get; set; }

//        [JsonPropertyName("content")]
//        public string? Content { get; set; }
//    }
//}
