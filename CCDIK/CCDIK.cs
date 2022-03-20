using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ross.Math;

namespace Ross.Animation
{

    public class CCDIK : MonoBehaviour
    {
        public Transform[] bones;
        public Transform target;
        public Vector3 baseForwardAxis = Vector3.right;
        public int iterations;
        public float error = 0.05f;
        public float minDelta = 0.05f;
        public bool useConstraints;
        public bool useLerp;
        public float trackSpeed;
        float totalChainLength;
        float delta;
        CCDIKJoint[] joints;
        
        void Start()
        {
            Init();
        }

        void Update()
        {
            //DrawDebugLines();
            SolveIK(target.position);
        }

        private void DrawDebugLines()
        {
            for (int i = 0; i < bones.Length - 1; i++)
            {
                Debug.DrawLine(bones[i].position, bones[i + 1].position, Color.red);
            }
        }

        private void Init()
        {
            CalculateChainLength();
        }

        private void CalculateChainLength()
        {
            joints = new CCDIKJoint[bones.Length];
            for (int i = 0; i < bones.Length - 1; i++)
            {
                totalChainLength += Vector3.Distance(bones[i].position, bones[i + 1].position);
                CCDIKJoint joint = bones[i].GetComponent<CCDIKJoint>();
                if(joint != null)
                {                   
                    joints[i] = joint;
                }
            }
        }


        private void SolveIK(Vector3 target)
        {
            Transform endEffector = bones[bones.Length - 1];
            if ((bones[0].position - target).magnitude > totalChainLength)
            {
                Vector3 directionVector = (target - bones[0].position)/(target - bones[0].position).magnitude;
               if(joints[0] != null)
                {
                    Vector3 axis = Vector3.Cross(Vector3.up, directionVector).normalized;
                    directionVector = Vector3.ProjectOnPlane(directionVector, axis).normalized;
                }

                Quaternion rot = Quaternion.FromToRotation(baseForwardAxis, directionVector);
                for (int i = 0; i < bones.Length; i++)
                {
                    bones[i].rotation = rot;
                }
                return;
            }
            int r = 0;
            //dont know if using this or not.
            delta = float.MaxValue;
            CCDIKJoint endEffectorJoint = bones[bones.Length - 2].GetComponent<CCDIKJoint>();
            if(endEffectorJoint != null)
            {
                target = endEffectorJoint.ProjectTargetToEndJoint(target);
            }
            while (r < iterations && (target - endEffector.position).sqrMagnitude > error)
            {
                Vector3 initialEndEffectorPos = endEffector.position;
                //either way works. orientation of bones in the chain is a bit different though.
                //int i = bones.Length - 1; i >= 0; i--
                //int i = 0; i < bones.Length; i++                
                for (int i = 0; i < bones.Length; i++)
                {                    
                    Transform ithLink = bones[i];
                    Vector3 ei = (endEffector.position - ithLink.position).normalized;
                    Vector3 ti = (target - ithLink.position).normalized;
                    Vector3 localRotationAxis;
                    Quaternion rot;
                    CCDIKJoint joint = bones[i].GetComponent<CCDIKJoint>();
                    if (joint != null && useConstraints)
                    {
                        rot = joint.Solve(target, bones, i);
                    }
                    else
                    {
                        localRotationAxis = Vector3.Cross(ti, ei).normalized;
                        float rotation_i = Vector3.SignedAngle(ei, ti, localRotationAxis);
                        rot = Quaternion.AngleAxis(rotation_i, localRotationAxis);
                    }
                    if (useLerp)
                    {
                        //maybe apply a smaller delta to bones closer to the base, so the overall movement of the chain is smoother.
                        ithLink.rotation = Quaternion.RotateTowards(ithLink.rotation, rot * ithLink.rotation, trackSpeed);
                    }
                    else
                    {
                        ithLink.rotation = rot * ithLink.rotation;
                    }
                }
                //dont know if using this or not.
                delta = (endEffector.position - initialEndEffectorPos).magnitude;               
                r++;
            }
        }
    }

}
