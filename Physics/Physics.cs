using UnityEngine;

namespace Ross.Physics
{
    public static class Physics
    {
        /// <summary>
        /// supplies the rotational force vector you can use to rotate a rigidbody to point in a certain direction with AddTorque command (ForceMode.Force). 
        /// instead of subtracting out the angular velocity * damping you can also set Angular Drag in the rigidbody component.
        /// </summary>
        /// <param name="currentVector"></param>
        /// <param name="targetVector"></param>
        /// <param name="rotationForce"></param>
        /// <param name="rb"></param>
        /// <param name="damping"></param>
        /// <returns></returns>
        public static Vector3 RotateTorque(Vector3 currentVector, Vector3 targetVector, float rotationForce, Rigidbody rb, float damping)
        {
            Vector3 rotationAxis = Vector3.Cross(currentVector, targetVector);
            return (rotationAxis * rotationForce + -rb.angularVelocity * damping);
        }
    }
}
