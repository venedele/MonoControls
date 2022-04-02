using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoControls.Containers.Base
{
    public class Interpolator
    {
        public static float DONE = float.MaxValue;
        double start = -1;
        private Func<float, float> function;
        double wait_milis; int total_ticks=0; float scale; float multiplier;
        public float changeable { get; private set; }
        private float changeable_start;

        protected bool finished_v = false;
        public bool finished
        {
            get { return finished_v; }
        }
        public bool started
        {
            get { return start != -1; }
        }
        public Interpolator(Func<float, float> function, float changeable = 0, double wait_milis = 0, float multiplier = 1, float scale = 1)
        {
            this.function = function;
            this.wait_milis = wait_milis;
            this.scale = scale;
            this.changeable = changeable;
            this.changeable_start = changeable;
            this.multiplier = multiplier;
        }

        public float Update(GameTime current)
        {
            
            if (start == -1)
                start = current.TotalGameTime.TotalMilliseconds;
            if (finished_v)
                return changeable;
            double total = (current.TotalGameTime.TotalMilliseconds - start);
            if (total > wait_milis)
            {
                float result = function.Invoke((++total_ticks) / scale);
                //Sets and checks the value of finished_v in one line
                if (!(finished_v = (result == DONE)))
                 return changeable = changeable_start + multiplier * result;
            }
            return changeable;
            //TODO: Changes speed with targetfps. Workaround: Use desiredfps/targetfps coefficient in order to stabalise. 
        }

        public void Reset(float changeable = float.NaN)
        {
            total_ticks = 0;

            //Checks whether the `changable`'s value is NaN
            if (!(changeable != changeable))
            {
                this.changeable = changeable;
                this.changeable_start = changeable;
            }
            finished_v = false;
            start = -1;
        }

        public void SetStartingInterval(double wait_milis)
        {
            this.wait_milis = wait_milis;
        }

        public static Interpolator GetPredefined(Predefined interlop, float changeable = 0, double wait_millis = 0, float multiplier = 1, float scale = 1, bool lock_maximum = false)
        {
            Func<float, float> result = null;
            switch (interlop) {
                case Predefined.Accelerate:
                    result = delegate (float x)
                    {
                        float val = x * x/5;
                        if (lock_maximum && val > 1)
                            return DONE;
                        return val;
                    };
                break;
                case Predefined.Decelerate:
                    result = delegate (float x)
                    {
                        float val = -x * x / 5;
                        if (lock_maximum && val < -1)
                            return DONE;
                        return val;
                    };
                break;
                case Predefined.LinearUp:
                    result = delegate (float x)
                    {
                        float val = 1*x;
                        if (lock_maximum && val > 1)
                            return DONE;
                        return val;
                    };
                    break;
                case Predefined.LinearDown:
                    result = delegate (float x)
                    {
                        float val = -1 * x;
                        if (lock_maximum && val < -1)
                            return DONE;
                        return val;
                    };
                    break;
                case Predefined.Constant:
                    result = delegate (float x)
                    {
                        return 1;
                    };
                    break;
                default:
                    throw new FormatException();
            }
            return new Interpolator(result, changeable, wait_millis, multiplier, scale);
        }

        public enum Predefined
        {
            Accelerate,
            Decelerate,
            LinearUp,
            LinearDown,
            Constant,
        }
    }
}
