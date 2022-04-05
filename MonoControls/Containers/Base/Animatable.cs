using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoControls.Inputs.Mouse;

namespace MonoControls.Containers.Base
{
    //TODO: Inherit DrawableGameComponent if possible
    public class Animatable : LinkedList<Animatable>
    {

        public int id;

        private Vector2 scale = new Vector2(1, 1);

        public Vector2 Scale
        {
            get { return scale; }
            set { if (texture == null) scale = value; else throw new NotImplementedException("Currently scale changing is not supported for Textured Animatables."); }
        }

        public Mutex drawing = new Mutex();

        public void UpdateScale()
        {
            if (texture != null)
            {
                Point temp = size;
                scale = new Vector2(temp.X / (float)(texture.Width), temp.Y / (float)(texture.Height));
            }
            else scale = new Vector2(1f, 1f);
        }

        public SpriteEffects effects = SpriteEffects.None;
        public float alpha = 1;
        public Animatable parent = null;
        public bool size_locked = false;
        private Texture2D texture_c = null;
        public Texture2D texture
        {
            get { return texture_c;  }
            set { texture_c = value; UpdateScale(); }
        }
        protected SpriteFont spriteFont = null;
        public String str = null;
        public Color color;
        private Vector2 location_c;
        public Vector2 location {
            get { return location_c; }
            set { location_c = value;
                UpdateLocationCache();
                if (event_handler != null) UpdateMouseevent(); 
                foreach (Animatable chi in this) 
                    if (chi.event_handler != null) 
                        chi.UpdateMouseevent();
            }
        }
        private Point size_c;
        public Point size
        {
            get { return (size_c == Point.Zero && texture_c!=null)?texture_c.Bounds.Size:size_c; }
            set { size_c = value; 
                UpdateLocationCache();
                if (event_handler != null) UpdateMouseevent(); 
                foreach (Animatable chi in this) 
                    if (chi.event_handler != null) chi.UpdateMouseevent();
                UpdateScale();
                
            }
        }



        private Vector2 draw_location_cached;
        private void UpdateLocationCache()
        {
            Point temp = GetSize();
            draw_location_cached = (location + (this.centerCoords ? Vector2.Zero : new Vector2(temp.X / 2f, temp.Y / 2f)));
        }



        private bool centerCoords = false;
        public bool isCenterCoord
        {
            get { return centerCoords; }
        }
        public void setCentralCoords(bool center_coords)
        {
            centerCoords = center_coords;
            UpdateLocationCache();
            UpdateMouseevent();
        }

        public float X
        {
            get { return location.X; }
            set { location_c.X = value;
                UpdateLocationCache();
                if (event_handler != null) UpdateMouseevent(); 
                foreach (Animatable chi in this) 
                    if (chi.event_handler != null) 
                        chi.UpdateMouseevent();
            }
        }

        public float Y
        {
            get { return location.Y; }
            set { 
                location_c.Y = value;
                UpdateLocationCache();
                if (event_handler != null) UpdateMouseevent(); 
                foreach (Animatable chi in this) 
                    if (chi.event_handler != null) chi.UpdateMouseevent();
            }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public int width
        {
            get { return size.X; }
            protected set { 
                size_c.X = value;
                UpdateLocationCache();
            }
        }

        public int height
        {
            get { return size.Y; }
            protected set { 
                size_c.Y = value;
                UpdateLocationCache();
            }
        }


        public float Xposition_left
        {
            get { return location.X - (this.centerCoords?(width / 2.0f):0); }
        }

        public float Xposition_right
        {
            get { return location.X + (this.centerCoords ? (width / 2.0f) : 0); }
        }

        public float Yposition_top
        {
            get { return location.Y - (this.centerCoords ? height / 2.0f:0); }
        }

        public float Yposition_bottom
        {
            get { return location.Y + (this.centerCoords ? height / 2.0f:0); }
        }

        private float rotation = 0.0f;

        private Mouse_Event event_handler = null;

        public Animatable SetParent(Animatable parent)
        {
            this.parent = parent;
            return this;
        }

        //Rectangular coordinate
        public Vector2 GetGlobalLocation()
        {
            return location - (isCenterCoord?(size.ToVector2()/2f):Vector2.Zero) + ((parent == null) ? new Vector2(0, 0) : parent.GetGlobalLocation());
        }

        public Vector2 GetGlobalLocationCenter()
        {
            return GetGlobalLocation() + size.ToVector2() / 2f;
        }

        public Point GetSize()
        {
            return size;
        }

        public Point GetContainerSize()
        {
            return size;
        }








        public Animatable (Texture2D texture, Vector2 location, Point size, Color color, float rotation = 0, LinkedList<Animatable> parents = null)
        {
            id = new Random().Next();
            this.texture = texture; this.location = location; this.size = size ; Rotation = rotation;
            if (parents != null) foreach (Animatable a in parents)
                { this.AddLast(a.SetParent(this)); }
            //Splitting the alpha value from the color for easier editing
            alpha = color.A/(float)255;
            color.A = 255; this.color = color;
        }

        public Animatable(Texture2D texture, float x, float y, int width, int height, Color color, float rotation = 0f, LinkedList<Animatable> parents = null)
          : this(texture, new Vector2(x, y), new Point(width, height), color, rotation, parents)
        {}

        public Animatable(SpriteFont spriteFont, String str, Vector2 location, Color color, int containerwidth = 0, int containerheight = 0, float rotation = 0, LinkedList<Animatable> parents = null)
            : this(null, location, new Point(containerwidth, containerheight), color, rotation, parents)
        {
            this.spriteFont = spriteFont; 
            this.str = str;
        }

        public Animatable(SpriteFont spriteFont, String str, float x, float y, Color color, int containerwidth = 0, int containerheight = 0, float rotation = 0, LinkedList<Animatable> parents = null)
            :this(spriteFont, str, new Vector2(x, y), color, containerwidth, containerheight, rotation, parents)
        {}

        protected Animatable()
        {

        }






        public Animatable Add(Animatable child)
        {
            drawing.WaitOne();
            this.AddLast(child.SetParent(this));
            drawing.ReleaseMutex();
            return this;
        }

        public Animatable Add(LinkedList<Animatable> parents)
        {
            drawing.WaitOne();
            if (parents != null) foreach (Animatable a in parents)
                { this.AddLast(a.SetParent(this)); }
            drawing.ReleaseMutex();
            return this;
        }




        public Animatable SetEffect(SpriteEffects eff)
        {
            effects = eff;
            return this;
        }


        public Animatable SetId(int id = 0)
        {
            this.id = id; 
            return this;
        }

        public Mouse_Event AddMouseEvents(Func<Animatable, MouseKeys, short> OnKeyChange, Func<Animatable, bool, short> OnHoverChange)
        {
            Vector2 temp = GetGlobalLocation();
            event_handler = new Mouse_Event(new Rectangle(new Point((int)temp.X, (int)temp.Y), GetSize()), delegate(MouseKeys mouse) { if (OnKeyChange != null) OnKeyChange.Invoke(this, mouse); return 0; }, delegate (bool hover) { if (OnHoverChange != null) OnHoverChange.Invoke(this, hover);  return 0; });
            return event_handler;
        }

        public Animatable AddMouseEvents(Func<Animatable, MouseKeys, short> OnKeyChange, Func<Animatable, bool, short> OnHoverChange, Mouse_Handler attachto, bool priority = false)
        {
            attachto.Add(AddMouseEvents(OnKeyChange, OnHoverChange), priority);
            return this;
        }


        //Mouse event locations are inaccurate when animatable is rotated
        public void UpdateMouseevent()
        {
            Vector2 temp = GetGlobalLocation();
            event_handler.region = new Rectangle(new Point((int)temp.X, (int)temp.Y), GetSize());
        }

        ~Animatable()
        {
            Dispose();
        }


        public virtual void Dispose()
        {

        }


        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, Vector2.Zero);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 cord_root)
        {
            Draw(spriteBatch, cord_root, 1);
        }

        //The alphal parameter is mainly used for Animatables to inherit their parents alpha
        public virtual void Draw(SpriteBatch spriteBatch, Vector2 cord_root, float alphal = 1)
        {
            drawing.WaitOne();
            if (texture != null)
            {
                if (alpha > 0)
                {
                        spriteBatch.Draw(texture, cord_root+draw_location_cached, null, color * (alphal * alpha), rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), scale, effects, 1f);
                }
            }
            else if (spriteFont != null)
            {
                if (alpha > 0)
                    spriteBatch.DrawString(spriteFont, str, cord_root + draw_location_cached, color * (alphal * alpha), rotation, new Vector2(size.X / 2f, size.Y / 2f), scale, SpriteEffects.None, 0f);
            }
            foreach (Animatable a in this)
            {
                //Children always positioned from left corner
                //TODO: Add child center positioning
                a.Draw(spriteBatch, new Vector2(cord_root.X + Xposition_left, cord_root.Y + Yposition_top), alphal * alpha);
            }
            drawing.ReleaseMutex();
        }
    }
}
