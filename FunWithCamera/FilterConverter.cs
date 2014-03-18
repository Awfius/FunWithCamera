using FunWithCamera.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Windows.Phone.Media.Capture;
using Microsoft.Devices;

namespace FunWithCamera
{
	public class FilterConverter : DependencyObject, IValueConverter
	{
		private static VideoBrush CreateBrush()
		{
			return new VideoBrush
			{
				Stretch = Stretch.UniformToFill,
				RelativeTransform = new CompositeTransform
				{
					CenterX = .5,
					CenterY = .5,
					ScaleY = -1
				}
			};
		}

		private static NokiaImagingSDKEffects _effect = null;
		private static MediaElement _element = null;
		private static CameraStreamSource _source = null;

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var filter = value == null ? Filter.None : (Filter)value;
			var brush = CreateBrush();
			var capture = Globals.Capture;
			
			if (_effect == null)
			{
				_effect = new NokiaImagingSDKEffects(filter)
				{
					PhotoCaptureDevice = capture
				};
			}
			else
			{
				_effect.FilterName = filter;
				_effect.GetEffect();
			}

			_source = new CameraStreamSource(_effect, capture.PreviewResolution);

			if (_element == null)
			{
				_element = new MediaElement
				{
					Stretch = Stretch.UniformToFill,
					BufferingTime = TimeSpan.Zero
				};
				_element.SetSource(_source);
			}

			brush.SetSource(_element);
			return brush;
		}
		
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new InvalidOperationException("FilterConverter can only be used One-Way.");
		}
	}
}
