using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoControls.Containers.Base
{
    ///<summary>
    ///A class used for Interpolating float values, allowing for differential equation function. 
    ///</summary>
    public class AdvancedInterpolator
    {
        /// <summary>
        /// Value describing the end of interpolation. 
        /// </summary>
        /// <remarks>
        /// When this value is returned by the driver function, the target is snapped to the current value. 
        /// </remarks>
        public const float DONE = float.MaxValue;

        /// <summary>
        /// Value describing the end of interpolation.  
        /// </summary>
        /// <remarks>
        /// When this value is returned by the driver function, the current value is snapped to the target. 
        /// </remarks>
        public const float DONE_SET_FINAL = float.MinValue;

        /// <summary>
        /// Contains the starting value of the current interpolation.
        /// </summary>
        public float starting { get; private set; } = 0.0f;


        public float target { get; private set; } = 0.0f;
        /// <summary>
        /// Represents the current target value
        /// </summary>
        public float Target
        {
            get { return target; }
            set { setTarget(value);  }
        }

        /// <summary>
        /// Contains the current interpolated value
        /// </summary>
        public float current { get; private set; } = 0.0f;
        /// <summary>
        /// Contains the current first order rate of change
        /// </summary>
        public float velocity { get; private set; } = 0.0f;
        /// <summary>
        /// Contains the current second order rate of change
        /// </summary>
        public float acceleration { get; private set; } = 0.0f;

        /// <summary>
        /// Returns whether there is an interpolation running.
        /// </summary>
        public bool Running
        {
            get { return this.current != target;  }
        }

        Func<double, AdvancedInterpolator, float> driver; 

        /// <summary>
        /// Defines which order derivative is controlled by a function.
        /// </summary>
        public Settables controlledValue;

        private double startTime = -1.0f;

        /// <summary>
        /// Represents time interval between the previous two updates (seconds)
        /// </summary>
        public double timestep { get; private set; } = 0.0f;

        /// <summary>
        /// Defines the animation intial delay. 
        /// </summary>
        public double animation_delay_ms = 0.0;

        /// <summary>
        /// All supported function driven derivatives.
        /// </summary>
        public enum Settables
        {
            Value = 0,
            Velocity = 1, 
            Acceleration = 2
        }

        /// <summary>
        /// Sets the target value for the current/new interpolation.
        /// </summary>
        /// <param name="target">Target value</param>
        /// <param name="v_init">Initial value for first order derivative</param>
        /// <param name="acc_init">Inital value for second order derivative</param>
        public void setTarget(float target, float v_init = 0, float acc_init = 0)
        {
            if (this.target == target) return;
            Reset();
            this.target = target;
            velocity = v_init;
            acceleration = acc_init;
        }

        /// <summary>
        /// Stop current interpolation
        /// </summary>
        /// <param name="value">End Value for the controller variable. If left empty, the current value is set </param>
        public void Reset(float value = float.NaN)
        {
            startTime = -1;
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

        /// <summary>
        /// Inialises a new instance of the AdvancedInterpolator class
        /// </summary>
        /// <param name="driver">
        ///     <para>A function driving the interpolation.</para>
        ///     <para>The function should return the next value fot the interpolation</para>
        ///     <para>Parameters:</para>
        ///     <para>double time: Time since start of interpolation</para>
        ///     <para>AdvancedInterpolator sender: instance of interpolator</para>
        /// </param>
        /// <param name="controlledValue">Derivative controlled by the driver function</param>
        /// <param name="startingvalue">Starting value for the Interpolated variable</param>
        /// <param name="animation_delay_ms">Initial intepolation delay</param>
        public AdvancedInterpolator(Func<double, AdvancedInterpolator, float> driver, Settables controlledValue, float startingvalue = 0, double animation_delay_ms = 0)
        {
            this.driver = driver;
            this.animation_delay_ms = animation_delay_ms;
            this.controlledValue = controlledValue;
            Reset(startingvalue);
        }


        /// <summary>
        /// Updates and returns interpolated value. 
        /// </summary>
        /// <param name="time">Current game time</param>
        /// <returns>Updated interpolated value</returns>
        public float Update(GameTime time)
        {
            if (startTime <= 0)
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
                timestep = time.ElapsedGameTime.TotalSeconds;
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
                        this.current += this.velocity*(float)timestep;
                        break;
                    case Settables.Acceleration:
                        this.acceleration = value;
                        this.velocity += this.acceleration*(float)timestep;
                        this.current += this.velocity*(float)timestep;
                        break;
                }
            }
            return this.current;
            
        }

        /// <summary>
        /// Starts an interpolation. Useful for driver function which don't follow a value. 
        /// </summary>
        /// <remarks>
        /// Works by adding a very small difference to the target. 
        /// </remarks>
        public void ForceStart(int direction = 1)
        {
            if (!Running) target = current + Math.Sign(direction)*0.0000000001f;
        }


        //TODO: Split InterpolatorData to a separate structure, to ensure good access integrity
        //Get Linear transition interpolator
        /// <summary>
        /// Builds a Linear constant time interpolator. 
        /// </summary>
        /// <param name="time_up">Time ascending changes take in miliseconds</param>
        /// <param name="time_down">Time descensing changes take in miliseconds</param>
        /// <param name="starting_value">Starting value for interpolated variable</param>
        /// <param name="animation_delay_ms">Initial intepolation delay</param>
        /// <returns>Linear AdvancedInterpolator tuned by parameters</returns>
        public static AdvancedInterpolator GetLinear(float time_up, float time_down, float starting_value = 0, double animation_delay_ms = 0)
        {
            return new AdvancedInterpolator(delegate(double time, AdvancedInterpolator sender) {
                if (sender.target - sender.current > 0.0001f) return (sender.target - sender.starting) / (time_up /1000f);
                else if (sender.target - sender.current < -0.0001f) return -(sender.target - sender.starting) / (time_down/1000f);
                else return DONE_SET_FINAL;
            }, Settables.Velocity, starting_value, animation_delay_ms);
        }

        /// <summary>
        /// Builds a Linear constant rate interpolator.
        /// </summary>
        /// <param name="rate_up">Rate for ascending changes in units/second</param>
        /// <param name="rate_down">Rate for descending changes in units/second</param>
        /// <param name="converge">Determines whether interpolator should stop at target value</param>
        /// <param name="starting_value">Starting value for interpolated variable</param>
        /// <param name="animation_delay_ms">Initial intepolation delay</param>
        /// <returns>Linear AdvancedInterpolator tuned by parameters</returns>
        public static AdvancedInterpolator GetLinear(float rate_up, float rate_down, bool converge, float starting_value = 0, double animation_delay_ms = 0)
        {
            return new AdvancedInterpolator(delegate (double time, AdvancedInterpolator sender) {
                if (sender.target - sender.starting > 0.0f) { 
                    if (converge && Math.Abs(sender.target - sender.current) < rate_up*sender.timestep) return DONE_SET_FINAL; 
                    else return rate_up; 
                }
                if (converge && Math.Abs(sender.target - sender.current) < rate_down * sender.timestep) return DONE_SET_FINAL;  
                else return rate_down;
            }, Settables.Velocity, starting_value, animation_delay_ms);
        }

        /// <summary>
        /// Builds a Exponential interpolator with defined time constant. (Value reached after 5.5 time constants)
        /// </summary>
        /// <param name="time_constant_ms">Time constant in miliseconds</param>
        /// <param name="starting_value">Starting value for interpolated variable</param>
        /// <param name="animation_delay_ms_l">Initial intepolation delay</param>
        /// <returns>Exponential AdvancedInterpolator tuned by parameters</returns>
        public static AdvancedInterpolator GetExponential(float time_constant_ms, float starting_value = 0, double animation_delay_ms_l = 0)
        {
            return new AdvancedInterpolator(delegate (double time, AdvancedInterpolator sender)
            {
                if(time > 5.5f*time_constant_ms) return DONE_SET_FINAL;
                //Functions as a low pass filter with a given time constant
                return (sender.target-sender.current)/(time_constant_ms/1000f);
            }, Settables.Velocity, starting_value, animation_delay_ms_l);
        }

        /// <summary>
        /// Builds a Exponential interpolator with defined initial rate
        /// </summary>
        /// <param name="rate_per_s">Inital rate</param>
        /// <param name="starting_value">Starting value for interpolated variable</param>
        /// <param name="animation_delay_ms_l">Exponential AdvancedInterpolator tuned by parameters</param>
        /// <returns>Exponential AdvancedInterpolator tuned by parameters</returns>
        public static AdvancedInterpolator GetExponentialConst(float rate_per_s, float starting_value = 0, double animation_delay_ms_l = 0)
        {
            return new AdvancedInterpolator(delegate (double time, AdvancedInterpolator sender)
            {
                double time_constant_ms = (sender.target - sender.starting) / rate_per_s;
                if (time > 5.5f * 1000f * time_constant_ms) return DONE_SET_FINAL;
                //Functions as a low pass filter with a given time constant
                return ((sender.target - sender.current) / (float)(time_constant_ms));
            }, Settables.Velocity, starting_value, animation_delay_ms_l);
        }

        /*public static AdvancedInterpolator GetExponentialTuned()
        {
            return null;
        }*/

    }
}
