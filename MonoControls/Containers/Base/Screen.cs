/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;*/
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoControls.Inputs.Mouse;

namespace MonoControls.Containers.Base
{
    public abstract class Screen
    {
        public bool active = true;

        public Screen parent { private set; get; } = null;

        protected Game context;

        protected Microsoft.Xna.Framework.Content.ContentManager content
        {
            get { return context.Content;  }
        }
        public GraphicsDevice graphicsDevice
        {
            get { return context.GraphicsDevice; }
        }


        //If Context is changed on an initialised object, the object will reaload its resources
        public Game Context
        {
            get { return context; }
            set { if (value != context)
                { 
                  context = value;
                  if (initialised) {
                        if (context.GraphicsDevice != null)
                        {
                            //Load is not called, as that will try and reload resources on every step of the screen chain
                            Resource_Load();
                        }
                        else initialised = false;
                  }
                    if (child != null)
                        child.Context = context;
                }
            }
        }
        public void Load()
        {
            initialised = true;
            Resource_Load(context.Content);
            if (child != null) child.Load();
        }

        public Screen(Game context)
        {
            this.Context = context;
        }

        private bool custom_root_compliable_v = true;
        public bool custom_root_compliable
        {
            get { return custom_root_compliable_v; }
            set { custom_root_compliable_v = value; OnCustomVectorChange(customroot_v, custom_root_compliable_v); }
        }
        private Vector2 customroot_v = new Vector2(0, 0);

        
        public Vector2 customroot
        {
            get { return custom_root_compliable?customroot_v:Vector2.Zero; }
            //TODO: Consider whether each element in the child stack should share a customroot, or each element should have its own independent one
            set { if(nested != null?nested.custom_root_compliable_v:false)nested.customroot = value; customroot_v = value; OnCustomVectorChange(customroot_v, custom_root_compliable); }
        }

        //Returns customroot without caring whether its used or not
        public Vector2 getCustomRootInd()
        {
            return customroot_v;
        }

        protected virtual void OnCustomVectorChange(Vector2 CustomVector, bool custom_vector_compliable)
        { }

        protected Mouse_Handler mouse = null;
        public bool mouseBlocked { get; private set; } = false;
        public void CreateMouseHandler()
        {
            mouse = new Mouse_Handler();
        }

        public virtual Screen setMouseState(bool blockMouse)
        {
            mouseBlocked = blockMouse;
            return this;
        }

        public bool automaticallyLoadNext = true;

        protected bool initialised = false;
        public bool Initialised
        {
            set { if (initialised ^ (initialised = value)) { if(this.child != null) this.child.Initialised = initialised;  if (initialised) { Resource_Load(content); } else {  Dispose(); } }  }
            get { return initialised; }
        }

        public bool paused = false;
  
        protected Screen child = null;
        public Screen nested
        {
            get { return child; }
            set { if(child!=null)child.parent = null; child = value; if (child != null) { child.parent = this; child.Context = context;  if (automaticallyLoadNext && !child.initialised && this.initialised) child.Load(); } }
        }
        public Screen Detach()
        {
            parent = null;
            return this;
        }
        public void Draw()
        {
            SpriteBatch spriteBatch = (SpriteBatch)context.Services.GetService(typeof(SpriteBatch));
            if (child!=null)child.Draw();
            Current_Draw(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            Update(gameTime, false);
        }

        public void Update(GameTime gameTime, bool mouseblocked)
        {
            if (!paused)
            {
                if (!(mouseBlocked||mouseblocked || mouse == null))
                    mouse.Update();
                if (child != null) child.Update(gameTime, mouseblocked||mouseBlocked);
                Current_Update(gameTime);
            }
        }

        protected abstract void Resource_Load(Microsoft.Xna.Framework.Content.ContentManager content_l = null);
        protected abstract void Current_Update(GameTime gameTime);
        protected abstract void Current_Draw(SpriteBatch spriteBatch);
        protected abstract void Dispose();

    }
}
