using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AiSearchApi.Model;

namespace AiSearchApi.Interface
{
    public interface IQdrantService
    {
        Task StoreEmbeddingAsync(string text, float[] embedding);
        Task<List<SearchResult>> SearchAsync(float[] embedding, int topK);
    }
}