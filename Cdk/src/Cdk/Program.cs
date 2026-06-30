using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cdk {
    sealed class Program {
        private Program() { }

        public static void Main(string[] args)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno APP_NAME");
            string accountAws = System.Environment.GetEnvironmentVariable("ACCOUNT_AWS") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno ACCOUNT_AWS");
            string regionAws = System.Environment.GetEnvironmentVariable("REGION_AWS") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno REGION_AWS");
			string globalServicesRegionAws = System.Environment.GetEnvironmentVariable("GLOBAL_SERVICES_REGION_AWS") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GLOBAL_SERVICES_REGION_AWS"); ;


			var app = new App();
			CdkStackGlobal stackGlobal = new(app, $"Cdk{appName}GlobalServices", new StackProps {
				Env = new Amazon.CDK.Environment {
					Account = accountAws,
					Region = globalServicesRegionAws,
				}
			});

            _ = new CdkStack(app, $"Cdk{appName}API", new CdkStackProps {
                CrossRegionReferences = true,
                Env = new Amazon.CDK.Environment {
                    Account = accountAws,
                    Region = regionAws,
                },
				HostedZone = stackGlobal.HostedZone,
				Certificate = stackGlobal.Certificate,
			});
            app.Synth();
        }
    }
}
