using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIUnityNavmesh : MonoBehaviour, EnemyAI
{
    private delegate void OnReachEndOfPath();

    private Vector3 target;
    private NavMeshPath path;
    private float elapsed;

    AgentType agent;

    float rotationAmt;
    Vector3 moveVector;
    bool reachedEndOfPath;

    Vector3 prevTarget;
    Vector3 targetPoint;
    Transform targetTransform;
    Vector3 currentCorner;
    float endMargin = 1.0f;
    int currentCornerIndex;
   

    OnReachEndOfPath reachEndOfPathCallback;

    public float _RotationAmt()
    {
        return rotationAmt;
    }

    public Vector3 _MoveVector()
    {
        return moveVector;
    }

    public bool _ReachedEndOfPath()
    {
        return reachedEndOfPath;
    }

    private void Start()
    {
        agent = GetComponent<AgentType>();
        path = new NavMeshPath();
        elapsed = 0.0f;
        prevTarget = target;
        //StartNewRandomPath();
        //reachEndOfPathCallback += StartNewRandomPath;
    }
    public void UpdateInputs(out float rotationAmt)
    {
        MovementUpdate(out moveVector, out rotationAmt);
    }

    private void MovementUpdate(out Vector3 moveVector, out float nextRotationAmt)
    {
        moveVector = Vector3.zero;
        nextRotationAmt = 0.0f;
        Vector3 currentPosition = transform.position;
        elapsed += Time.deltaTime;

        if (targetTransform != null) { UpdateTargetPosition(); }

        CheckCorner();

        ReachedEndOfPath();     

        if (prevTarget != target) { RecalculatePath(GetNewStartPoint()); }

        else if(elapsed > 1.0f)
        {
            RecalculatePath(GetNewStartPoint());
        }

        if (path.corners.Length == 0) { return; }

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        }
        moveVector = (path.corners[currentCornerIndex] - currentPosition).normalized;
        nextRotationAmt = agent._Controls.RotationCorrection((path.corners[currentCornerIndex] - currentPosition).normalized);

    }

    private void RecalculatePath(Vector3 startPoint)
    {
        elapsed -= 1.0f;        
        NavMesh.CalculatePath(startPoint, target, NavMesh.AllAreas, path);
        currentCornerIndex = 1;
    }   

    private void CheckCorner()
    {
        if(currentCornerIndex > path.corners.Length - 1) { return; }
        Vector3 corner = path.corners[currentCornerIndex];
        Vector2 agentPos = new Vector2(agent.transform.position.x, agent.transform.position.z);
        Vector2 offset = agentPos - new Vector2(corner.x, corner.z);
        float sqrLen = offset.sqrMagnitude;
        if (sqrLen < endMargin * endMargin)
        {
            currentCornerIndex++;
        }
    }

    private bool ReachedEndOfPath()
    {
        if(currentCornerIndex >= path.corners.Length)
        {
            path.ClearCorners();
            currentCornerIndex = 0;
            reachedEndOfPath = true;
            return true;
        }
        reachedEndOfPath = false;
        return false;
    }

    private Vector3 GetNewStartPoint()
    {
        Vector3 startPoint;
        if (agent._Controls.Ground(out startPoint, .25f))
        {
            return startPoint;
        }
        else
        {
            if (GetPointNearCenter(transform.position, out startPoint))
            {
                return startPoint;
            }
        }
        return startPoint;
    }

    public void SetNewTarget(Vector3 target)
    {
        this.target = target;
        prevTarget = target;
    }

    public void SetNewTarget(Transform target)
    {
        targetTransform = target;
        prevTarget = target.position;
    }

    public void UpdateTargetPosition()
    {
        prevTarget = target;
        target = targetTransform.position;
    }

    public void ClearTargetTransform()
    {
        targetTransform = null;
    }

    private bool GetRandomPointNearCenter(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = transform.position;
        return false;
    }

    private bool GetPointNearCenter(Vector3 center, out Vector3 result)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(center, out hit, 20.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = transform.position;
        return false;
    }

    private void StartNewRandomPath()
    {
        Vector3 start;
        Vector3 end;

        if(agent._Controls.Ground(out start, .25f))
        {
            if (GetRandomPointNearCenter(start, 50.0f, out end))
            {
                SetNewTarget(end);
                RecalculatePath(start);
            }
        }

        else 
        {
            if (GetPointNearCenter(transform.position, out start))
            {
                if (GetRandomPointNearCenter(start, 50.0f, out end))
                {
                    SetNewTarget(end);
                    RecalculatePath(start);
                }
            }
        }
    }

    public void SetRandomTarget(float range)
    {
        Vector3 start;
        Vector3 end;

        if (agent._Controls.Ground(out start, .25f))
        {
            if (GetRandomPointNearCenter(start, range, out end))
            {
                SetNewTarget(end);
            }
        }

        else
        {
            if (GetPointNearCenter(transform.position, out start))
            {
                if (GetRandomPointNearCenter(start, range, out end))
                {
                    SetNewTarget(end);
                }
            }
        }
    }

    private void GroundCheck()
    {

    }

}
