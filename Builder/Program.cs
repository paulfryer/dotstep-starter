using DotStep.Builder;
using DotStepStarter.StateMachines.Calculator;
using Newtonsoft.Json;
using System;
using System.IO;

namespace DotStepStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            var template = DotStepBuilder.BuildCloudFormationTemplate<SimpleCalculator>();
            File.WriteAllText("template.json", template);

            var s3Url = Environment.GetEnvironmentVariable("CODEBUILD_SOURCE_REPO_URL");
            Console.WriteLine($"S3 URL: {s3Url}");
            var s3Bucket = s3Url.Split('/')[2];
            var s3Key = s3Url.Replace(s3Bucket + "/", string.Empty);

            var config = JsonConvert.SerializeObject(new
            {
                Parameters = new
                {
                    S3CodeBucket = s3Bucket,
                    S3CodeKey = s3Key
                }
            });

            File.WriteAllText("config.json", config);            
        }
    }
}
