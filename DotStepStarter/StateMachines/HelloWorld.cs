using DotStep.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotStepStarter.StateMachines.HelloWorld
{


    public sealed class HelloWorldStateMachine : StateMachine<HelloWorldStateMachine.InitializeVaraiables>
    {
        public class Context : IContext
        {
            public string Name { get; set; }

            public bool HelloDelivered { get; set; }
            public int Iterations { get; set; }
        }


        public sealed class InitializeVaraiables : TaskState<Context, CheckIfHelloRequired>
        {
            public override async Task<Context> Execute(Context context)
            {
                context.Iterations = 0;
                context.HelloDelivered = false;
                return context;
            }
        }

        public sealed class CheckIfHelloRequired : ChoiceState<TryToSayHello>
        {
            public override List<Choice> Choices
            {
                get
                {
                    return new List<Choice> {
                    new Choice<Done, Context>(c => c.Iterations >= 10),
                    new Choice<Done, Context>(c => c.HelloDelivered == true)
                };
                }
            }
        }

        public sealed class TryToSayHello : TaskState<Context, CheckIfHelloRequired>
        {
            public override async Task<Context> Execute(Context context)
            {
                var random = new Random();
                var value = random.Next(0, 4);
                if (value == 2)
                {
                    Console.WriteLine($"Hello {context.Name}!");
                    context.HelloDelivered = true;
                }
                context.Iterations++;
                return context;
            }
        }

        public sealed class Done : PassState
        {
            public override bool End => true;
        }
    }

}