﻿using Microsoft.Xna.Framework;
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
        private float starting = 0.0f;
        private float target = 0.0f;
        public float Target
        {
            get { return target; }
        }
        public float current { get; private set; } = 0.0f;
        public float velocity { get; private set; } = 0.0f;
        public float acceleration { get; private set; } = 0.0f;


        Func<double, AdvancedInterpolator, float> driver; 
        public Settables controlledValue;

        private double startTime = -1.0f;

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

        private int direction = 1;

        public void Update(GameTime time)
        {
            if(startTime < 0)
            {
                //Comparison should be safe, because an explisit assignment to the same value is performed when resetting. 
                if (current != target)
                {
                    startTime = (time.TotalGameTime.TotalMilliseconds);
                }
                else return;
                direction = target < starting ? -1 : 1;
            }
            else
            {
                float value = driver(time.TotalGameTime.TotalMilliseconds - startTime, this);
                if(value == DONE)
                {
                    Reset();
                    return;
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
            
        }

    }
}
