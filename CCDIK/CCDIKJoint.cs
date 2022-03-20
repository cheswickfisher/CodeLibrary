using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CCDIKJoint
{
    Vector3 ReferenceAxis { get; }
    Quaternion Solve(Vector3 target, Transform[] bones, int i);
    Vector3 ProjectTargetToEndJoint(Vector3 target);
}
