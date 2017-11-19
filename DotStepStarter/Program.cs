using DotStep.Core;
using DotStepStarter.StateMachines.HelloWorld;

namespace DotStepStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            IStateMachine stateMachine = new HelloWorldStateMachine();
            var context = new HelloWorldStateMachine.Context
            {
                Name = "Alice"
            };
            var engine = new StateMachineEngine<HelloWorldStateMachine, HelloWorldStateMachine.Context>(context);
            engine.Start().Wait();
        }
    }
}
