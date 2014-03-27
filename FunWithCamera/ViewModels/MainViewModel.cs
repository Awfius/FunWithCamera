using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.Phone.Media.Capture;

namespace FunWithCamera.ViewModels
{
	public class MainViewModel
	{
		#region Private Members

		private readonly ObservableCollection<VideoBrush> _cameras = new ObservableCollection<VideoBrush>();

		#endregion

		#region Public Properties

		public ReadOnlyObservableCollection<VideoBrush> Cameras { get; private set; }

		#endregion


		#region Constructors

		public MainViewModel()
		{
			Cameras = new ReadOnlyObservableCollection<VideoBrush>(_cameras);
		}

		#endregion


		#region Public Methods

		public async Task Activate(PhotoCaptureDevice photoCaptureDevice)
		{
			var resolution = new Windows.Foundation.Size(640, 480);
			await photoCaptureDevice.SetPreviewResolutionAsync(resolution);

			var cameraEffect = new NokiaImagingSDKEffects();
			cameraEffect.PhotoCaptureDevice = photoCaptureDevice;
			
			var cameraStreamSource = new CameraStreamSource(cameraEffect, resolution);
			
			var mediaElement = new MediaElement();
			mediaElement.Stretch = Stretch.UniformToFill;
			mediaElement.BufferingTime = new TimeSpan(0);
			mediaElement.SetSource(cameraStreamSource);

			var videoBrush = new VideoBrush();
			videoBrush.SetSource(mediaElement);
			await AddCamera(videoBrush);
		}

		public async Task AddCamera(VideoBrush videoBrush)
		{
			await Task.Delay(_cameras.Count * 100);

			_cameras.Add(videoBrush);
		}

		#endregion
	}
}
