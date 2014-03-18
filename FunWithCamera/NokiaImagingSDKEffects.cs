/*
 * Copyright © 2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using Nokia.Graphics.Imaging;
using FunWithCamera.Resources;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using Windows.Storage.Streams;
using FunWithCamera.Interfaces;
using FunWithCamera.Core;

namespace FunWithCamera
{
	public class NokiaImagingSDKEffects : ICameraEffect
	{
		private PhotoCaptureDevice _photoCaptureDevice = null;
		private CameraPreviewImageSource _cameraPreviewImageSource = null;
		private CustomEffectBase _customEffect = null;
		private int _effectCount = 11;
		private Semaphore _semaphore = new Semaphore(1, 1);

		public FilterEffect FilterEffect = null;
		public Filter FilterName = Filter.None;
		public String EffectName { get; private set; }
		public int EffectCount { get; private set; }

		public NokiaImagingSDKEffects() { }

		public NokiaImagingSDKEffects(Filter filter)
		{
			FilterName = filter;
		}

		public PhotoCaptureDevice PhotoCaptureDevice
		{
			set
			{
				if (_photoCaptureDevice != value)
				{
					while (!_semaphore.WaitOne(100)) ;

					_photoCaptureDevice = value;

					Initialize();

					_semaphore.Release();
				}
			}
		}

		~NokiaImagingSDKEffects()
		{
			while (!_semaphore.WaitOne(100)) ;

			Uninitialize();

			_semaphore.Release();
		}

		public async Task GetNewFrameAndApplyEffect(IBuffer frameBuffer, Size frameSize)
		{
			if (_semaphore.WaitOne(500))
			{
				_cameraPreviewImageSource.InvalidateLoad();
				var scanlineByteSize = (uint)frameSize.Width * 4; // 4 bytes per pixel in BGRA888 mode
				var bitmap = new Bitmap(frameSize, ColorMode.Bgra8888, scanlineByteSize, frameBuffer);

				if (FilterEffect != null)
				{
					var renderer = new BitmapRenderer(FilterEffect, bitmap);
					await renderer.RenderAsync();
				}
				else if (_customEffect != null)
				{
					var renderer = new BitmapRenderer(_customEffect, bitmap);
					await renderer.RenderAsync();
				}
				else
				{
					var renderer = new BitmapRenderer(_cameraPreviewImageSource, bitmap);
					await renderer.RenderAsync();
				}

				_semaphore.Release();
			}
		}

		//public void NextEffect()
		//{
		//	if (_semaphore.WaitOne(500))
		//	{
		//		Uninitialize();

		//		_effectIndex++;

		//		if (_effectIndex >= _effectCount)
		//		{
		//			_effectIndex = 0;
		//		}

		//		Initialize();

		//		_semaphore.Release();
		//	}
		//}

		//public void PreviousEffect()
		//{
		//	if (_semaphore.WaitOne(500))
		//	{
		//		Uninitialize();

		//		_effectIndex--;

		//		if (_effectIndex < 0)
		//		{
		//			_effectIndex = _effectCount - 1;
		//		}

		//		Initialize();

		//		_semaphore.Release();
		//	}
		//}

		public void GetEffect()
		{
			if (_semaphore.WaitOne(500))
			{
				Uninitialize();
				Initialize();

				_semaphore.Release();
			}
		}

		private void Uninitialize()
		{
			if (_cameraPreviewImageSource != null)
			{
				_cameraPreviewImageSource.Dispose();
				_cameraPreviewImageSource = null;
			}

			if (FilterEffect != null)
			{
				FilterEffect.Dispose();
				FilterEffect = null;
			}

			if (_customEffect != null)
			{
				_customEffect.Dispose();
				_customEffect = null;
			}
		}

		private void Initialize()
		{
			var filters = new List<IFilter>();

			EffectCount = _effectCount;

			if (_photoCaptureDevice != null)
			{
				_cameraPreviewImageSource = new CameraPreviewImageSource(_photoCaptureDevice);
			}

			EffectName = FilterName.ToString();
			switch (FilterName)
			{
				case Filter.Antique:
					filters.Add(new AntiqueFilter());
					break;
				case Filter.AutoEnhance:
					filters.Add(new AutoEnhanceFilter());
					break;
				case Filter.AutoLevels:
					filters.Add(new AutoLevelsFilter());
					break;
				case Filter.Blur:
					filters.Add(new BlurFilter());
					break;
				case Filter.Brightness:
					filters.Add(new BrightnessFilter());
					break;
				case Filter.Cartoon:
					filters.Add(new CartoonFilter());
					break;
				case Filter.ChromaKey:
					filters.Add(new ChromaKeyFilter());
					break;
				case Filter.ColorAdjust:
					filters.Add(new ColorAdjustFilter());
					break;
				case Filter.ColorBoost:
					filters.Add(new ColorBoostFilter());
					break;
				case Filter.Colorization:
					filters.Add(new ColorizationFilter());
					break;
				case Filter.ColorSwap:
					filters.Add(new ColorSwapFilter());
					break;
				case Filter.Contrast:
					filters.Add(new ContrastFilter());
					break;
				case Filter.Crop:
					filters.Add(new CropFilter());
					break;
				case Filter.Curves:
					filters.Add(new CurvesFilter());
					break;
				case Filter.Despeckle:
					filters.Add(new DespeckleFilter());
					break;
				case Filter.Emboss:
					filters.Add(new EmbossFilter(0.5));
					break;
				case Filter.Exposure:
					filters.Add(new ExposureFilter());
					break;
				case Filter.Flip:
					filters.Add(new FlipFilter());
					break;
				case Filter.Foundation:
					filters.Add(new FoundationFilter());
					break;
				case Filter.Grayscale:
					filters.Add(new GrayscaleFilter());
					break;
				case Filter.GrayscaleNegative:
					filters.Add(new GrayscaleNegativeFilter());
					break;
				case Filter.HueSaturation:
					filters.Add(new HueSaturationFilter());
					break;
				case Filter.Lomo:
					filters.Add(new LomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Yellow));
					break;
				case Filter.MagicPen:
					filters.Add(new MagicPenFilter());
					break;
				case Filter.Milky:
					filters.Add(new MilkyFilter());
					break;
				case Filter.Mirror:
					filters.Add(new MirrorFilter());
					break;
				case Filter.MonoColor:
					filters.Add(new MonoColorFilter());
					break;
				case Filter.Moonlight:
					filters.Add(new MoonlightFilter());
					break;
				case Filter.Negative:
					filters.Add(new NegativeFilter());
					break;
				case Filter.Noise:
					filters.Add(new NoiseFilter());
					break;
				case Filter.Oily:
					filters.Add(new OilyFilter());
					break;
				case Filter.Paint:
					filters.Add(new PaintFilter());
					break;
				case Filter.Posterize:
					filters.Add(new PosterizeFilter());
					break;
				case Filter.Reframing:
					filters.Add(new ReframingFilter());
					break;
				case Filter.Rotation:
					filters.Add(new RotationFilter());
					break;
				case Filter.Sepia:
					filters.Add(new SepiaFilter());
					break;
				case Filter.Sharpness:
					filters.Add(new SharpnessFilter());
					break;
				case Filter.Sketch:
					filters.Add(new SketchFilter(SketchMode.Color));
					break;
				case Filter.Solarize:
					filters.Add(new SolarizeFilter());
					break;
				case Filter.Spotlight:
					filters.Add(new SpotlightFilter(new Windows.Foundation.Point(400, 300), 1200, 0.5));
					break;
				case Filter.Stamp:
					filters.Add(new StampFilter(5, 0.7));
					break;
				case Filter.TemperatureAndTint:
					filters.Add(new TemperatureAndTintFilter());
					break;
				case Filter.Vignetting:
					filters.Add(new VignettingFilter());
					break;
				case Filter.Warp:
					filters.Add(new WarpFilter(WarpEffect.Twister, 1.0));
					break;
				case Filter.Watercolor:
					filters.Add(new WatercolorFilter());
					break;
				case Filter.WhiteBalance:
					filters.Add(new WhiteBalanceFilter());
					break;
				case Filter.WhiteboardEnhancement:
					filters.Add(new WhiteboardEnhancementFilter());
					break;

				//case 10:
				//	{
				//		EffectName = String.Format(nameFormat, AppResources.Filter_Custom);
				//		_customEffect = new CustomEffect(_cameraPreviewImageSource);
				//	}
				//	break;
			}

			if (filters.Count > 0)
			{
				if (_cameraPreviewImageSource != null)
				{
					FilterEffect = new FilterEffect(_cameraPreviewImageSource)
					{
						Filters = filters
					};
				}
				else
				{
					FilterEffect = new FilterEffect()
					{
						Filters = filters
					};
				}
			}
		}
	}
}
