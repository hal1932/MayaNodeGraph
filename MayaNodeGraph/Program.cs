using MayaNodeGraph.MayaAscii;
using System;
using System.Threading.Tasks;

namespace MayaNodeGraph
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var filePath = @"C:\Program Files\Autodesk\Maya2018\Examples\Animation\Rigs\rp_janna_rigged_HIK_arnold.ma";

            var graph = default(NodeGraph);
            using (var reader = new SceneFileReader(filePath))
            {
                graph = await NodeGraph.LoadAsync(reader);
            }

            var cmds = new CommandProxy(graph);
            foreach (var node in cmds.Ls(typeName: "transform"))
            {
                Console.WriteLine(node.Name);
            }
            Console.WriteLine("=====================");
            foreach (var node in cmds.ListConnections("root"))
            {
                Console.WriteLine(node.Name);
            }
            Console.WriteLine("=====================");
            foreach (var node in cmds.ListRelatives("root"))
            {
                Console.WriteLine(node.Name);
            }

            Console.ReadKey();
        }
    }
}
