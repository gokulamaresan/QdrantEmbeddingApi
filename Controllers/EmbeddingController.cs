using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AiSearchApi.Interface;
using AiSearchApi.Model;

namespace AiSearchApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmbeddingController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IQdrantService _qdrantService;

    public EmbeddingController(IEmbeddingService embeddingService, IQdrantService qdrantService)
    {
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
    }

    [HttpPost("embed")]
    public async Task<IActionResult> Embed([FromBody] EmbedRequest request)
    {
        if (request?.Source == null || !request.Source.Any())
            return BadRequest("Source list is required.");

        if (request?.Targets == null || !request.Targets.Any())
            return BadRequest("Targets list is required.");

        var source = request.Source.First();

        var embeddings = await _embeddingService.GenerateEmbeddingAsync(
            new[] { source }.Concat(request.Targets));
        var sourceEmbedding = embeddings[0];
        var scores = embeddings.Skip(1)
            .Select(targetEmbedding => CosineSimilarity(sourceEmbedding, targetEmbedding))
            .ToArray();

        // Optionally: store embeddings or scores in Qdrant
        for (int i = 0; i < request.Targets.Count; i++)
        {
            await _qdrantService.StoreEmbeddingAsync(request.Targets[i], new float[] { scores[i] });
        }

        return Ok(new
        {
            message = "Similarity scores calculated and stored successfully.",
            source = source,
            targets = request.Targets,
            scores = scores
        });
    }



    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        var queryVector = (await _embeddingService.GenerateEmbeddingAsync(new[] { request.Query }))[0];
        var results = await _qdrantService.SearchAsync(queryVector, request.TopK);

        return Ok(new { query = request.Query, results });
    }

    private static float CosineSimilarity(float[] first, float[] second)
    {
        var dotProduct = 0f;
        var firstMagnitude = 0f;
        var secondMagnitude = 0f;

        for (var i = 0; i < first.Length; i++)
        {
            dotProduct += first[i] * second[i];
            firstMagnitude += first[i] * first[i];
            secondMagnitude += second[i] * second[i];
        }

        return firstMagnitude == 0 || secondMagnitude == 0
            ? 0
            : dotProduct / MathF.Sqrt(firstMagnitude * secondMagnitude);
    }
}
