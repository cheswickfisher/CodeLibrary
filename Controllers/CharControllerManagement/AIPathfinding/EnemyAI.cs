using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface EnemyAI 
{
    float _RotationAmt();
    Vector3 _MoveVector();
    bool _ReachedEndOfPath();
    void UpdateInputs(out float rotationAmt);
    void SetNewTarget(Vector3 target);
    void SetNewTarget(Transform target);
    void UpdateTargetPosition();
    void ClearTargetTransform();
    void SetRandomTarget(float range);
}
