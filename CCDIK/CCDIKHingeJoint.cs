using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ross.Animation
{
    public class CCDIKHingeJoint : MonoBehaviour, CCDIKJoint
    {
        public Vector3 rotationAxis = Vector3.right;
        public Vector3 referenceAxis = Vector3.forward;
        public Vector3 forwardAxis = Vector3.forward;

        [Range(0.0f, 360.0f)]public float cwConstraintDegs;
        [Range(0.0f, 360.0f)]public float acwConstraintDegs;

        private void Awake()
        {
            rotationAxis = rotationAxis.normalized;
        }

        public Vector3 ReferenceAxis
        {
            get { return referenceAxis; }
        }

        public Vector3 ProjectTargetToEndJoint(Vector3 target)
        {
            Vector3 localRotationAxis = transform.rotation * rotationAxis;
            return target - localRotationAxis * Vector3.Dot(localRotationAxis, target - transform.position);
        }


        public Quaternion Solve(Vector3 target, Transform[] bones, int i)
        {          
            Transform ithLink = bones[i];
            Vector3 ei = (bones[bones.Length - 1].position - ithLink.position).normalized;
            Vector3 ti = (target - ithLink.position).normalized;
            Vector3 localHingeAxis;
            Vector3 relativeHingeReferenceAxis;
            if (i > 0)
            {
                localHingeAxis = bones[i - 1].rotation * rotationAxis;
                relativeHingeReferenceAxis = bones[i - 1].rotation * referenceAxis;
            }
            else
            {
                localHingeAxis = /*bones[i].rotation **/ rotationAxis;
                relativeHingeReferenceAxis = referenceAxis;
            }

            Vector3 currentForward = ithLink.rotation * forwardAxis;
            float offRef = Vector3.SignedAngle(relativeHingeReferenceAxis, currentForward, localHingeAxis);
            float rotation_i = Vector3.SignedAngle(ei, ti, localHingeAxis);
            float combined = offRef + rotation_i;
            float finalRotation = rotation_i;
            if(combined > acwConstraintDegs)
            {
                finalRotation = acwConstraintDegs - offRef;
            }

            else if(combined < -cwConstraintDegs)
            {
                finalRotation = -cwConstraintDegs - offRef;
            }
            Quaternion rot = Quaternion.AngleAxis(finalRotation, localHingeAxis);            

            return rot;

        }




    }
}
