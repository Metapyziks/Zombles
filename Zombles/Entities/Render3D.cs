using System;
using OpenTK;
using Zombles.Graphics;

namespace Zombles.Entities
{
    public class Render3D : Component
    {
        private Vector3 _lastPos;
        
        private Quaternion _rotation;
        private Vector3 _offset;
        private Vector3 _scale;

        private Matrix4 _transform;
        private bool _transformInvalid;

        public Quaternion Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _transformInvalid = true;
            }
        }

        public Vector3 Offset
        {
            get { return _offset; }
            set
            {
                _offset = value;
                _transformInvalid = true;
            }
        }

        public Vector3 Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                _transformInvalid = true;
            }
        }

        public EntityModel Model { get; set; }
        public int Skin { get; set; }

        public Render3D(Entity ent)
            : base(ent)
        {
            _transform = Matrix4.Identity;
            _transformInvalid = true;

            Rotation = Quaternion.Identity;
            Scale = new Vector3(1f, 1f, 1f);
            Model = null;
            Skin = 0;
        }

        public Render3D SetRotation(float angle)
        {
            Rotation = Quaternion.FromAxisAngle(Tools.Up, angle);
            return this;
        }

        public Render3D SetRotation(Vector3 axis, float angle)
        {
            Rotation = Quaternion.FromAxisAngle(axis, angle);
            return this;
        }

        public Render3D SetRotation(Quaternion quaternion)
        {
            Rotation = quaternion;
            return this;
        }

        public Render3D SetModel(EntityModel model)
        {
            Model = model;
            return this;
        }

        public Render3D SetSkin(int id)
        {
            Skin = id;
            return this;
        }

        public Render3D SetSkin(Random rand)
        {
            Skin = rand.Next(Model.Skins);
            return this;
        }

        public Render3D SetOffset(Vector3 offset)
        {
            Offset = offset;
            return this;
        }

        public Render3D SetOffset(float x, float y, float z)
        {
            Offset = new Vector3(x, y, z);
            return this;
        }

        public Render3D SetScale(float scale)
        {
            Scale = new Vector3(scale, scale, scale);
            return this;
        }

        public Render3D SetScale(Vector3 scale)
        {
            Scale = scale;
            return this;
        }

        public Render3D SetScale(float x, float y, float z)
        {
            Scale = new Vector3(x, y, z);
            return this;
        }

        public virtual void OnRender(ModelEntityShader shader)
        {
            if (_transformInvalid || _lastPos != Position) {
                _transform =
                    Matrix4.Mult(
                        Matrix4.Mult(
                            Matrix4.Mult(
                                Matrix4.CreateTranslation(_offset),
                                Matrix4.CreateScale(_scale)),
                        Matrix4.CreateFromQuaternion(Rotation)),
                    Matrix4.CreateTranslation(Entity.Position));

                _transformInvalid = false;
                _lastPos = Position;
            }

            if (Model != null) {
                shader.Render(Model, Skin, _transform);
            }
        }
    }
}
