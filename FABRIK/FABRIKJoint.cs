using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ross.Animation
{
    public interface FABRIKJoint
    {
        Vector3 SolveForward(Transform boneToConstraint, Transform jointBone, Vector3 referenceAxis);
        Vector3 SolveBackward(Transform boneToConstraint, Transform jointBone, Vector3 referenceAxis);

    }
}
