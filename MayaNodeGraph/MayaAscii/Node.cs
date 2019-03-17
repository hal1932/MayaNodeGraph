using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MayaNodeGraph.MayaAscii
{
    [DebuggerDisplay("{Name}")]
    public class NodeType : IEquatable<NodeType>
    {
        public static readonly NodeType Unknown = new NodeType("unknown");

        public string Name { get; }

        public NodeType(string name)
        {
            Name = name;
        }

        public bool Equals(NodeType other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return typeof(NodeType).GetHashCode() ^ Name.GetHashCode();
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class NodePlug : IEquatable<NodePlug>
    {
        public Node Node { get; }
        public string Name { get; }

        public NodePlug(Node node, string name)
        {
            Node = node;
            Name = name;
        }

        public override string ToString()
            => GetPath(Node, Name);

        public static string GetPath(Node node, string name)
            => GetPath(node.Name, name);

        public static string GetPath(string nodeName, string name)
            => $"{nodeName}.{name}";

        public bool Equals(NodePlug other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Node.Equals(other.Node) && Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return typeof(NodePlug).GetHashCode() ^ Node.GetHashCode() ^ Name.GetHashCode();
        }
    }

    [DebuggerDisplay("{Source} -> {Destination}")]
    public class NodeConnection : IEquatable<NodeConnection>
    {
        public NodePlug Source { get; }
        public NodePlug Destination { get; }

        public NodeConnection(NodePlug source, NodePlug destination)
        {
            Source = source;
            Destination = destination;
        }

        public bool Equals(NodeConnection other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Source.Equals(other.Source) && Destination.Equals(other.Destination);
        }

        public override int GetHashCode()
        {
            return typeof(NodeConnection).GetHashCode() ^ Source.GetHashCode() ^ Destination.GetHashCode();
        }
    }

    [DebuggerDisplay("{Name} <{Type.Name}>")]
    public class Node : IEquatable<Node>
    {
        public static readonly Node World = new Node("world", null);

        public string Name { get; }
        public NodeType Type { get; }
        public Node Parent { get; private set; } = World;

        public Node(string name, NodeType type)
        {
            Name = name;
            Type = type;
        }

        public IEnumerable<Node> GetSourceNodes()
            => _sourceConnections.Select(x => x.Source.Node).Distinct();

        public IEnumerable<Node> GetDestinationNodes()
            => _destConnections.Select(x => x.Destination.Node).Distinct();

        internal void SetParent(Node node)
        {
            Parent = node ?? World;
        }

        internal void AddSourceConnection(NodeConnection connection)
            => _sourceConnections.Add(connection);

        internal void AddDestConnection(NodeConnection connection)
            => _destConnections.Add(connection);

        public bool Equals(Node other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return typeof(Node).GetHashCode() ^ Name.GetHashCode();
        }

        private List<NodeConnection> _sourceConnections = new List<NodeConnection>();
        private List<NodeConnection> _destConnections = new List<NodeConnection>();
    }
}
