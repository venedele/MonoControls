using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoControls.Containers.Base;

namespace MonoControls.Containers.Additions
{
    public class InterpolAnimatable : AdvancedAnimatable
    {
        protected Interpolator alpha_ip;
        protected Interpolator size_ip;
        protected Interpolator rotation_ip;

        protected int width_init = 0;
        protected int height_init = 0;

        public InterpolAnimatable(Texture2D texture, Vector2 location, Point size, Color color, float rotation = 0, LinkedList<Animatable> parents = null, Game context = null) : base(context, location, size, color, rotation, parents)
        {
            this.texture = texture;
            width_init = width;
            height_init = height;
        }

        public InterpolAnimatable(Texture2D texture, float x, float y, int width, int height, Color color, float rotation = 0, LinkedList<Animatable> parents = null, Game context = null)
            : this(texture, new Vector2(x, y), new Point(width, height), color, rotation, parents, context)
        { }

        public Animatable setInterpolators(Interpolator alpha, Interpolator size_scale, Interpolator rotation)
        {
            //TODO: Add color
            alpha_ip = alpha;
            alpha_ip?.Reset(this.alpha);
            size_ip = size_scale;
            rotation_ip = rotation;
            rotation_ip?.Reset(this.Rotation);
            return this; 
        }

        public void StartAnimation()
        {
            started = true;
        }

        public bool started = false;

        public void StopAnimation()
        {
            started = false;
        }


        //If you wanna update the animated values, stop, change the values and then reset with reset_values = false;
        public void ResetAnimation(bool reset_values = false)
        {
            started = false;
            if (reset_values)
            {
                alpha = alpha_ip.getInitialValue();
                alpha_ip.Reset();
                size_ip.Reset();
                width = width_init;
                height = height_init;
                Rotation = rotation_ip.getInitialValue();
                rotation_ip.Reset();
            }
            else
            {
                alpha_ip?.Reset(alpha);
                size_ip?.Reset(size_ip.changeable);
                height_init = height;
                width_init = width;
                rotation_ip?.Reset(Rotation);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (started)
            {
                if (alpha_ip != null)
                {
                    alpha = alpha_ip.Update(gameTime);
                }
                if (rotation_ip != null)
                {
                    Rotation = rotation_ip.Update(gameTime);
                }
                if (size_ip != null)
                {
                    float value = size_ip.Update(gameTime);
                    width = (int)(width_init * value);
                    height = (int)(height_init * value);
                }
            }
        }
    }
}
