using DotStep.Builder;
using DotStepStarter.StateMachines.Calculator;
using System.IO;

namespace DotStepStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            var releaseDirectory = args.Length > 0 ? args[0] : "bin/release";
            var template = DotStepBuilder.BuildCloudFormationTemplate<SimpleCalculator>();
            File.WriteAllText($"{releaseDirectory}/template.json", template);
        }
    }
}
