using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;

namespace ImageLoader
{

	public sealed class DownloadItem
	{
		private readonly string _filePath;
		private readonly string _savePath;
		private readonly Progress<int> _progressUpdater = new Progress<int>();
		private readonly object _pauseToken = new object();
		private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource ();

		private bool _paused = false;
		private int _progress = 0;

		public DownloadItem(string filePath, string savePath)
		{
			_progressUpdater.ProgressChanged += (sender, newProgress) => 
			{
				_progress = newProgress;
				if(ProgressChanged != null)
					ProgressChanged(newProgress);
			};

			_filePath = filePath;
			_savePath = savePath;
		}

		public string FilePath { get { return _filePath; } }
		public string SavePath { get { return _savePath; } }
		public Progress<int> ProgressUpdater { get { return _progressUpdater; } }
		public int Progress { get { return _progress; } }
		public object PauseToken { get { return _pauseToken; } }
		public bool IsPaused { get { return _paused; } }
		public CancellationToken CancellationToken{ get {return _cancellationToken.Token; } }

		public event ProgressUpdateEventHandler ProgressChanged;

		public void ResetProgressSubscriptions()
		{
			ProgressChanged = null;
		}

		public void Pause()
		{
			if (!_paused) 
			{
				Monitor.Enter (_pauseToken);
				_paused = true;
			}
		}

		public void Resume()
		{
			if (_paused) 
			{
				Monitor.Exit (_pauseToken);
				_paused = false;
			}
		}

		public void Cancel()
		{
			Resume ();
			ResetProgressSubscriptions ();
			_cancellationToken.Cancel ();
		}
	}

}
