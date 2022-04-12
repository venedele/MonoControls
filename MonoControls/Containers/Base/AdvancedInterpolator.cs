using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoControls.Containers.Base
{
    public class AdvancedInterpolator
    {
        public const float DONE = float.MaxValue;
        public const float DONE_SET_FINAL = float.MinValue;
        private float starting = 0.0f;
        private float target = 0.0f;
        public float Target
        {
            get { return target; }
            set { setTarget(value);  }
        }
        public float current { get; private set; } = 0.0f;
        public float velocity { get; private set; } = 0.0f;
        public float acceleration { get; private set; } = 0.0f;

        public bool Running
        {
            get { return this.current != target;  }
        }

        Func<double, AdvancedInterpolator, float> driver; 
        public Settables controlledValue;

        private double startTime = -1.0f;
        public double animation_delay_ms = 0.0;

        public enum Settables
        {
            Value = 0,
            Velocity = 1, 
            Acceleration = 2

        }

        public void setTarget(float target, float v_init = 0, float acc_init = 0)
        {
            Reset();
            this.target = target;
            velocity = v_init;
            acceleration = acc_init;
        }

        public void Reset(float value = float.NaN)
        {
            startTime = 0;
            if (float.IsNaN(value))
            {
                value = current;
            }
            this.current = value;
            this.target = value;
            this.starting = value;
            this.velocity = 0;
            this.acceleration = 0;
        }

        public AdvancedInterpolator(Func<double, AdvancedInterpolator, float> driver, Settables controlledValue, float startingvalue = 0, double animation_delay_ms = 0)
        {
            this.driver = driver;
            this.animation_delay_ms = animation_delay_ms;
            this.controlledValue = controlledValue;
            Reset(startingvalue);
        }

        public float Update(GameTime time)
        {
            if (startTime < 0)
            {
                //Comparison should be safe, because an explisit assignment to the same value is performed when resetting. 
                if (Running)
                {
                    startTime = (time.TotalGameTime.TotalMilliseconds);
                }
            }
            else
            {
                double time_m = time.TotalGameTime.TotalMilliseconds - startTime;
                if (time_m <= animation_delay_ms) return this.current;
                float value = driver(time_m, this);
                if (value == DONE || value == DONE_SET_FINAL)
                {
                    Reset(value == DONE_SET_FINAL?target:float.NaN);
                    return current;
                }
                switch (controlledValue)
                {
                    case Settables.Value:
                        this.current = value;
                        break;
                    case Settables.Velocity:
                        this.velocity = value;
                        this.current += this.velocity;
                        break;
                    case Settables.Acceleration:
                        this.acceleration = value;
                        this.velocity += acceleration;
                        this.current += velocity;
                        break;
                }
            }
            return this.current;
            
        }

        public void ForceStart()
        {
            if (!Running) target = current + 0.0000000001f;
        }


        //TODO: Split InterpolatorData to a separate structure, to ensure good access integrity
        //Get Linear transition interpolator
        public static AdvancedInterpolator GetLinear(float time_up, float time_down, float starting_value = 0, double animation_delay_ms = 0)
        {
            return new AdvancedInterpolator(delegate(double time, AdvancedInterpolator sender) {
                if (sender.target - sender.current > 0.0001f) return (sender.target - sender.starting) / time_up;
                else if (sender.target - sender.current < -0.0001f) return -(sender.target - sender.starting) / time_down;
                else return DONE_SET_FINAL;
            }, Settables.Velocity, starting_value, animation_delay_ms);
        }

        public static AdvancedInterpolator GetLinear(float rate_up, float rate_down, float starting_value = 0, bool converge = false, double animation_delay_ms = 0)
        {
            return new AdvancedInterpolator(delegate (double time, AdvancedInterpolator sender) {
                if (sender.target - sender.starting > 0.0f) if (converge && Math.Abs(sender.target - sender.current) < rate_up) return DONE_SET_FINAL; else return rate_up;
                if (converge && Math.Abs(sender.target - sender.current) < rate_down) return DONE_SET_FINAL;  else return rate_down;
            }, Settables.Velocity, starting_value, animation_delay_ms);
        }

        //Final value will be reached after around 5*time_constant (value reset at 5.5*time_constant)
        public static AdvancedInterpolator GetExponential(float time_constant_ms, float starting_value = 0, double animation_delay_ms_l = 0)
        {
            return new AdvancedInterpolator(delegate (double time, AdvancedInterpolator sender)
            {
                if(time > 5.5f*time_constant_ms) return DONE_SET_FINAL;
                //Functions as a low pass filter with a given time constant
                return (sender.target-sender.current)*(time_constant_ms/1000f);
            }, Settables.Velocity, starting_value, animation_delay_ms_l);
        }

        /*public static AdvancedInterpolator GetExponentialTuned()
        {
            return null;
        }*/

    }
}
