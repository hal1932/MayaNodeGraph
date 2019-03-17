namespace MayaNodeGraph.MayaAscii.Commands
{
    struct CreateNode
    {
        public string TypeName;
        public string NodeName;
        public string ParentNodeName;

        public static bool TryParse(MayaAsciiCommand candidate, out CreateNode command)
        {
            command = default;

            if (!candidate.Command.StartsWith("createNode"))
            {
                return false;
            }

            var items = candidate.ToSingleLineCommand().Split(' ');
            if (items.Length < 2)
            {
                return false;
            }

            var nodeType = items[1];
            var nodeName = default(string);
            var parentNodeName = default(string);

            for (var i = 2; i < items.Length; ++i)
            {
                if (items[i] == "-n")
                {
                    ++i;
                    nodeName = items[i].Trim('"');
                    continue;
                }
                if (items[i] == "-p")
                {
                    ++i;
                    parentNodeName = items[i].Trim('"');
                    continue;
                }
            }

            command = new CreateNode()
            {
                TypeName = nodeType,
                NodeName = nodeName,
                ParentNodeName = parentNodeName,
            };
            return true;
        }
    }
}
