using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AiSearchApi.Interface;
using AiSearchApi.Model;
using RestSharp;

namespace AiSearchApi.Service;

public class QdrantService : IQdrantService
{
    private readonly IConfiguration _config;
    private readonly RestClient _client;
    private readonly string _collection;

    public QdrantService(IConfiguration config)
    {
        _config = config;
        _collection = _config["Qdrant:Collection"];
        _client = new RestClient(_config["Qdrant:Url"]);
        var apiKey = _config["Qdrant:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
            _client.AddDefaultHeader("api-key", apiKey);
    }

    public async Task StoreEmbeddingAsync(string text, float[] embedding)
    {
        var pointId = Guid.NewGuid().ToString();
        var request = new RestRequest($"/collections/{_collection}/points", Method.Put)
            .AddJsonBody(new { points = new[] { new { id = pointId, vector = embedding, payload = new { text } } } });

        var response = await _client.ExecuteAsync(request);
        if (!response.IsSuccessful)
            throw new Exception($"Failed to store embedding: {response.StatusCode} - {response.Content}");
    }

    public async Task<List<SearchResult>> SearchAsync(float[] embedding, int topK)
    {
        var request = new RestRequest($"/collections/{_collection}/points/search", Method.Post)
            .AddJsonBody(new { vector = embedding, limit = topK });

        var response = await _client.ExecuteAsync<QdrantSearchResponse>(request);
        if (!response.IsSuccessful || response.Data == null)
            throw new Exception($"Qdrant search failed: {response.StatusCode} - {response.Content}");

        return response.Data.Result?.Select(r => new SearchResult { Text = r.Payload["text"]?.ToString() ?? "", Score = r.Score }).ToList()
               ?? new List<SearchResult>();
    }

    private class QdrantSearchResponse { public List<QdrantResult> Result { get; set; } }
    private class QdrantResult { public float Score { get; set; } public Dictionary<string, object> Payload { get; set; } }
}
