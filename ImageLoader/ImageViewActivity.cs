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
using Android.Graphics;

namespace ImageLoader
{
	[Activity (Label = "ImageViewActivity")]			
	public class ImageViewActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.ImageView);

			var view = FindViewById<ImageView> (Resource.Id.imageView);
			var closeButton = FindViewById<ImageButton> (Resource.Id.closeButton);
			var textBox = FindViewById<TextView> (Resource.Id.fileNameTextView);

			var path = Intent.GetStringExtra ("Path");
			textBox.Text = path;
			closeButton.Click += (sender, e) => Finish();

			Bitmap bitmap = null;
			try
			{
				bitmap = BitmapFactory.DecodeFile (path);
			}
			catch(Exception ex) 
			{
				new AlertDialog.Builder(this)
					.SetMessage(ex.Message)
					.SetTitle("Can't display image")
					.Show();
			}

			if (bitmap != null) 
			{
				view.SetImageBitmap (bitmap);
			}
		}
	}
}

