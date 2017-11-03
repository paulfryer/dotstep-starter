using Amazon.IdentityManagement;
using Amazon.S3;
using Amazon.S3.Model;
using DotStep.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotStepStarter.StateMachines.Calculator
{
    public class Context : IContext
    {
        public int Number1 { get; set; }
        public int Number2 { get; set; }
        public bool StoreResultsOnS3 { get; set; }

        public int Product { get; set; }
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

    public sealed class StoreResultsOnS3 : TaskState<Context, Done>
    {
        IAmazonIdentityManagementService iam = new AmazonIdentityManagementServiceClient();
        IAmazonS3 s3 = new AmazonS3Client();

        public override async Task<Context> Execute(Context context)
        {

            var getUserResult = await iam.GetUserAsync();

            var accountId = getUserResult.User.Arn.Split(':')[4];
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