using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiSearchApi.Interface
{
    public interface IEmbeddingService
    {
       Task<float[][]> GenerateEmbeddingAsync(IEnumerable<string> texts);
    }
}