using System;
using Nez;
using Microsoft.Xna.Framework;


namespace EyesHaveIt
{
    public class CameraBounds : Component, IUpdatable
    {
        public Vector2 Min, Max;


        public CameraBounds()
        {
            // make sure we run last so the camera is already moved before we evaluate its position
            setUpdateOrder(int.MaxValue);
        }


        public CameraBounds(Vector2 min, Vector2 max) : this()
        {
            this.Min = min;
            this.Max = max;
        }


        public override void onAddedToEntity()
        {
            entity.updateOrder = int.MaxValue;
        }


        void IUpdatable.update()
        {
            var cameraBounds = entity.scene.camera.bounds;

            if (cameraBounds.top < Min.Y)
                entity.scene.camera.position += new Vector2(0, Min.Y - cameraBounds.top);

            if (cameraBounds.left < Min.X)
                entity.scene.camera.position += new Vector2(Min.X - cameraBounds.left, 0);

            if (cameraBounds.bottom > Max.Y)
                entity.scene.camera.position += new Vector2(0, Max.Y - cameraBounds.bottom);

            if (cameraBounds.right > Max.X)
                entity.scene.camera.position += new Vector2(Max.X - cameraBounds.right, 0);
        }
    }
}

