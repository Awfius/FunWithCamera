using FunWithCamera.Core;
using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FunWithCamera.CustomControls
{
	public class FilteredImageControl : Control
	{
		#region Constructors
		/// <summary>
		/// Creates a new instance of <see cref="FilteredImageControl"/>.
		/// </summary>
		public FilteredImageControl()
        {
			this.DefaultStyleKey = typeof(FilteredImageControl);
        }

		#endregion

		#region Private Members

		private Image _filteredImage;
		private TextBlock _filteredImageName;

		private static NokiaImagingSDKEffects _effect = null;
		
		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty FilteredImageProperty = DependencyProperty.Register(
			"FilteredImage",
			typeof(Filter),
			typeof(FilteredImageControl),
			null
		);

		public Filter FilteredImage
		{
			get { return (Filter)GetValue(FilteredImageProperty); }
			set { SetValue(FilteredImageProperty, value); }
		}

		#endregion

		#region Event Handling

		/// <summary>
		/// Called when the control template is applied.
		/// </summary>
		public async override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_filteredImage = (Image)GetTemplateChild("FilteredImage");
			//_filteredImageName = (TextBlock)GetTemplateChild("FilteredImageName");

			var filter = FilteredImage;
			var uri = new Uri("Assets/Images/soccerball.jpg", UriKind.Relative);
			var stream = App.GetResourceStream(uri).Stream;

			if (filter == Filter.None)
			{
				var image = new BitmapImage();
				image.SetSource(stream);
				_filteredImage.Source = image;
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
				//_filteredImageName.Text = _effect.EffectName;
				var filters = _effect.FilterEffect.Filters;

				var imageBitmap = new WriteableBitmap(160, 120);
				
				using (var source = new StreamImageSource(stream))
				using (var effect = new FilterEffect(source) { Filters = filters })
				using (var renderer = new WriteableBitmapRenderer(effect, imageBitmap))
				{
					imageBitmap = await renderer.RenderAsync();
					_filteredImage.Source = imageBitmap;
				}
			}
		}
		#endregion
	}
}
