using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            string accountAws = System.Environment.GetEnvironmentVariable("ACCOUNT_AWS") ?? throw new ArgumentNullException("ACCOUNT_AWS");
            string regionAws = System.Environment.GetEnvironmentVariable("REGION_AWS") ?? throw new ArgumentNullException("REGION_AWS");

            var app = new App();
            _ = new CdkStack(app, $"Cdk{appName}API", new StackProps {
                Env = new Amazon.CDK.Environment {
                    Account = accountAws,
                    Region = regionAws,
                }
            });
            app.Synth();
        }
    }
}
