using System;
using System.Net;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ImageLoader
{

	public static class DownloadHelper
	{
		public static int Download(string urlToDownload, IProgress<int> progressReporter, 
			object pauseToken, CancellationToken cancelToken, string savePath)
		{
			if (urlToDownload == null || progressReporter == null)
				throw new ArgumentNullException ();

			int receivedBytes = 0;
			int totalBytes = 0;
			WebClient client = new WebClient();
			var allBytesRead = new List<byte> ();

			using (var stream = client.OpenRead (urlToDownload)) {

				byte[] buffer = new byte[4096];
				totalBytes = Int32.Parse (client.ResponseHeaders [HttpResponseHeader.ContentLength]);

				while (true) 
				{
					if (cancelToken.IsCancellationRequested) 
						return receivedBytes;
					Monitor.Enter (pauseToken);
					Monitor.Exit (pauseToken);

					int bytesRead = stream.Read (buffer, 0, buffer.Length);
					if (bytesRead == 0) 
						break;

					allBytesRead.AddRange (buffer.Take (bytesRead));
					receivedBytes += bytesRead;
					int progressPercentage = receivedBytes * 100 / totalBytes;
					progressReporter.Report (progressPercentage);
				}
			}

			File.WriteAllBytes (savePath, allBytesRead.ToArray());
			return receivedBytes;
		
		}
	}
}
