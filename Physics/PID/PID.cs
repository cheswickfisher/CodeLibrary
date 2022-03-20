using UnityEngine;

namespace Ross.Physics.PID
{
    public class PIDController
    {
        /// <summary>
        /// kp = proportional factor
        /// ki = integral factor
        /// kd = derivative factor
        /// </summary>
        public float kp;
        public float ki;
        public float kd;

        private float output;

        private float prevError;
        private float accumulatedError;

        public PIDController(float kp, float ki, float kd)
        {
            this.kp = kp;
            this.ki = ki;
            this.kd = kd;
        }

        /// <summary>
        /// input is the result of some type of error function. eg: (currentVector - desiredVector).magnitude
        /// output is what you use to modify whatever quantity it is you are trying to stabilize.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public float UpdateController(float error)
        {
            output = kp * error + kd * (error - prevError) / Time.fixedDeltaTime + ki * accumulatedError;
            prevError = error;
            accumulatedError += error * Time.fixedDeltaTime;
            return output;
        }
    }

}
