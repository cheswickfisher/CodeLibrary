using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.Util;
using System;

public class EnemyAIAStarPathfindingProject : AgentAIBase, IAstarAI, EnemyAI
{
    AgentType agent;
    public float slowdownDistance = 0.6F;
    public float pickNextWaypointDist = 2;
    public float endReachedDistance = 0.2F;
    public bool slowWhenNotFacingTarget = true;
    public CloseToDestinationMode whenCloseToDestination = CloseToDestinationMode.Stop;
    public bool constrainInsideGraph = false;
    protected Path path;
    public Transform interpolatorTransform;
    public Transform destinationTransform;
    protected PathInterpolator interpolator = new PathInterpolator();

    float rotationAmt;
    Vector3 moveVector;

    private void Awake()
    {
        agent = GetComponent<AgentType>();
    }
    protected override void Start()
    {
        base.Start();
    }

    protected void FixedUpdate()
    {
       // UpdateInputs();
       // agentType.UpdateStateMachines();
       // controls.ResetCollisionInfo();
    }

    public override void DoMovement()
    {
        if (!waitingForPathCalculation && canMove)
        {
            interpolatorTransform.position = interpolator.position;
            destinationTransform.position = destination;
            agent._Controls.DoMovement();
        }
    }

    public void UpdateInputs(out float rotationAmt)
    {
        if (!(rigid == null) && canMove)
        {
            Vector3 nextPosition;
            Quaternion nextRotation;
            MovementUpdate(Time.fixedDeltaTime, out nextPosition, out nextRotation);
            rotationAmt = this.rotationAmt;
        }
        else
        {
            rotationAmt = 0;
        }
    }

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


    #region Pathfinding


    public override void Teleport(Vector3 newPosition, bool clearPath = true)
    {
        reachedEndOfPath = false;
        if (clearPath) ClearPath();
        prevPosition1 = prevPosition2 = simulatedPosition = newPosition;
        if (updatePosition) tr.position = newPosition;
        if (clearPath) SearchPath();
    }

    public float remainingDistance
    {
        get
        {
            return interpolator.valid ? interpolator.remainingDistance + movementPlane.ToPlane(interpolator.position - position).magnitude : float.PositiveInfinity;
        }
    }

    public bool reachedDestination
    {
        get
        {
            if (!reachedEndOfPath) return false;
            if (remainingDistance + movementPlane.ToPlane(destination - interpolator.endPoint).magnitude > endReachedDistance) return false;

            // Don't do height checks in 2D mode
            if (orientation != OrientationMode.YAxisForward)
            {
                // Check if the destination is above the head of the character or far below the feet of it
                float yDifference;
                movementPlane.ToPlane(destination - position, out yDifference);
                var h = tr.localScale.y * height;
                if (yDifference > h || yDifference < -h * 0.5) return false;
            }

            return true;
        }
    }
    public bool reachedEndOfPath { get; protected set; }
    public bool hasPath
    {
        get
        {
            return interpolator.valid;
        }
    }
    public bool pathPending
    {
        get
        {
            return waitingForPathCalculation;
        }
    }
    public Vector3 steeringTarget
    {
        get
        {
            return interpolator.valid ? interpolator.position : position;
        }
    }
    float IAstarAI.radius { get { return radius; } set { radius = value; } }

    float IAstarAI.height { get { return height; } set { height = value; } }

    float IAstarAI.maxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }

    bool IAstarAI.canSearch { get { return canSearch; } set { canSearch = value; } }

    bool IAstarAI.canMove { get { return canMove; } set { canMove = value; } }

    public void GetRemainingPath(List<Vector3> buffer, out bool stale)
    {
        buffer.Clear();
        buffer.Add(position);
        if (!interpolator.valid)
        {
            stale = true;
            return;
        }

        stale = false;
        interpolator.GetRemainingPath(buffer);
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        // Release current path so that it can be pooled
        if (path != null) path.Release(this);
        path = null;
        interpolator.SetPath(null);
    }

    public virtual void OnTargetReached()
    {
        destination = transform.position;
        reachedEndOfPath = false;
        lastRepath = float.NegativeInfinity;
        ClearPath();
        SearchPath();
    }
    protected override void OnPathComplete(Path newPath)
    {
        ABPath p = newPath as ABPath;

        if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

        waitingForPathCalculation = false;

        // Increase the reference count on the new path.
        // This is used for object pooling to reduce allocations.
        p.Claim(this);

        // Path couldn't be calculated of some reason.
        // More info in p.errorLog (debug string)
        if (p.error)
        {
            p.Release(this);
            return;
        }

        // Release the previous path.
        if (path != null) path.Release(this);

        // Replace the old path
        path = p;

        // Make sure the path contains at least 2 points
        if (path.vectorPath.Count == 1) path.vectorPath.Add(path.vectorPath[0]);
        interpolator.SetPath(path.vectorPath);

        var graph = path.path.Count > 0 ? AstarData.GetGraph(path.path[0]) as ITransformedGraph : null;
        movementPlane = graph != null ? graph.transform : (orientation == OrientationMode.YAxisForward ? new GraphTransform(Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 270, 90), Vector3.one)) : GraphTransform.identityTransform);

        // Reset some variables
        reachedEndOfPath = false;
        canMove = true;
        // Simulate movement from the point where the path was requested
        // to where we are right now. This reduces the risk that the agent
        // gets confused because the first point in the path is far away
        // from the current position (possibly behind it which could cause
        // the agent to turn around, and that looks pretty bad).
        interpolator.MoveToLocallyClosestPoint((GetFeetPosition() + p.originalStartPoint) * 0.5f);
        interpolator.MoveToLocallyClosestPoint(GetFeetPosition());

        // Update which point we are moving towards.
        // Note that we need to do this here because otherwise the remainingDistance field might be incorrect for 1 frame.
        // (due to interpolator.remainingDistance being incorrect).
        interpolator.MoveToCircleIntersection2D(position, pickNextWaypointDist, movementPlane);

        var distanceToEnd = remainingDistance;
        if (distanceToEnd <= endReachedDistance)
        {
            reachedEndOfPath = true;
            OnTargetReached();
        }
    }
    protected override void ClearPath()
    {
        pathLock = false;
        canMove = false;
        CancelCurrentPathRequest();
        interpolator.SetPath(null);
        reachedEndOfPath = false;
    }

    protected override void MovementUpdateInternal(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
    {
        nextRotation = Quaternion.identity;
        // float currentAcceleration = maxAcceleration;

        // If negative, calculate the acceleration from the max speed
        // if (currentAcceleration < 0) currentAcceleration *= -maxSpeed;

        if (updatePosition)
        {
            // Get our current position. We read from transform.position as few times as possible as it is relatively slow
            // (at least compared to a local variable)
            simulatedPosition = tr.position;
        }
        // if (updateRotation) simulatedRotation = tr.rotation;

        var currentPosition = simulatedPosition;

        // Update which point we are moving towards
        interpolator.MoveToCircleIntersection2D(currentPosition, pickNextWaypointDist, movementPlane);
        var dir = movementPlane.ToPlane(steeringTarget - currentPosition);

        // Calculate the distance to the end of the path
        float distanceToEnd = dir.magnitude + Mathf.Max(0, interpolator.remainingDistance);

        // Check if we have reached the target
        var prevTargetReached = reachedEndOfPath;
        reachedEndOfPath = distanceToEnd <= endReachedDistance && interpolator.valid;
        if (!prevTargetReached && reachedEndOfPath) OnTargetReached();
        float slowdown;

        // Normalized direction of where the agent is looking
        // var forwards = movementPlane.ToPlane(simulatedRotation * (orientation == OrientationMode.YAxisForward ? Vector3.up : Vector3.forward));
        nextPosition = (steeringTarget - currentPosition).normalized;

        moveVector = nextPosition;
        rotationAmt = agent._Controls.RotationCorrection((steeringTarget - currentPosition).normalized);
        // Check if we have a valid path to follow and some other script has not stopped the character
        if (interpolator.valid && !isStopped)
        {
            // How fast to move depending on the distance to the destination.
            // Move slower as the character gets closer to the destination.
            // This is always a value between 0 and 1.
            slowdown = distanceToEnd < slowdownDistance ? Mathf.Sqrt(distanceToEnd / slowdownDistance) : 1;

            /* if (reachedEndOfPath && whenCloseToDestination == CloseToDestinationMode.Stop)
             {
                 // Slow down as quickly as possible
                 //velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
             }
             else
             {
                 //velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(dir, dir.normalized * maxSpeed, velocity2D, currentAcceleration, rotationSpeed, maxSpeed, forwards) * deltaTime;
             }*/
        }
        else
        {
            slowdown = 1;
            // Slow down as quickly as possible
            //velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
        }

        // velocity2D = MovementUtilities.ClampVelocity(velocity2D, maxSpeed, slowdown, slowWhenNotFacingTarget && enableRotation, forwards);

        //ApplyGravity(deltaTime);


        // Set how much the agent wants to move during this frame
        var delta2D = lastDeltaPosition = CalculateDeltaToMoveThisFrame(movementPlane.ToPlane(currentPosition), distanceToEnd, deltaTime);
        // nextPosition = currentPosition + movementPlane.ToWorld(delta2D, verticalVelocity * lastDeltaTime);
        //CalculateNextRotation(slowdown, out nextRotation);
    }
    /*protected virtual void CalculateNextRotation(float slowdown, out Quaternion nextRotation)
    {
        if (lastDeltaTime > 0.00001f && enableRotation)
        {
            Vector2 desiredRotationDirection;
            desiredRotationDirection = velocity2D;

            // Rotate towards the direction we are moving in.
            // Don't rotate when we are very close to the target.
           // var currentRotationSpeed = rotationSpeed * Mathf.Max(0, (slowdown - 0.3f) / 0.7f);
           // nextRotation = SimulateRotationTowards(desiredRotationDirection, currentRotationSpeed * lastDeltaTime);
        }
        else
        {
            nextRotation = rotation;
        }
    }*/
    static NNConstraint cachedNNConstraint = NNConstraint.Default;
    protected override Vector3 ClampToNavmesh(Vector3 position, out bool positionChanged)
    {
        if (constrainInsideGraph)
        {
            cachedNNConstraint.tags = seeker.traversableTags;
            cachedNNConstraint.graphMask = seeker.graphMask;
            cachedNNConstraint.distanceXZ = true;
            var clampedPosition = AstarPath.active.GetNearest(position, cachedNNConstraint).position;

            // We cannot simply check for equality because some precision may be lost
            // if any coordinate transformations are used.
            var difference = movementPlane.ToPlane(clampedPosition - position);
            float sqrDifference = difference.sqrMagnitude;
            if (sqrDifference > 0.001f * 0.001f)
            {
                // The agent was outside the navmesh. Remove that component of the velocity
                // so that the velocity only goes along the direction of the wall, not into it
                velocity2D -= difference * Vector2.Dot(difference, velocity2D) / sqrDifference;

                positionChanged = true;
                // Return the new position, but ignore any changes in the y coordinate from the ClampToNavmesh method as the y coordinates in the navmesh are rarely very accurate
                return position + movementPlane.ToWorld(difference);
            }
        }

        positionChanged = false;
        return position;
    }

    public void SetNewTarget(Vector3 target)
    {
        throw new NotImplementedException();
    }

    public void SetNewTarget(Transform target)
    {
        throw new NotImplementedException();
    }

    public void UpdateTargetPosition()
    {
        throw new NotImplementedException();
    }

    public void ClearTargetTransform()
    {
        throw new NotImplementedException();
    }

    public void SetRandomTarget(float range)
    {
        throw new NotImplementedException();
    }

    #endregion
}
