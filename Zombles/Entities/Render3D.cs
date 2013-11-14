using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zombles.Graphics;

namespace Zombles.Entities
{
    public class Render3D : Component
    {
        private Matrix4 _transform;
        private bool _transformInvalid;

        public Quaternion Rotation { get; set; }
        public EntityModel Model { get; set; }

        public Render3D(Entity ent)
            : base(ent)
        {
            _transform = Matrix4.Identity;
            _transformInvalid = true;

            Rotation = Quaternion.Identity;
            Model = null;
        }

        public virtual void OnRender(ModelEntityShader shader)
        {
            if (_transformInvalid) {
                _transform = Matrix4.Mult(
                    Matrix4.CreateTranslation(Entity.Position),
                    Matrix4.CreateFromQuaternion(Rotation));

                _transformInvalid = false;
            }

            shader.Render(Model, _transform);
        }
    }
}
