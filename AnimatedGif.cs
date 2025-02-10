using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace AnimatedGifWPF
{
    public class AnimatedGif : Image
    {
        private GifBitmapDecoder _gifDecoder;
        private int _frameCount;
        private int _currentFrame;
        private WriteableBitmap _writeableBitmap;
        private List<BitmapFrame> _cachedFrames;
        private List<int> _frameDelays;
        private TimeSpan _lastRenderTime;

        public AnimatedGif()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_gifDecoder != null)
            {
                StartAnimation();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopAnimation();
        }

        public void SetGifSource(Uri uri)
        {
            _gifDecoder = new GifBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            _frameCount = _gifDecoder.Frames.Count;
            _currentFrame = 0;
            CacheFrames();
            Source = _writeableBitmap;

            if (IsLoaded)
            {
                StartAnimation();
            }
        }

        private void CacheFrames()
        {
            _cachedFrames = _gifDecoder.Frames.ToList();
            var pixelWidth = _cachedFrames[0].PixelWidth;
            var pixelHeight = _cachedFrames[0].PixelHeight;

            PixelFormat pixelFormat = _cachedFrames[0].Format == PixelFormats.Indexed8 || _cachedFrames[0].Format == PixelFormats.Indexed4
                ? PixelFormats.Bgra32
                : _cachedFrames[0].Format;

            _writeableBitmap = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96, pixelFormat, null);

            if (pixelFormat == PixelFormats.Bgra32)
            {
                _cachedFrames = _cachedFrames.Select(frame => ConvertToBgra32(frame)).ToList();
            }

            _frameDelays = _gifDecoder.Frames.Select(frame => GetFrameDelay(frame)).ToList();
        }

        private int GetFrameDelay(BitmapFrame frame)
        {
            if (frame.Metadata is BitmapMetadata metadata && metadata.ContainsQuery("/grctlext/Delay"))
            {
                ushort delay = (ushort)metadata.GetQuery("/grctlext/Delay");
                return Math.Max(10, delay * 10);
            }
            return 100;
        }

        private BitmapFrame ConvertToBgra32(BitmapFrame frame)
        {
            var formatConvertedBitmap = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);
            return BitmapFrame.Create(formatConvertedBitmap);
        }

        private void StartAnimation()
        {
            CompositionTarget.Rendering -= OnRendering;
            CompositionTarget.Rendering += OnRendering;
        }

        private void StopAnimation()
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            var renderEventArgs = (RenderingEventArgs)e;
            var frameDelay = _frameDelays[_currentFrame];

            if (renderEventArgs.RenderingTime - _lastRenderTime >= TimeSpan.FromMilliseconds(frameDelay))
            {
                _lastRenderTime = renderEventArgs.RenderingTime;
                UpdateFrame();
            }
        }

        private void UpdateFrame()
        {
            if (_cachedFrames != null && _frameCount > 1)
            {
                _currentFrame = (_currentFrame + 1) % _frameCount;
                var frame = _cachedFrames[_currentFrame];

                frame.CopyPixels(new Int32Rect(0, 0, frame.PixelWidth, frame.PixelHeight),
                                  _writeableBitmap.BackBuffer,
                                  _writeableBitmap.BackBufferStride * frame.PixelHeight,
                                  _writeableBitmap.BackBufferStride);

                _writeableBitmap.Lock();
                _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, frame.PixelWidth, frame.PixelHeight));
                _writeableBitmap.Unlock();
            }
        }
    }
}
