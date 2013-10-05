using System;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace Zombles.Graphics
{
    public class VertexBuffer : IDisposable
    {
        private int _stride;

        private bool _dataSet = false;

        private int _unitSize;
        private int _vboID;
        private int _length;

        private int VboID
        {
            get
            {
                if (_vboID == 0)
                    GL.GenBuffers(1, out _vboID);

                return _vboID;
            }
        }

        public int Stride
        {
            get { return _stride; }
        }

        public VertexBuffer(int stride)
        {
            _stride = stride;
        }

        public void SetData<T>(T[] vertices) where T : struct
        {
            _unitSize = Marshal.SizeOf(typeof(T));
            _length = vertices.Length / _stride;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * _unitSize), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            CheckForError();

            _dataSet = true;
        }

        private void CheckForError()
        {
            ErrorCode error = GL.GetError();

            if (error != ErrorCode.NoError)
                throw new Exception("OpenGL hates your guts: " + error.ToString());
        }

        public void StartBatch(ShaderProgram shader)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);

            foreach (AttributeInfo info in shader.Attributes) {
                GL.VertexAttribPointer(info.Location, info.Size, info.PointerType,
                    info.Normalize, shader.VertexDataStride, info.Offset);
                GL.EnableVertexAttribArray(info.Location);
            }
        }

        public void Render(ShaderProgram shader, int first = 0, int count = -1)
        {
            if (_dataSet) {
                if (count == -1)
                    count = _length - first;

                GL.DrawArrays(shader.BeginMode, first, count);
            }
        }

        public void EndBatch(ShaderProgram shader)
        {
            foreach (AttributeInfo info in shader.Attributes)
                GL.DisableVertexAttribArray(info.Location);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Dispose()
        {
            if (_dataSet)
                GL.DeleteBuffers(1, ref _vboID);

            _dataSet = false;
        }
    }
}