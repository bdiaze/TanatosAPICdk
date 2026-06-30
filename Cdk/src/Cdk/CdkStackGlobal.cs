using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Route53;
using Constructs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cdk {
	public class CdkStackGlobal : Stack {
		public HostedZone HostedZone { get; set; }
		public Certificate Certificate { get; set; }

		internal CdkStackGlobal(Construct scope, string id, IStackProps props = null) : base(scope, id, props) {
			string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno APP_NAME");
			string certDomainName = System.Environment.GetEnvironmentVariable("CERT_DOMAIN_NAME") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno CERT_DOMAIN_NAME");
			string certAlternativeNames = System.Environment.GetEnvironmentVariable("CERT_ALTERNATIVE_NAMES") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno CERT_ALTERNATIVE_NAMES");
			string googleSiteVerification = System.Environment.GetEnvironmentVariable("GOOGLE_SITE_VERIFICATION") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_SITE_VERIFICATION");
			string googleDkimValue = System.Environment.GetEnvironmentVariable("GOOGLE_DKIM_VALUE") ?? throw new InvalidOperationException("No se ha configurado la variable de entorno GOOGLE_DKIM_VALUE");

			// Se crea hosted zone...
			HostedZone = new(this, $"{appName}HostedZone", new HostedZoneProps {
				Comment = $"{appName} Hosted Zone",
				ZoneName = certDomainName
			});

			// Se crea registro TXT para verificación de dominio en Google y SPF1 de MAIL...
			_ = new TxtRecord(this, $"{appName}TXTRecord", new TxtRecordProps {
				Zone = HostedZone,
				RecordName = HostedZone.ZoneName,
				Values = [googleSiteVerification, "v=spf1 include:_spf.google.com include:amazonses.com ~all"],
			});

			// Se crea registro MX para integración con Google Workspace...
			_ = new MxRecord(this, $"{appName}MXRecord", new MxRecordProps {
				Zone = HostedZone,
				RecordName = HostedZone.ZoneName,
				Values = [new MxRecordValue {
					HostName = $"smtp.google.com.",
					Priority = 1
				}]
			});

			// Se crea registro DKIM para Google Workspace...
			_ = new TxtRecord(this, $"{appName}DKIMRecord", new TxtRecordProps {
				Zone = HostedZone,
				RecordName = $"google._domainkey.{HostedZone.ZoneName}",
				Values = [googleDkimValue],
			});

			// Se configura DMARC para el dominio...
			/*
			_ = new TxtRecord(this, $"{appName}DMARCTXTRecord", new TxtRecordProps {
				Zone = HostedZone,
				RecordName = $"_dmarc.{HostedZone.ZoneName}",
				Values = [dmarcValue]
			});
			*/

			// Se crea certificado para custom domain...
			Certificate = new(this, $"{appName}Certificate", new CertificateProps {
				CertificateName = $"{appName}Certificate",
				DomainName = certDomainName,
				SubjectAlternativeNames = certAlternativeNames.Split(","),
				Validation = CertificateValidation.FromDns(HostedZone),
			});
		}
	}
}
