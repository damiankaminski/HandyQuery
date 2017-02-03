using System;
using System.Collections.Generic;
using System.IO;
using HandyQuery.Language.Lexing.Gramma;
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

            File.WriteAllText(
                Path.Combine(visualizerPath.FullName, "data.json"),
                new LexerExecutionGraphJsonifier(lexer.ExecutionGraph).GetJson());
        }
    }

    internal sealed class LexerExecutionGraphJsonifier
    {
        private readonly LexerExecutionGraph _graph;

        private readonly List<Node> _visitedNodes = new List<Node>();

        private readonly List<JsonNode> _nodes = new List<JsonNode>();
        private readonly List<JsonEdge> _edges = new List<JsonEdge>();

        private readonly Dictionary<int, int> _xAxis = new Dictionary<int, int>();
        private const int XAxisMultiplier = 8;
        private const int YAxisMultiplier = 1;

        public LexerExecutionGraphJsonifier(LexerExecutionGraph graph)
        {
            _graph = graph;
        }

        public string GetJson()
        {
            Process(_graph.Root, null, 0);

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

        public void Process(Node node, JsonNode parent, int y)
        {
            if (_visitedNodes.Contains(node))
            {
                return;
            }

            _visitedNodes.Add(node);

            int x;
            if (_xAxis.TryGetValue(y, out x) == false)
            {
                _xAxis[y] = x;
            }

            var jsonNode = new JsonNode()
            {
                Id = Guid.NewGuid().ToString(),
                Label = node.ToString(),
                X = _xAxis[y]++ * XAxisMultiplier,
                Y = y * YAxisMultiplier,
                Size = 1,
                Node = node
            };
            _nodes.Add(jsonNode);

            if (parent != null)
            {
                _edges.Add(new JsonEdge()
                {
                    Id = Guid.NewGuid().ToString(),
                    Source = parent.Id,
                    Target = jsonNode.Id
                });
            }

            for (var index = 0; index < node.Children.Count; index++)
            {
                var nodeChild = node.Children[index];
                if (index > 0) _xAxis[y + index] = ++x;
                Process(nodeChild, jsonNode, y + index);
            }
        }

        internal sealed class JsonNode
        {
            public string Id { get; set; }
            public string Label { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Size { get; set; }

            [JsonIgnore]
            public Node Node { get; set; }
        }

        internal sealed class JsonEdge
        {
            public string Id { get; set; }
            public string Source { get; set; }
            public string Target { get; set; }
        }
    }
}