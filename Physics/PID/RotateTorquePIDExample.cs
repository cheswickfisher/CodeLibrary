using UnityEngine;
using Ross.Physics.PID;

public class RotateTorquePIDExample : MonoBehaviour
{
        public float force;

        [Tooltip("the axis along which you want the object aligned. ex: (0,0,1) aligns along the objects forward axis")]
        public Vector3 alignment = Vector3.forward;

        [Tooltip("the direction you want to move the alignment vector towards")]
        public Vector3 desiredDirection;

        private Rigidbody rb;

        private PIDController PID = new PIDController(5, 0.01f, 1);

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            ///eg: transform.rotation * Vector3.forward = transform.forward
            Vector3 currentDirection = transform.rotation * alignment;
            
            ///Do not normalize rotationAxis. The magnitude of the Cross Product will be greater the further apart transform.forward & desiredForward are. 
            Vector3 rotationAxis = Vector3.Cross(currentDirection, desiredDirection);

            float output = PID.UpdateController((currentDirection - desiredDirection).magnitude);

            rb.AddTorque(rotationAxis * force * output);
        }
}
