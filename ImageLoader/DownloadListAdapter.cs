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

namespace ImageLoader
{
	public sealed class DownloadListAdapter : BaseAdapter<DownloadItem>
	{
		private readonly List<DownloadItem> _items = new List<DownloadItem>();
		private readonly Activity _context;
		private readonly Dictionary<View, ViewData> _progressUpdaters = new Dictionary<View, ViewData> ();
		private readonly Dictionary<DownloadItem, View> _activeViews = new Dictionary<DownloadItem, View> ();

		public DownloadListAdapter(Activity context)
			: base()
		{
			if (context == null)
				throw new ArgumentNullException ();

			_context = context;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override DownloadItem this[int position]
		{
			get { return _items[position]; }
		}

		public override int Count
		{
			get { return _items.Count; }
		}
		

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = _items[position];
			View view = convertView;
			if (view == null) 
				view = _context.LayoutInflater.Inflate(Resource.Layout.DownloadRow, null);

			var progressBar = view.FindViewById<ProgressBar> (Resource.Id.downloadProgressBar);
			var fileTextView = view.FindViewById<TextView> (Resource.Id.fileTextView);
			var pauseButton = view.FindViewById<ImageButton> (Resource.Id.pauseButton);
			var cancelButton = view.FindViewById<ImageButton> (Resource.Id.cancelButton);
			var viewButton = view.FindViewById<ImageButton> (Resource.Id.viewButton);

			EventHandler pauseHandler = BuildPauseHandler (item, pauseButton);
			EventHandler cancelHandler = BuildCancelHandler (item);
			EventHandler viewHandler = BuildViewHandler (item);
			UpdateViewData (item, view, pauseButton, cancelButton, viewButton, pauseHandler, cancelHandler, viewHandler);
			progressBar.Progress = item.Progress;

			fileTextView.Text = item.FilePath;


			item.ResetProgressSubscriptions();
			item.ProgressChanged += (progressPercentage) => progressBar.Progress = progressPercentage;
			pauseButton.SetImageResource (item.IsPaused ? Resource.Drawable.control_play : Resource.Drawable.control_pause);
			pauseButton.Click += pauseHandler;
			cancelButton.Click += cancelHandler;
			viewButton.Click += viewHandler;

			return view;
		}
		
		private void UpdateViewData (DownloadItem item, View view, ImageButton pauseButton, ImageButton cancelButton, ImageButton viewButton,
			EventHandler pauseHandler, EventHandler cancelHandler, EventHandler viewHandler)
		{
			if (_progressUpdaters.ContainsKey (view)) {
				var viewData = _progressUpdaters [view];
				pauseButton.Click -= viewData.PauseHandler;
				cancelButton.Click -= viewData.CancelHandler;
				viewButton.Click -= viewData.ViewHandler;

				if (_activeViews [viewData.Item] == view) 
				{
					viewData.Item.ResetProgressSubscriptions ();
					_activeViews [viewData.Item] = null;
				}
				viewData.Item = item;
				viewData.PauseHandler = pauseHandler;
				viewData.CancelHandler = cancelHandler;
				viewData.ViewHandler = viewHandler;
			}
			else
				_progressUpdaters [view] = new ViewData (item, pauseHandler, cancelHandler, viewHandler);
			_activeViews [item] = view;
		}

		private EventHandler BuildPauseHandler(DownloadItem item, ImageButton button)
		{
			EventHandler pauseHandler = (obj, args) => 
			{
				if(item.IsPaused)
				{
					button.SetImageResource(Resource.Drawable.control_pause);
					item.Resume();
				}
				else
				{
					button.SetImageResource(Resource.Drawable.control_play);
					item.Pause();
				}
			};
			return pauseHandler;
		}

		private EventHandler BuildCancelHandler(DownloadItem item)
		{
			EventHandler cancelHandler = (obj, args) => 
			{
				var dialog = new AlertDialog.Builder (_context)
					.SetPositiveButton ("Yes", (sender, a) => 
						{
							if(!_activeViews.ContainsKey(item))
								return;
							var view = _activeViews[item];
							if(view != null && _progressUpdaters.ContainsKey(view))
								_progressUpdaters.Remove(view);
							_activeViews.Remove(item);
							_items.Remove(item);
							item.Cancel();
							NotifyDataSetChanged ();
				})
					.SetNegativeButton ("No", (sender, a) => {})
					.SetMessage (string.Format("Cancel image {0} loading?", item.FilePath))
					.SetTitle ("Cancel");
				dialog.Show();
			};
			return cancelHandler;
		}

		private EventHandler BuildViewHandler(DownloadItem item)
		{
			EventHandler viewHandler = (obj, args) => {
				if(item.Progress < 100)
					return;
				var imageView = new Intent (_context, typeof(ImageViewActivity));
				imageView.PutExtra ("Path", item.SavePath);
				_context.StartActivity (imageView);
			};
			return viewHandler;
		}

		public void AddItem(DownloadItem item)
		{
			if (item == null)
				throw new ArgumentNullException ();

 			_items.Add (item);
			_activeViews [item] = null;
			NotifyDataSetChanged ();
		} 

		private sealed class ViewData
		{
			public ViewData(DownloadItem item, EventHandler pauseHandler, EventHandler cancelHandler, EventHandler viewHandler)
			{
				Item = item;
				PauseHandler = pauseHandler;
				CancelHandler = cancelHandler;
				ViewHandler = viewHandler;
			}

			public DownloadItem Item { get;set; }
			public EventHandler PauseHandler { get; set; }
			public EventHandler CancelHandler { get; set; }
			public EventHandler ViewHandler { get; set; }
		}
	}

	public delegate void ProgressUpdateEventHandler(int progressPercentage);

}

