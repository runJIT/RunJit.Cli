using System.Text.Json.Serialization;
using System.Text.Json;
using Extensions.Pack;
using Newtonsoft.Json;

namespace $ProjectName$.JsonSerializing
{
    public static class AddJsonSerializeOptionsExtension
    {
        internal static void AddJsonSerializeOptions(this IServiceCollection services)
        {
            var jsonSerializeOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                Converters = { new JsonStringEnumConverter() }
            };

            services.ConfigureHttpJsonOptions(options =>
                                              {
                                                  // options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
                                                  options.SerializerOptions.PropertyNameCaseInsensitive = jsonSerializeOptions.PropertyNameCaseInsensitive;
                                                  options.SerializerOptions.PropertyNamingPolicy = jsonSerializeOptions.PropertyNamingPolicy;
                                                  options.SerializerOptions.DictionaryKeyPolicy = jsonSerializeOptions.DictionaryKeyPolicy;
                                                  options.SerializerOptions.NumberHandling = jsonSerializeOptions.NumberHandling;
                                                  options.SerializerOptions.Converters.AddRange(jsonSerializeOptions.Converters);
                                              });
            
            
            services.AddSingletonIfNotExists(jsonSerializeOptions);
        }
    }
}
