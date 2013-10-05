using System;

using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class Texture
    {
        protected static int GetNextPOTS(int wid, int hei)
        {
            int max = wid > hei ? wid : hei;

            return (int) Math.Pow(2.0, Math.Ceiling(Math.Log(max, 2.0)));
        }

        private static Texture _sCurrentLoadedTexture;

        public static Texture Current
        {
            get
            {
                return _sCurrentLoadedTexture;
            }
        }

        private int _id;
        private bool _loaded;

        public TextureTarget TextureTarget { get; private set; }

        public bool Ready
        {
            get
            {
                return _id > -1;
            }
        }

        public int ID
        {
            get
            {
                if (!Ready)
                    GL.GenTextures(1, out _id);

                return _id;
            }
        }

        public Texture(TextureTarget target)
        {
            TextureTarget = target;

            _id = -1;
            _loaded = false;
        }

        public void Update()
        {
            _loaded = false;
        }

        protected virtual void Load()
        {

        }

        public void Bind()
        {
            if (_sCurrentLoadedTexture != this) {
                GL.BindTexture(TextureTarget, ID);
                _sCurrentLoadedTexture = this;
            }

            if (!_loaded) {
                Load();
                _loaded = true;
            }
        }
    }
}
