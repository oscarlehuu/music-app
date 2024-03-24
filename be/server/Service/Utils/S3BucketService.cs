using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace server.Service.Utils
{
    public class S3BucketService
    {
        private readonly AmazonS3Client _s3Client;
        public S3BucketService(AmazonS3Client s3Client) 
        {
            _s3Client = s3Client;
        }
        public async Task ListBuckets() 
        {
            try 
            {
                ListBucketsResponse response = await _s3Client.ListBucketsAsync();
                Console.WriteLine("Connect S3 ");
                Console.WriteLine("Your S3 Buckets:");
                foreach (S3Bucket bucket in response.Buckets)
                {
                    Console.WriteLine(bucket.BucketName);
                }
            } 
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error listing buckets: {ex.Message}");
            }
        }
    }
}