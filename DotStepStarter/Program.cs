using DotStep.Core;
using DotStepStarter.StateMachines.Calculator;

namespace DotStepStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            IStateMachine stateMachine = new SimpleCalculator();

            var context = new Context
            {
                Number1 = 1159,
                Number2 = 23,
                StoreResultsOnS3 = true
            };

            var engine = new StateMachineEngine<SimpleCalculator, Context>(context);
            engine.Start().Wait();
        }
    }
}
