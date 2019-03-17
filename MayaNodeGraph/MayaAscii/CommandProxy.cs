using System;
using System.Collections.Generic;
using System.Linq;

namespace MayaNodeGraph.MayaAscii
{
    public class CommandProxy
    {
        public CommandProxy(NodeGraph graph)
        {
            _graph = graph;
        }

        public IEnumerable<Node> Ls(string nodeName = default, string typeName = default)
        {
            if (nodeName != default)
            {
                var node = default(Node);
                if (!_graph.TryGetNode(nodeName, out node))
                {
                    return Enumerable.Empty<Node>();
                }
                if (typeName != default)
                {
                    return node.Type.Name == typeName ? new[] { node } : Enumerable.Empty<Node>();
                }
            }

            if (typeName != default)
            {
                var nodes = default(IEnumerable<Node>);
                if (!_graph.TryGetNodes(new NodeType(typeName), out nodes))
                {
                    return Enumerable.Empty<Node>();
                }
                return nodeName != default ? nodes.Where(x => x.Name == nodeName) : nodes;
            }

            return _graph.Nodes;
        }

        public IEnumerable<Node> ListConnections(string nodeName, bool sources = true, bool destinations = true)
        {
            if (!_graph.TryGetNode(nodeName, out var node))
            {
                return Array.Empty<Node>();
            }

            var connections = Enumerable.Empty<Node>();
            if (sources)
            {
                connections = connections.Concat(node.GetSourceNodes());
            }
            if (destinations)
            {
                connections = connections.Concat(node.GetDestinationNodes());
            }
            return connections;
        }

        public IEnumerable<Node> ListRelatives(string nodeName, bool children = true, bool allDescendents = false, bool parent = false, bool allParents = false)
        {
            if (!_graph.TryGetNode(nodeName, out var node))
            {
                return Enumerable.Empty<Node>();
            }

            var nodes = Enumerable.Empty<Node>();
            if (allDescendents)
            {
                if (!_graph.TryGetChildren(node, out var tmpChildren))
                {
                    return Enumerable.Empty<Node>();
                }
                nodes = nodes.Concat(nodes);

                var queue = new Queue<Node>(tmpChildren);
                while (queue.Any())
                {
                    var child = queue.Dequeue();
                    if (_graph.TryGetChildren(child, out var tmpChildren1))
                    {
                        foreach (var tmpChild in tmpChildren1)
                        {
                            queue.Enqueue(tmpChild);
                        }
                        nodes = nodes.Concat(tmpChildren1);
                    }
                }
            }
            else if (children)
            {
                if (_graph.TryGetChildren(node, out var childNodes))
                {
                    nodes = nodes.Concat(childNodes);
                }
            }
            if (allParents)
            {
                var parents = new List<Node>();

                var parentNode = node.Parent;
                while (parentNode != Node.World)
                {
                    parents.Add(parentNode);
                    parentNode = parentNode.Parent;
                }

                nodes = nodes.Concat(parents);
            }
            else if (parent)
            {
                if (node.Parent != Node.World)
                {
                    nodes = nodes.Concat(new[] { node.Parent });
                }
            }
            return nodes;
        }

        private NodeGraph _graph;
    }
}
