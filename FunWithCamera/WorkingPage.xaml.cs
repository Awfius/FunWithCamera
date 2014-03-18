
using FunWithCamera.Core;
using FunWithCamera.Core.ViewModels;
using Microsoft.Devices;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media.Imaging;
using Nokia.Graphics.Imaging;

namespace FunWithCamera
{
	public static class Globals
	{
		public static PhotoCaptureDevice Capture { get; private set; }

		public static async Task Initialize()
		{
			if (Capture != null)
			{
				return;
			}

			var resolutions = PhotoCaptureDevice.GetAvailableCaptureResolutions(CameraSensorLocation.Front);

			if (resolutions.Count == 0)
			{
				throw new InvalidOperationException();
			}

			var resolution = resolutions.First();
			var capture = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Front, resolution);

			capture.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, 90);

			Capture = capture;
		}
	}

	public partial class WorkingPage
	{
		private readonly MainViewModel _viewModel = new MainViewModel();
		
		private static NokiaImagingSDKEffects _effect = null;

		public WorkingPage()
		{
			InitializeComponent();
			
			//Pause 2 seconds for the splash screen duration
			Thread.Sleep(2000);

			DataContext = _viewModel;

			Loaded += OnLoaded;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			await _viewModel.Activate();

			base.OnNavigatedTo(e); 
		}

		private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
		{
			_CameraView.Height = _Viewport.ActualWidth;
			_CameraView.Width = _Viewport.ActualHeight;
			_CameraView.MinHeight = _Viewport.ActualWidth;
			_CameraView.MinWidth = _Viewport.ActualHeight;
			_CameraView.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			_CameraView.VerticalAlignment = System.Windows.VerticalAlignment.Center;
		}
		
		private async void CapturePhoto_Click(object sender, EventArgs e)
		{
			var cameraCaptureSequence = Globals.Capture.CreateCaptureSequence(1);

			var stream = new MemoryStream();
			cameraCaptureSequence.Frames[0].CaptureStream = stream.AsOutputStream();

			await Globals.Capture.PrepareCaptureSequenceAsync(cameraCaptureSequence);
			await cameraCaptureSequence.StartCaptureAsync();

			stream.Seek(0, SeekOrigin.Begin);
			
			var filter = _viewModel.SelectedFilter.Filter;
			if (filter == Filter.None)
			{
				SaveImageToLibrary(stream);
			}
			else
			{
				if (_effect == null)
				{
					_effect = new NokiaImagingSDKEffects(filter);
					_effect.GetEffect();
				}
				else
				{
					_effect.FilterName = filter;
					_effect.GetEffect();
				}

				var filters = _effect.FilterEffect.Filters;

				var imageBitmap = new WriteableBitmap(160, 120);

				using (var source = new StreamImageSource(stream))
				using (var effect = new FilterEffect(source) { Filters = filters })
				using (var renderer = new WriteableBitmapRenderer(effect, imageBitmap))
				{
					imageBitmap = await renderer.RenderAsync();

					var fileStream = new MemoryStream();
					imageBitmap.SaveJpeg(fileStream, imageBitmap.PixelWidth, imageBitmap.PixelHeight, 100, 100);
					fileStream.Seek(0, SeekOrigin.Begin);
					SaveImageToLibrary(fileStream);
				}
			}
		}

		private void SaveImageToLibrary(Stream stream)
		{
			var library = new MediaLibrary();
			library.SavePictureToCameraRoll("picture", stream);
		}
	}

	
}