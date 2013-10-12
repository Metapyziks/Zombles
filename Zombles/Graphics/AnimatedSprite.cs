using System;
using OpenTK;
using OpenTKTK.Scene;
using OpenTKTK.Shaders;
using OpenTKTK.Textures;

namespace Zombles.Graphics
{
    public class AnimatedSprite : Sprite
    {
        private static long CurrentMilliseconds()
        {
            return DateTime.Now.Ticks / 10000;
        }

        private int _frameWidth;
        private int _frameHeight;

        private long _startTime;
        private long _stopTime;

        private Vector2[] _frameLocations;
        private int _lastFrame;

        private bool _playing;

        public int StartFrame;
        public int FrameCount;

        public double FrameRate;

        public AnimatedSprite(BitmapTexture2D texture, int frameWidth, int frameHeight, double frameRate, float scale = 1.0f)
            : base(texture, scale)
        {
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;

            FrameRate = frameRate;

            _startTime = 0;
            _stopTime = 0;

            SubrectSize = new Vector2(frameWidth, frameHeight);

            FindFrameLocations();

            StartFrame = 0;
            FrameCount = _frameLocations.Length;

            _lastFrame = -1;
        }

        private void FindFrameLocations()
        {
            int xMax = Texture.Width / _frameWidth;
            int yMax = Texture.Height / _frameHeight;

            int frameCount = xMax * yMax;

            _frameLocations = new Vector2[frameCount];

            int i = 0;

            for (int y = 0; y < yMax; ++y)
                for (int x = 0; x < xMax; ++x, ++i)
                    _frameLocations[i] = new Vector2(x * _frameWidth, y * _frameHeight);
        }

        public void Start()
        {
            if (!_playing) {
                _startTime = CurrentMilliseconds();
                _playing = true;
            }
        }

        public void Stop()
        {
            if (_playing) {
                _stopTime = CurrentMilliseconds() - _startTime;
                _playing = false;
            }
        }

        public void Reset()
        {
            _stopTime = 0;

            if (_playing)
                _startTime = CurrentMilliseconds();
        }

        public override void Render(SpriteShader shader)
        {
            double secs = (CurrentMilliseconds() - _startTime + _stopTime) / 1000.0;
            int frame = StartFrame + (int) ((long) (secs * FrameRate) % (long) FrameCount);

            if (frame != _lastFrame)
                SubrectOffset = _frameLocations[frame];

            base.Render(shader);
        }
    }
}
