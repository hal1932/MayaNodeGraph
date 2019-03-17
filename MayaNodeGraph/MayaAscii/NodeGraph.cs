using MayaNodeGraph.MayaAscii.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MayaNodeGraph.MayaAscii
{
    public class NodeGraph
    {
        public IEnumerable<Node> Nodes => _nameNodeDic.Values;
        public IEnumerable<NodeType> NodeTypes => _typeNodesDic.Keys;

        public static async Task<NodeGraph> LoadAsync(SceneFileReader reader)
        {
            var commands = new List<MayaAsciiCommand>();
            while (!reader.EndOfStream)
            {
                var command = reader.ReadCommand();
                if (command.Command != null)
                {
                    commands.Add(command);
                }
            }

            var parseNodes = Task.Factory.StartNew(() => ParseCreateNodeCommand(commands));
            var parseConnections = Task.Factory.StartNew(() => ParseConnectAttrCommand(commands));

            await Task.WhenAll(parseNodes, parseConnections).ConfigureAwait(false);

            var (nameNodeDic, typeNodesDic) = ConstructNodes(parseNodes.Result);
            ConstructConnections(parseConnections.Result, nameNodeDic);

            return new NodeGraph(nameNodeDic, typeNodesDic);
        }

        private NodeGraph(Dictionary<string, Node> nameNodeDic, Dictionary<NodeType, List<Node>> typeNodesDic)
        {
            _nameNodeDic = nameNodeDic;
            _typeNodesDic = typeNodesDic;

            ConstructNodeTree(nameNodeDic.Values);
        }

        public bool TryGetNode(string name, out Node node)
            => _nameNodeDic.TryGetValue(name, out node);

        public bool TryGetNodes(NodeType type, out IEnumerable<Node> nodes)
        {
            if (_typeNodesDic.TryGetValue(type, out var tmp))
            {
                nodes = tmp;
                return true;
            }
            nodes = default;
            return false;
        }

        public bool TryGetChildren(Node parent, out IEnumerable<Node> children)
        {
            if (_parentNodeDic.TryGetValue(parent, out var tmp))
            {
                children = tmp;
                return true;
            }
            children = default;
            return false;
        }

        private void ConstructNodeTree(IEnumerable<Node> nodes)
        {
            _parentNodeDic.Clear();

            _parentNodeDic[Node.World] = new List<Node>();
            foreach (var node in nodes)
            {
                if (!_parentNodeDic.TryGetValue(node.Parent, out var children))
                {
                    children = new List<Node>();
                    _parentNodeDic[node.Parent] = children;
                }
                children.Add(node);
            }
        }

        private static List<CreateNode> ParseCreateNodeCommand(IEnumerable<MayaAsciiCommand> commands)
        {
            var result = new List<CreateNode>();

            foreach (var command in commands)
            {
                if (CreateNode.TryParse(command, out var createNode))
                {
                    result.Add(createNode);
                }
            }

            return result;
        }

        private static List<ConnectAttr> ParseConnectAttrCommand(IEnumerable<MayaAsciiCommand> commands)
        {
            var result = new List<ConnectAttr>();

            foreach (var command in commands)
            {
                if (ConnectAttr.TryCreate(command, out var connectAttr))
                {
                    result.Add(connectAttr);
                }
            }

            return result;
        }

        private static (Dictionary<string, Node>, Dictionary<NodeType, List<Node>>) ConstructNodes(IEnumerable<CreateNode> commands)
        {
            var nameNodes = new Dictionary<string, Node>();
            var typeNodes = new Dictionary<NodeType, List<Node>>();

            var parentNames = new Dictionary<string, string>();
            var types = new Dictionary<string, NodeType>();
            foreach (var command in commands)
            {
                if (!types.TryGetValue(command.TypeName, out var type))
                {
                    type = new NodeType(command.TypeName);
                    types[type.Name] = type;
                }
                var node = new Node(command.NodeName, type);
                nameNodes[node.Name] = node;

                if (!typeNodes.TryGetValue(type, out var nodes))
                {
                    nodes = new List<Node>();
                    typeNodes[type] = nodes;
                }
                typeNodes[type].Add(node);

                if (command.ParentNodeName != default)
                {
                    parentNames[node.Name] = command.ParentNodeName;
                }
            }

            foreach (var item in parentNames)
            {
                var node = nameNodes[item.Key];
                node.SetParent(nameNodes[item.Value]);
            }

            return (nameNodes, typeNodes);
        }

        private static Dictionary<string, NodePlug> ConstructConnections(IEnumerable<ConnectAttr> commands, Dictionary<string, Node> nodes)
        {
            var plugs = new Dictionary<string, NodePlug>();

            foreach (var command in commands)
            {
                if (!nodes.TryGetValue(command.SourceNodeName, out var sourceNode))
                {
                    sourceNode = new Node(command.SourceNodeName, NodeType.Unknown);
                    nodes[sourceNode.Name] = sourceNode;
                }
                if (!nodes.TryGetValue(command.DestNodeName, out var destNode))
                {
                    destNode = new Node(command.DestNodeName, NodeType.Unknown);
                    nodes[destNode.Name] = destNode;
                }

                var sourcePlugName = NodePlug.GetPath(sourceNode.Name, command.SourceAttrName);
                if (!plugs.TryGetValue(sourcePlugName, out var sourcePlug))
                {
                    sourcePlug = new NodePlug(sourceNode, command.SourceAttrName);
                    plugs[sourcePlugName] = sourcePlug;
                }
                var destPlugName = NodePlug.GetPath(destNode.Name, command.DestAttrName);
                if (!plugs.TryGetValue(destPlugName, out var destPlug))
                {
                    destPlug = new NodePlug(destNode, command.DestAttrName);
                    plugs[destPlugName] = destPlug;
                }

                var connection = new NodeConnection(sourcePlug, destPlug);

                sourceNode.AddDestConnection(connection);
                destNode.AddSourceConnection(connection);
            }

            return plugs;
        }

        private Dictionary<string, Node> _nameNodeDic;
        private Dictionary<NodeType, List<Node>> _typeNodesDic;
        private Dictionary<Node, List<Node>> _parentNodeDic = new Dictionary<Node, List<Node>>();
    }
}
