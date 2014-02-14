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
using System.IO;

namespace ImageLoader
{
	[Activity (Label = "ImageLoader", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private DownloadListAdapter _adapter;
		private readonly UrlProcessor _validator = new UrlProcessor();

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			var downloadButton = FindViewById<ImageButton>(Resource.Id.downloadButton);
			var downloadsListView = FindViewById<ListView> (Resource.Id.downloadsListView);

			_adapter = new DownloadListAdapter (this);
			downloadsListView.Adapter = _adapter;

			downloadButton.Click += StartDownloadHandler;
		}

		private async void StartDownloadHandler(object sender, System.EventArgs e)
		{

			var fileTextBox = FindViewById<EditText> (Resource.Id.fileTextBox);
			var downloadsListView = FindViewById<ListView> (Resource.Id.downloadsListView);

			var filePath = fileTextBox.Text;
			if (!_validator.Validate (filePath)) 
			{
				new AlertDialog.Builder(this)
					.SetMessage("Invalid file path")
					.SetTitle("Error")
					.Show();
				return;
			}

			var savePath = BuildImageSavePath (filePath);
			var downloadItem = new DownloadItem (filePath, savePath); 
			fileTextBox.Text = String.Empty;
			_adapter.AddItem (downloadItem);


			var task = Task.Run(() => 
				DownloadHelper.Download
				(filePath, downloadItem.ProgressUpdater, downloadItem.PauseToken, downloadItem.CancellationToken, savePath), 
				downloadItem.CancellationToken);
			try
			{
				await Task.WhenAll(new[] {task});
			}
			catch(Exception ex)
			{
				new AlertDialog.Builder(this)
					.SetMessage(ex.Message)
					.SetTitle("Error while loading file")
					.Show();
			}
		}

		private string BuildImageSavePath(string url)
		{
			string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			string localFilename = _validator.GetFileName(url);
			string localPath = Path.Combine (documentsPath, localFilename);
			return localPath;
		}
	}


}


