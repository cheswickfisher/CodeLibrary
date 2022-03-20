using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ross.Animation
{
    public class FABRIK : MonoBehaviour
    {
        public Transform[] bones;
        public Transform target;
        public float error = 0.05f;
        public int iterations = 6;
        public float lerpTrackSpeed = 1.0f;
        public bool useLerp;
        public Vector3 baseBoneReferenceAxis = Vector3.forward;
        public bool drawDebugLines;
        private float[] lengths;
        private float totalChainLength;
        private float dMax;
        private float minDelta = 0.01f;
        private float delta;
        private float rotDegs;
        private float rotDelta = 0.1f;
        private Vector3 basePos;

        void Start()
        {
            Init();
        }

        void Update()
        {
            if (drawDebugLines)
            {
                DrawDebugLines();
            }

            SolveIK(target.position);
        }

        private void Init()
        {
            if (bones.Length <= 1) { return; }
            delta = float.MaxValue;
            basePos = bones[0].position;
            lengths = new float[bones.Length - 1];
            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = Vector3.Distance(bones[i].position, bones[i + 1].position);
                totalChainLength += lengths[i];
                if (lengths[i] > dMax) { dMax = lengths[i]; }
            }
        }

        private void DrawDebugLines()
        {
            for (int i = 0; i < bones.Length - 1; i++)
            {
                Debug.DrawLine(bones[i].position, bones[i + 1].position, Color.red);
            }
        }

        private void SolveIK(Vector3 target)
        {
            Vector3 baseToTarget = target - bones[0].position;
            float distanceToTarget = baseToTarget.magnitude;

            if (distanceToTarget > totalChainLength)
            {
                Vector3 directionToGoal = (target - bones[0].position).normalized;
                Vector3 currentPos = bones[0].position;
                for (int i = 0; i < bones.Length - 1; i++)
                {
                    bones[i].position = currentPos;
                    currentPos = bones[i].position + directionToGoal * lengths[i];
                }
                bones[bones.Length - 1].position = currentPos;
            }

            else
            {
                Vector3 effectorToTarget = bones[bones.Length - 1].position - target;
                float endEffectorToTargetDistance = effectorToTarget.sqrMagnitude;
                if (endEffectorToTargetDistance <= error) { rotDegs = 0.0f; }
                int x = 0;
                while ((bones[bones.Length - 1].position - target).sqrMagnitude > error && x < iterations)
                {
                    Vector3 endEffectorInitialPosition = bones[bones.Length - 1].position;

                    Forward(target);
                    Backward(target, x);
                    if (delta < minDelta && x == 0)
                    {
                        if (useLerp)
                        {
                            bones[0].rotation = Quaternion.Slerp(bones[0].rotation, DeadlockProcedure(target, bones[0], bones[1]), lerpTrackSpeed * Time.deltaTime);
                        }
                        else
                        {
                            bones[0].rotation = Quaternion.AngleAxis(1.0f, Vector3.forward) * bones[0].rotation;
                        }
                    }
                    delta = (bones[bones.Length - 1].position - endEffectorInitialPosition).magnitude;

                    x++;
                }
            }
        }

        private void Forward(Vector3 target)
        {
            for (int i = bones.Length - 1; i >= 0; i--)
            {
                if (i != bones.Length - 1)
                {
                    Transform outerBone = bones[i + 1];
                    Transform currentBone = bones[i];

                    Vector3 directionUV = (currentBone.position - outerBone.position).normalized;
                    FABRIKJoint joint = outerBone.GetComponent<FABRIKJoint>();
                    if (joint != null && i + 1 != bones.Length - 1)
                    {
                        directionUV = joint.SolveForward(currentBone, outerBone, (outerBone.position - bones[i + 2].position).normalized);
                    }
                    if (useLerp)
                    {
                        currentBone.position = Vector3.MoveTowards(currentBone.position, outerBone.position + directionUV * lengths[i], lerpTrackSpeed * Time.deltaTime);
                    }
                    else
                    {
                        currentBone.position = outerBone.position + directionUV * lengths[i];
                    }
                }

                else
                {
                    Transform endBone = bones[bones.Length - 1];
                    Vector3 directionUV = (target - endBone.position).normalized;
                    if (useLerp)
                    {
                        endBone.position = Vector3.MoveTowards(endBone.position, target, lerpTrackSpeed * Time.deltaTime);
                    }
                    else
                    {
                        endBone.position = target;
                    }
                }
            }
        }

        private void Backward(Vector3 target, int x)
        {
            Vector3 prevStartPos = bones[0].position;

            for (int i = 0; i < bones.Length; i++)
            {

                if (i == 0)
                {
                    Transform baseBone = bones[0];
                    baseBone.position = basePos;
                }

                else
                {
                    Transform currentBone = bones[i];
                    Transform innerBone = bones[i - 1];



                    Vector3 directionUV = (currentBone.position - innerBone.position).normalized;
                    FABRIKJoint joint = innerBone.GetComponent<FABRIKJoint>();

                    if (joint != null)
                    {
                        if (i - 1 > 0)
                        {
                            directionUV = joint.SolveBackward(currentBone, innerBone, (innerBone.position - bones[i - 2].position).normalized);
                        }

                        else if (i - 1 == 0)
                        {
                            directionUV = joint.SolveBackward(currentBone, innerBone, baseBoneReferenceAxis);
                        }
                    }

                    if (useLerp)
                    {
                        currentBone.position = Vector3.MoveTowards(currentBone.position, innerBone.position + directionUV * lengths[i - 1], lerpTrackSpeed * Time.deltaTime);
                    }
                    else
                    {
                        currentBone.position = innerBone.position + directionUV * lengths[i - 1];
                    }

                }
            }
        }

        private Quaternion DeadlockProcedure(Vector3 target, Transform chainBase, Transform secondChainBone)
        {
            Vector3 directionToTargetUV = (target - bones[0].position).normalized * -1f;
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, directionToTargetUV).normalized;
            Vector3 directionUV = (secondChainBone.position - chainBase.position).normalized;
            float angle = Vector3.SignedAngle(directionUV, directionToTargetUV, rotationAxis);
            int sign;
            rotDegs += rotDelta;

            if (angle > 0)
            {
                sign = 1;
            }
            else
            {
                sign = -1;
            }

            return Quaternion.AngleAxis(rotDegs, rotationAxis) * chainBase.rotation;

        }
    }
}
