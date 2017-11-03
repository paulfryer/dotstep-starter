using Amazon.S3;
using Amazon.S3.Model;
using DotStep.Builder;
using DotStepStarter.StateMachines.Calculator;
using System;
using System.IO;

namespace DotStepStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            IAmazonS3 s3 = new AmazonS3Client();
            var releaseDirectory = args.Length > 0 ? args[0] : "bin/release";
            var template = DotStepBuilder.BuildCloudFormationTemplate<SimpleCalculator>();
            var path = $"{releaseDirectory}/template.json";
            File.WriteAllText(path, template);

            var version = Environment.GetEnvironmentVariable("CODEBUILD_SOURCE_VERSION");

            var bucket = version.Split(':')[5];
            var key = version.Split('/')[1] + "/template.json";

            s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                FilePath = path,
                ContentType = "application/json",
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            }).Wait();

        }
    }
}
