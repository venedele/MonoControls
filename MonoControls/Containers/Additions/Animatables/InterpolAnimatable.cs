using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoControls.Containers.Base;

namespace MonoControls.Containers.Additions.Animatables
{
    public class InterpolAnimatable : AdvancedAnimatable
    {
        protected AdvancedInterpolator alpha_ip;
        protected AdvancedInterpolator size_ip;
        protected AdvancedInterpolator rotation_ip;

        protected int width_init = 0;
        protected int height_init = 0;

        new public float Rotation
        {
            get { return base.Rotation; }
            set { if (rotation_ip == null) base.Rotation = value; else rotation_ip.setTarget(value); }
        }

        public float getRotationTarget()
        {
            return rotation_ip == null ? base.Rotation : rotation_ip.Target;
        }

        //TOOD: Consider overriding instead of new
        new public float alpha
        {
            get { return base.alpha; }
            set { if (alpha_ip == null) base.alpha = value; else alpha_ip.setTarget(value); }
        }

        public float getAlphaTarget()
        {
            return alpha_ip == null ? base.alpha : alpha_ip.Target;
        }

        public bool Running
        {
            get
            {
                if (alpha_ip != null)
                {
                    if (alpha_ip.Running) return true;
                }
                if (size_ip != null)
                {
                    if (size_ip.Running) return true;
                }
                if (rotation_ip != null)
                {
                    if (rotation_ip.Running) return true;
                }
                return false;
            }
        }

        public InterpolAnimatable(Texture2D texture, Vector2 location, Point size, Color color, float rotation = 0, LinkedList<Animatable> parents = null, Game context = null) : base(context, location, size, color, rotation, parents)
        {
            this.texture = texture;
            width_init = width;
            height_init = height;
        }

        public InterpolAnimatable(SpriteFont spriteFont, String str, Vector2 location, Color color, int containerwidth = 0, int containerheight = 0, float rotation = 0, LinkedList<Animatable> parents = null, Game context = null) : base(context, location, new Point(containerwidth, containerheight), color, rotation, parents)
        {
            this.spriteFont = spriteFont;
            this.str = str;
            width_init = 1; //Font Scaling factor
        }

        public InterpolAnimatable(Texture2D texture, float x, float y, int width, int height, Color color, float rotation = 0, LinkedList<Animatable> parents = null, Game context = null)
            : this(texture, new Vector2(x, y), new Point(width, height), color, rotation, parents, context)
        { }

        public InterpolAnimatable(SpriteFont spriteFont, String str, float x, float y, Color color, int containerwidth = 0, int containerheight = 0, float rotation = 0, LinkedList<Animatable> parents = null, Game context = null)
            : this(spriteFont, str, new Vector2(x, y), color, containerwidth, containerheight, rotation, parents, context)
        { }

        public Animatable setInterpolators(AdvancedInterpolator alpha, AdvancedInterpolator size_scale, AdvancedInterpolator rotation)
        {
            //TODO: Add color
            alpha?.Reset(this.alpha);
            alpha_ip = alpha;
            size_ip?.Reset(1f);
            size_ip = size_scale;
            rotation_ip?.Reset(this.Rotation);
            rotation_ip = rotation;
            return this; 
        }

        private bool animation_running = true;

        public void StartAnimation()
        {
            animation_running = true;
        }

        public void ForceStartAnimation()
        {
            animation_running = true;
            alpha_ip?.ForceStart();
            size_ip?.ForceStart();
            rotation_ip?.ForceStart();
        }

        public void PauseAnimation()
        {
            animation_running = false;
        }


        //If you wanna update the animated values, stop, change the values and then reset with reset_values = false;
        public void ResetAnimation(bool reset_values = false)
        {
            if (reset_values)
            {
                alpha = alpha_ip.starting;
                alpha_ip?.Reset();
                size_ip?.Reset();
                width = width_init;
                height = height_init;
                Rotation = rotation_ip.starting;
                rotation_ip?.Reset();
            }
            else
            {
                alpha_ip?.Reset();
                size_ip?.Reset();
                height_init = height;
                width_init = width;
                rotation_ip?.Reset();
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (animation_running)
            {
                if (alpha_ip != null)
                {
                    base.alpha = alpha_ip.Update(gameTime);
                }
                if (rotation_ip != null)
                {
                    base.Rotation = rotation_ip.Update(gameTime);
                }
                if (size_ip != null)
                {
                    float value = size_ip.Update(gameTime);
                    if (texture == null)
                        base.Scale = new Vector2((width_init + value), (width_init + value));
                    else
                    {
                        base.width = (int)(width_init + value);
                        base.height = (int)(height_init + value);
                    }
                }
            }
        }
    }
}
