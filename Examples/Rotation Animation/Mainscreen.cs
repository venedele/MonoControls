using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoControls.Containers.Additions.Animatables;
using MonoControls.Containers.Base;
using MonoControls.Containers.Helpers.Animatables;
using MonoControls.Inputs.Mouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotationAnimation
{
    internal class Mainscreen : Screen
    {
        public Mainscreen(Game context) : base(context) { }

        DuplexStateAnimatable button;

        InterpolAnimatable rotator; 

        protected void Swap()
        {
            bool started = rotator.started;
            if (started) rotator.ResetAnimation(false); else rotator.StartAnimation();
            button.First.Value.str = started ? "Start" : "Stop";
        }

        protected override void Resource_Load(ContentManager content_l = null)
        {
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new Color[] { Color.White }) ;

            float screen_height = context.Window.ClientBounds.Height;
            float screen_width = context.Window.ClientBounds.Width;

            rotator = new InterpolAnimatable(texture, new Vector2(screen_width/2f, 100), new Point(100, 100), Color.Gray);
            rotator.setCentralCoords(true);
            rotator.setInterpolators(null, null, Interpolator.GetPredefined(Interpolator.Predefined.LinearUp, 0, 10, 0.01f));
            rotator.Add(new Animatable(texture, new Vector2(0, 0), new Point(50, 50), Color.Violet));
            rotator.First.Value.Add(new Animatable(content.Load<SpriteFont>("Font"), "0°", new Vector2(10, 10), Color.White));

            button = new DuplexStateAnimatable(texture, screen_width/2f, screen_height-100, 100, 50, Color.Black);
            button.setCentralCoords(true);
            button.CreateState(null, Color.Green.ToVector4(), 0.8f);
            button.Add(new Animatable(content.Load<SpriteFont>("Font"), "Start", new Vector2(10, 10), Color.White));
            this.CreateMouseHandler();
            button.AddMouseEvents(delegate (Animatable sender, MouseKeys keys) { if(keys.left)Swap(); return 0; }, delegate (Animatable sender, bool state) { button.SwapStates(false); return 0; }, mouse);
        }

        protected override void Current_Update(GameTime gameTime)
        {
            rotator.Update(gameTime);
            rotator.First.Value.First.Value.str = Math.Floor(rotator.Rotation*180/Math.PI%360)+ "°";
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Space))
            {
                rotator.First.Value.X += 0.3f;
            } else if(rotator.First.Value.X>0) { rotator.First.Value.X -= 0.3f; }
        }

        protected override void Current_Draw(SpriteBatch spriteBatch)
        {
            button.Draw(spriteBatch, customroot);
            rotator.Draw(spriteBatch, customroot);
        }

        protected override void Dispose()
        {
            
        }

    }
}
