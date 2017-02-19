using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HandyQuery.Language.Lexing.Grammar;
using HandyQuery.Language.Lexing.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace HandyQuery.Language.Tests
{
    public sealed class Debug
    {
        [Test]
        [Explicit("Just for debug.")]
        public void GetLexerGraph()
        {
            var generator = new LexerGenerator();
            var lexer = generator.GenerateLexer();
            var mainDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory).Parent.Parent.Parent.Parent;
            var visualizerPath = new DirectoryInfo(Path.Combine(
                mainDir.FullName,
                "Tools\\LexerExecutionGraphsVisualizer"));
            
            if (visualizerPath.Exists == false)
            {
                throw new InvalidOperationException();
            }

            var json = new LexerExecutionGraphJsonifier(lexer.ExecutionGraph).GetJson();
            File.WriteAllText(
                Path.Combine(visualizerPath.FullName, "data.js"),
                $"window.data = {json}");
        }
    }

    internal sealed class LexerExecutionGraphJsonifier
    {
        private readonly LexerExecutionGraph _graph;

        private readonly List<Node> _visitedNodes = new List<Node>();

        private readonly List<JsonNode> _nodes = new List<JsonNode>();
        private readonly List<JsonEdge> _edges = new List<JsonEdge>();

        private int _nodeCounter = 0;

        public LexerExecutionGraphJsonifier(LexerExecutionGraph graph)
        {
            _graph = graph;
        }

        public string GetJson()
        {
            Process(_graph.Root, null);

            return JsonConvert.SerializeObject(new
            {
                nodes = _nodes,
                edges = _edges
            }, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public void Process(Node node, JsonNode parent)
        {
            var jsonNode = GetJsonNode(node);

            if (parent != null)
            {
                _edges.Add(new JsonEdge()
                {
                    From = parent.Id,
                    To = jsonNode.Id
                });
            }

            if (_visitedNodes.Contains(node))
            {
                return;
            }

            _visitedNodes.Add(node);

            foreach (var child in node.Children)
            {
                Process(child, jsonNode);
            }
        }

        private JsonNode GetJsonNode(Node node)
        {
            var existing = _nodes.FirstOrDefault(x => x.Node == node);
            if (existing != null)
            {
                return existing;
            }

            var jsonNode = new JsonNode()
            {
                Id = _nodeCounter++,
                Label = node.ToString(),
                Node = node
            };

            _nodes.Add(jsonNode);
            return jsonNode;
        }

        internal sealed class JsonNode
        {
            public int Id { get; set; }
            public string Label { get; set; }

            [JsonIgnore]
            public Node Node { get; set; }
        }

        internal sealed class JsonEdge
        {
            public int From { get; set; }
            public int To { get; set; }
            public JsonArrows Arrows { get; set; } = new JsonArrows() { To =  true };
            //public int Length { get; set; } = 500;
            public bool Physics { get; set; } = false;

            internal sealed class JsonArrows
            {
                public bool To { get; set; }
            }
        }
    }
}