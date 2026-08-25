using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiSearchApi.Model
{
    public class SearchRequest
    {
        public string Query { get; set; }
        public int TopK { get; set; } = 3;
        public string Language { get; set; } = "en";
    }
}