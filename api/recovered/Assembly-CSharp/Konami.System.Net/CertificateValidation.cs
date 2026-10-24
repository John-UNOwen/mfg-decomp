using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

namespace Konami.System.Net;

public class CertificateValidation
{
	private static X509Certificate2Collection s_CertificateList = null;

	private static string indenet_one = "    ";

	public static void Init()
	{
		if (s_CertificateList == null)
		{
			s_CertificateList = new X509Certificate2Collection();
			s_CertificateList.Add(CreateCertificate("certs/DigiCert"));
			s_CertificateList.Add(CreateCertificate("certs/GeoTrustRSACA_2018"));
			s_CertificateList.Add(CreateCertificate("certs/RapidSSL_RSA_CA_2018"));
		}
	}

	private static X509Certificate2 CreateCertificate(string path)
	{
		TextAsset textAsset = Resources.Load(path) as TextAsset;
		if (textAsset == null)
		{
			throw new Exception("load error : " + path);
		}
		return new X509Certificate2(Encoding.ASCII.GetBytes(textAsset.text));
	}

	private static X509Certificate2Collection GetCertificateList()
	{
		return s_CertificateList;
	}

	public static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		return true;
	}

	private static void PrintChain(X509Chain chain)
	{
		StringBuilder sb = new StringBuilder();
		PrintChainPolicy(sb, chain.ChainPolicy, "");
		PrintChainStatus(sb, chain.ChainStatus, "");
		PrintChainElements(sb, chain, "");
	}

	private static void PrintCertificate(X509Certificate2 cert)
	{
		if (cert != null)
		{
			PrintX509Certificate2(new StringBuilder(), cert, "");
		}
	}

	private static void PrintChainPolicy(StringBuilder sb, X509ChainPolicy policy, string indent)
	{
		sb.Append(indent + "chain policy{\n");
		string text = indent + indenet_one;
		sb.Append(text + "ApplicationPolicy{\n");
		for (int i = 0; i < policy.ApplicationPolicy.Count; i++)
		{
			sb.Append(text + indenet_one + policy.ApplicationPolicy[i].FriendlyName + " : " + policy.ApplicationPolicy[i].Value + "\n");
		}
		sb.Append(text + "}\n");
		sb.Append(text + "CertificatePolicy{\n");
		for (int j = 0; j < policy.CertificatePolicy.Count; j++)
		{
			sb.Append(text + indenet_one + policy.CertificatePolicy[j].FriendlyName + " : " + policy.CertificatePolicy[j].Value + "\n");
		}
		sb.Append(text + "}\n");
		sb.Append(text + "ExtraStore{\n");
		for (int k = 0; k < policy.ExtraStore.Count; k++)
		{
			X509Certificate2 cert = policy.ExtraStore[k];
			PrintX509Certificate2(sb, cert, text + indenet_one);
		}
		sb.Append(text + "}\n");
		sb.Append(text + "RevocationFlag : " + policy.RevocationFlag.ToString() + "\n");
		sb.Append(text + "RevocationMode : " + policy.RevocationMode.ToString() + "\n");
		sb.Append(text + "UrlRetrievalTimeout : " + policy.UrlRetrievalTimeout.ToString() + "\n");
		sb.Append(text + "VerificationFlags : " + policy.VerificationFlags.ToString() + "\n");
		sb.Append(text + "VerificationTime : " + policy.VerificationTime.ToString() + "\n");
		sb.Append(indent + "}\n");
	}

	private static void PrintChainStatus(StringBuilder sb, X509ChainStatus[] chainStatus, string indent)
	{
		sb.Append(indent + "chain status{\n");
		for (int i = 0; i < chainStatus.Length; i++)
		{
			sb.Append(indent + indenet_one + chainStatus[i].Status.ToString() + "\n");
		}
		sb.Append(indent + "}\n");
	}

	private static void PrintChainElements(StringBuilder sb, X509Chain chain, string indent)
	{
		for (int i = 0; i < chain.ChainElements.Count; i++)
		{
			X509ChainElement x509ChainElement = chain.ChainElements[i];
			sb.Append(indent + "chain element" + i + "{\n");
			sb.Append(indent + indenet_one + "Information : " + x509ChainElement.Information + "\n");
			PrintX509Certificate2(sb, x509ChainElement.Certificate, indent + indenet_one);
			PrintChainStatus(sb, x509ChainElement.ChainElementStatus, indent + indenet_one);
			sb.Append(indent + "}\n");
		}
	}

	private static void PrintX509Certificate2(StringBuilder sb, X509Certificate2 cert, string indent)
	{
		sb.Append(indent + "X509Certificate2{\n");
		string text = indent + indenet_one;
		sb.Append(text + "FriendlyName : " + cert.FriendlyName + "\n");
		sb.Append(text + "IssuerName : " + cert.IssuerName.Name + "\n");
		sb.Append(text + "SubjectName : " + cert.SubjectName.Name + "\n");
		sb.Append(text + "NotBefore : " + cert.NotBefore.ToString() + "\n");
		sb.Append(text + "NotAfter : " + cert.NotAfter.ToString() + "\n");
		sb.Append(text + "HasPrivateKey : " + cert.HasPrivateKey + "\n");
		sb.Append(text + "SignatureAlgorithm : " + cert.SignatureAlgorithm.FriendlyName + " : " + cert.SignatureAlgorithm.Value + "\n");
		sb.Append(text + "SerialNumber : " + cert.SerialNumber + "\n");
		sb.Append(text + "Archived : " + cert.Archived + "\n");
		sb.Append(text + "Version : " + cert.Version + "\n");
		sb.Append(text + "Thumbprint : " + cert.Thumbprint + "\n");
		sb.Append(indent + "}\n");
	}

	private static void PrintX509Certificate2(X509Certificate2 cert, string indent)
	{
		PrintX509Certificate2(new StringBuilder(), cert, indent);
	}
}
