using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using DotStep.Core;
using Newtonsoft.Json;

namespace DotStepStarter.StateMachines
{
    public sealed class SimpleCalculator : StateMachine<SimpleCalculator.AddNumbers>
    {
        public class Context : IContext
        {
            public int Number1 { get; set; }
            public int Number2 { get; set; }
            public bool StoreResultsOnS3 { get; set; }

            public int Product { get; set; }
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
                    return new List<Choice>
                    {
                        new Choice<MakeProductSmaller, Context>(c => c.Product > 100),
                        new Choice<StoreResultsOnS3, Context>(c => c.StoreResultsOnS3 == true)
                    };
                }
            }
        }

        public sealed class MakeProductSmaller : TaskState<Context, Wait>
        {
            public override async Task<Context> Execute(Context context)
            {
                context.Product = Convert.ToInt32(context.Product * 0.5);
                return context;
            }
        }

        [DotStep.Core.Action(ActionName = "sts:GetCallerIdentity")]
        [DotStep.Core.Action(ActionName = "s3:PutBucket")]
        [DotStep.Core.Action(ActionName = "s3:PutObject")]
        public sealed class StoreResultsOnS3 : TaskState<Context, Done>
        {
            readonly IAmazonS3 s3 = new AmazonS3Client();
            readonly IAmazonSecurityTokenService sts = new AmazonSecurityTokenServiceClient();

            public override async Task<Context> Execute(Context context)
            {
                var getCallerResult = await sts.GetCallerIdentityAsync(new GetCallerIdentityRequest());

                var accountId = getCallerResult.Account;
                var region = Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION") ?? "us-west-2";

                var bucketName = $"{typeof(SimpleCalculator).Name}-{region}-{accountId}".ToLower();

                if (!s3.DoesS3BucketExistAsync(bucketName).Result)
                {
                    await s3.PutBucketAsync(
                        new PutBucketRequest
                        {
                            BucketName = bucketName,
                            UseClientRegion = true
                        });
                }

                var putResult = await s3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = "state.json",
                    ContentType = "application/json",
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                    ContentBody = JsonConvert.SerializeObject(context)
                });

                return context;
            }
        }

        public sealed class Wait : WaitState<DetermineNextStep>
        {
            public override int Seconds => 10;
        }


        public sealed class Done : PassState
        {
            public override bool End => true;
        }
    }
}