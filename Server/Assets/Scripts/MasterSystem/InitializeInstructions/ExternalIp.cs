
using System;
using System.Net;
using UnityEngine;

public static class ExternalIp {

	public static string GetIpAdress() {

		string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
		var externalIp = IPAddress.Parse(externalIpString);

		Console.WriteLine(externalIp.ToString());
		return externalIp.ToString();

	}


}
