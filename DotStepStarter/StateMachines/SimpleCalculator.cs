using DotStep.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotStepStarter.StateMachines.Calculator
{
    public class Context : IContext
    {
        public int Number1 { get; set; }
        public int Number2 { get; set; }

        public int Product { get; internal set; }
    }

    public sealed class SimpleCalculator : StateMachine<AddNumbers>
    {
    }

    public sealed class AddNumbers : TaskState<Context, DetermineNextStep>
    {
        public override async Task<Context> Execute(Context context)
        {
            context.Product = context.Number1 + context.Number2;
            return context;
        }
    }

    public sealed class DetermineNextStep : ChoiceState<Done>
    {
        public override List<Choice> Choices
        {
            get
            {
                return new List<Choice> {
                new Choice<Wait, Context>(c => c.Product > 0)
                    };
            }
        }
    }

    public sealed class Wait : WaitState<SubtractNumbers>
    {
        public override int Seconds => 10;
    }

    public sealed class SubtractNumbers : TaskState<Context, Done> {
        public override async Task<Context> Execute(Context context)
        {
            context.Product = context.Number1 - context.Number2;
            return context;
        }
    }
    
    public sealed class Done : PassState
    {
        public override bool End => true;
    }
}