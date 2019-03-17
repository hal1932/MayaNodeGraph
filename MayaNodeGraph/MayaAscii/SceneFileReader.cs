using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MayaNodeGraph.MayaAscii
{
    public struct MayaAsciiCommand
    {
        public string Command;
        public int StartLineIndex;
        public int EndLineIndex;

        public string ToSingleLineCommand()
        {
            var lines = Command.Split(_NewLines, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();
            return string.Join(" ", lines).TrimEnd(' ', ';');
        }

        private static readonly string[] _NewLines = new[] { "\r\n", "\n" };
    }

    public class SceneFileReader : StreamReader
    {
        public SceneFileReader(string filePath, Encoding encoding = default)
            : base(filePath, encoding ?? GetDefaultEncoding())
        { }

        public MayaAsciiCommand ReadCommand()
        {
            var command = new StringBuilder();
            var startIndex = 0;

            while (!EndOfStream)
            {
                var line = ReadLine();
                ++ _currentLineIndex;

                if (line.StartsWith("//"))
                {
                    continue;
                }

                command.AppendLine(line);
                if (startIndex == 0)
                {
                    startIndex = _currentLineIndex - 1;
                }

                if (line.EndsWith(";"))
                {
                    return new MayaAsciiCommand()
                    {
                        Command = command.ToString(),
                        StartLineIndex = startIndex,
                        EndLineIndex = _currentLineIndex,
                    };
                }
            }

            return default;
        }

        private static Encoding GetDefaultEncoding()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Encoding.GetEncoding(932);
            }
            return Encoding.UTF8;
        }

        private int _currentLineIndex;
    }
}
