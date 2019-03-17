using System.Collections.Generic;

namespace MayaNodeGraph.MayaAscii.Commands
{
    struct ConnectAttr
    {
        public string SourceNodeName;
        public string SourceAttrName;
        public string DestNodeName;
        public string DestAttrName;

        public static bool TryCreate(MayaAsciiCommand candidate, out ConnectAttr command)
        {
            command = default;

            if (!candidate.Command.StartsWith("connectAttr"))
            {
                return false;
            }

            var items = candidate.ToSingleLineCommand().Split(' ');
            if (items.Length < 3)
            {
                return false;
            }

            var attrs = new List<string>();
            for (var i = 1; i < items.Length; ++i)
            {
                if (items[i].StartsWith("-"))
                {
                    ++i;
                    continue;
                }
                attrs.Add(items[i].Trim('"'));
            }

            var source = attrs[0].Split('.');
            var dest = attrs[1].Split('.');

            command = new ConnectAttr()
            {
                SourceNodeName = source[0],
                SourceAttrName = source[1],
                DestNodeName = dest[0],
                DestAttrName = dest[1],
            };
            return true;
        }
    }
}
