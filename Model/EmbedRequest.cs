using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiSearchApi.Model
{
    public class EmbedRequest
    {
        public List<string> Source { get; set; } = new List<string>();
        public List<string> Targets { get; set; } = new List<string>();
        public string Language { get; set; } = "en"; // optional multilingual support
    }

}