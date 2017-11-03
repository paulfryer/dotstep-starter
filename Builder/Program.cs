using DotStep.Builder;
using DotStepStarter.StateMachines.Calculator;

namespace DotStepStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            var codeBuildLocation = args.Length > 0 ? 
                args[0] : 
                "../DotStepStarter/bin/release/netcoreapp1.0/publish";
            var releaseDirectory = args.Length > 1 ?
                args[1] :
                "bin//release";

            DotStepBuilder.BuildStateMachine<SimpleCalculator>(codeBuildLocation, releaseDirectory).Wait();
        }
    }
}
