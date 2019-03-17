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

            using (var reader = new SceneFileReader(filePath))
            {
                var graph = await NodeGraph.LoadAsync(reader);
            }

            Console.ReadKey();
        }
    }
}
