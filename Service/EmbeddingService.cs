using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AiSearchApi.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace AiSearchApi.Service
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly IConfiguration _config;
        private readonly RestClient _client;

        public EmbeddingService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _client = new RestClient("https://api-inference.huggingface.co");
        }

        public async Task<float[][]> GenerateEmbeddingAsync(IEnumerable<string> texts)
        {
            if (texts == null || !texts.Any())
                return Array.Empty<float[]>();

            var apiKey = _config["HuggingFace:ApiKey"];
            var modelId = "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2";

            var request = new RestRequest($"/models/{modelId}", Method.Post)
                .AddHeader("Authorization", $"Bearer {apiKey}")
                .AddHeader("Content-Type", "application/json")
                .AddJsonBody(new
                {
                    inputs = texts
                });

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
                throw new Exception($"Embedding generation failed: {response.StatusCode} - {response.Content}");

            try
            {
                var embedding = JsonConvert.DeserializeObject<List<List<float>>>(response.Content);
                return embedding?.Select(e => e.ToArray()).ToArray() ?? Array.Empty<float[]>();
            }
            catch (JsonException jsonEx)
            {
                throw new InvalidOperationException($"Failed to parse HuggingFace response: {response.Content}", jsonEx);
            }
        }


    }
}
