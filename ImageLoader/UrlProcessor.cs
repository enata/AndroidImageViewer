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
using System.Text.RegularExpressions;

namespace ImageLoader
{

	public sealed class UrlProcessor
	{
		private readonly Regex _urlRegex = new Regex(@"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?)/");
		private readonly Regex _fileNameRegex = new Regex (@"(?:[^/][\d\w\.]+)$(?<=\.\w{3,4})");
		private const string DefaultFileName = "noName";

		public bool Validate(string url)
		{
			return _urlRegex.IsMatch (url);
		}

		public string GetFileName(string url)
		{
			var match = _fileNameRegex.Match (url);
			string result = match.Success ? match.Value : DefaultFileName;
			return result;
		}
	}

}
