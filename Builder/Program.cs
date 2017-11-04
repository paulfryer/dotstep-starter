﻿using Amazon.S3;
using Amazon.S3.Model;
using DotStep.Builder;
using DotStepStarter.StateMachines.Calculator;
using DotStepStarter.StateMachines.HelloWorld;
using System;
using System.Collections.Generic;
using System.IO;

namespace DotStepStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            IAmazonS3 s3 = new AmazonS3Client();
            var releaseDirectory = args.Length > 0 ? args[0] : "bin/release";

            var types = new List<Type> {
                typeof(SimpleCalculator),
                typeof(HelloWorldStateMachine)
            };

            var template = DotStepBuilder.BuildCloudFormationTemplates(types);
            var path = $"{releaseDirectory}/template.json";
            File.WriteAllText(path, template);

            var version = Environment.GetEnvironmentVariable("CODEBUILD_SOURCE_VERSION");
            Console.WriteLine("Version: " + version);
            var bucket = version.Split(':')[5].Split('/')[0];
            var key = version.Split('/')[1] + "/template.json";
            var codeBuildKmsKeyId = Environment.GetEnvironmentVariable("CODEBUILD_KMS_KEY_ID");
            Console.WriteLine("Bucket: " + bucket);
            Console.WriteLine("Key: " + key);
            s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                FilePath = path,
                ContentType = "application/json",
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AWSKMS,
                ServerSideEncryptionKeyManagementServiceKeyId = codeBuildKmsKeyId
            }).Wait();

        }
    }
}
