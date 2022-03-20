using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ross.Math;

namespace Ross.Animation
{
    public class FABRIKBallConstraint : MonoBehaviour, FABRIKJoint
    {
        public float constraintDegs;

        public Vector3 SolveForward(Transform boneToConstrain, Transform jointBone, Vector3 referenceAxis)
        {
            return SolveConstraint(boneToConstrain, jointBone, referenceAxis);
        }

        public Vector3 SolveBackward(Transform boneToConstrain, Transform jointBone, Vector3 referenceAxis)
        {
            return SolveConstraint(boneToConstrain, jointBone, referenceAxis);
        }

        private Vector3 SolveConstraint(Transform boneToConstrain, Transform jointBone, Vector3 referenceAxis)
        {
            Vector3 boneUV = (boneToConstrain.position - jointBone.position).normalized;

            float angle = Vector3.Angle(referenceAxis, boneUV);

            if (angle > constraintDegs)
            {
                boneUV = MathFunctions.GetUnitVectorConstrainedToAngleDegs(referenceAxis, boneUV, constraintDegs);
            }

            return boneUV;
        }

    }
}
