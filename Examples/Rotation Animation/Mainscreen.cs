using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoControls.Containers.Additions.Animatables;
using MonoControls.Containers.Base;
using MonoControls.Containers.Additions.Animatables;
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
        Animatable rotation_text;
        Animatable text_back;


        protected void Swap()
        {
            bool started = rotator.Running;
            if (started) rotator.ResetAnimation(false); else rotator.Rotation = float.MaxValue;
            button.First.Value.str = started ? "Start" : "Stop";
        }

        protected override void Resource_Load(ContentManager content_l = null)
        {
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new Color[] { Color.White }) ;

            float screen_height = context.Window.ClientBounds.Height;
            float screen_width = context.Window.ClientBounds.Width;

            rotator = new InterpolAnimatable(texture, new Vector2(screen_width/2f, 100), new Point(100, 100), Color.Gray);
            rotator.setCenterCoord(true);
            rotator.setInterpolators(null, null, AdvancedInterpolator.GetLinear(1f, 1f, false), null);
            text_back = new Animatable(texture, new Vector2(0, 0), new Point(50, 50), Color.Violet);
            rotator.Add(text_back);
            rotation_text = new Animatable(content.Load<SpriteFont>("Font"), "0°", new Vector2(10, 10), Color.White);
            text_back.Add(rotation_text);

            Animatable square = new Animatable(texture, new Vector2(50, 50), new Point(50, 50), Color.Black*0.8f, 0.785f);
            rotator.Add(square);
            square.setCenterCoord(true);
            square.Add(new Animatable(texture, new Vector2(25, 25), new Point(30, 30), Color.Gray, 0.785f));
            square.First.Value.setCenterCoord(true);

            button = new DuplexStateAnimatable(texture, screen_width/2f, screen_height-100, 100, 50, 0.5f*Color.Black);
            button.setCenterCoord(true);
            button.CreateState(null, Color.Green.ToVector4(), 1f);
            button.setInterpolators(AdvancedInterpolator.GetExponentialConst(1f), null, null, AdvancedInterpolator.GetExponentialConst(1.85f));
            button.Add(new Animatable(content.Load<SpriteFont>("Font"), "Start", new Vector2(10, 10), Color.White));
            this.CreateMouseHandler();
            button.AddMouseEvents(delegate (Animatable sender, MouseKeys keys) { if(keys.left)Swap(); return 0; }, delegate (Animatable sender, bool state) { button.SwapStates(false); return 0; }, mouse);
        }

        protected override void Current_Update(GameTime gameTime)
        {
            rotator.Update(gameTime);
            rotation_text.str = Math.Floor(rotator.Rotation*180/Math.PI%360)+ "°";
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Space))
            {
                text_back.X += 0.3f;
            } else if(text_back.X>0) { text_back.X -= 0.3f; }
            button.Update(gameTime);
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
