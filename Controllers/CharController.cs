using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RossMaths;
using System;

public abstract class CharController : MonoBehaviour   
   
{
    [SerializeField]
    Animator ac;
    public Animator _AC { get { return ac; } }
    [SerializeField]
    protected AgentType agent;
    [SerializeField]
    float airDuration = .1f;
    float lastGroundedTime;
    protected Collider col;
    public Collider _Col { get { return col; } }
    private float colliderYOffset;
    public float _ColliderYOffset { get => colliderYOffset; }
    protected Rigidbody rb;
    public Rigidbody _Rb { get { return rb; } }
    [SerializeField]
    protected bool canMove;
    [SerializeField]
    protected float speed = 1;
    public float _Speed { get => speed; }


    #region Orientation Fields
    protected Vector3 nextForward;
    public Vector3 _NextForward { get => nextForward; }
    [SerializeField]
    protected Vector3 localUpAxis;
    public Vector3 _LocalUpAxis { get { return localUpAxis; } }
    [SerializeField]
    protected Vector3 localForwardAxis;
    public Vector3 _LocalForwardAxis { get { return localForwardAxis; } }
    protected Vector3 localRightAxis;
    public Vector3 _LocalRightAxis { get { return localRightAxis; } }
    protected Vector3 forward;
    public Vector3 _Forward{ get { return forward; } }
    protected Vector3 up;
    public Vector3 _Up { get { return up; } }
    protected Vector3 right;
    public Vector3 _Right { get { return right; } }
    protected Quaternion startRot;
    public Quaternion _StartRot { get { return startRot; } }
    protected Quaternion currentRotation;
    public Quaternion _CurrentRotation { get { return currentRotation; } }
    protected int rotationFactor;
    public int _RotationFactor { get { return rotationFactor; } }
    #endregion


    #region Submergence Fields
    [SerializeField]
    int waterLayer;
    [SerializeField]
    float submergenceOffset;
    float submergenceRange;
    public float submergence;
    public bool InWater => submergence > 0.5f;
    protected Vector3 headingDirection;
    public Vector3 _HeadingDirection { get => headingDirection; }
    Vector3 prevPos;
    #endregion

    #region Grounding Fields
    protected RaycastHit groundHitInfo;
    public RaycastHit _GroundHitInfo { get => groundHitInfo; }
    protected Vector3 hitNormal;
    public Vector3 _HitNormal { get { return hitNormal; } }
    [SerializeField]
    protected float heightPadding;
    [SerializeField]
    protected int groundLayer;
    [SerializeField]
    protected float groundHitDistance;
    public bool grounded;
    public bool _Grounded { get => grounded; }
    protected float impactDuration;
    protected float impactStartTime;
    protected float impactForce;
    public float _ImpactForce { get => impactForce; }
    #endregion


    protected virtual void Awake()
    {
        col = GetComponent<Collider>();
        colliderYOffset = col.bounds.center.y - transform.position.y;
    }

    protected virtual void Start()
    {
        submergenceRange = submergenceOffset * 2f;
        localRightAxis = Vector3.Cross(localUpAxis, localForwardAxis).normalized;
        localForwardAxis = localForwardAxis.normalized;
        localUpAxis = new Vector3(Mathf.Abs(localUpAxis.x), Mathf.Abs(localUpAxis.y), Mathf.Abs(localUpAxis.z)).normalized;
        startRot = transform.rotation;
    }

    protected virtual void Update()
    {
    }
    //TODO: turn this into a method that gets called by the gameagent
    protected virtual void FixedUpdate()
    {
        //if (Time.time - lastGroundedTime > airDuration)
       // {
           // grounded = false;
       // }

       // hitNormal = Vector3.up;
       // prevPos = transform.position;
        // Debug.Log("fixed update: " + headingDirection);
        //Debug.DrawLine(transform.position, transform.position + right * 5.0f, Color.red);
        //Debug.DrawLine(transform.position, transform.position + forward * 5.0f, Color.blue);
       // Debug.DrawLine(transform.position, transform.position + up * 5.0f, Color.green);

    }

    public void ResetCollisionInfo()
    {
        if (Time.time - lastGroundedTime > airDuration)
        {
            grounded = false;
        }
        hitNormal = Vector3.up;
        prevPos = transform.position;
    }

    protected virtual void LateUpdate()
    {
        headingDirection = (transform.position - prevPos).normalized;
       // Debug.Log("late update: " + headingDirection);
    }

    public void CalculateAlignmentVectors()
    {
        up = transform.rotation * localUpAxis;
        //float sign = System.Math.Sign(Vector3.Dot(up, Vector3.up));
       // up *= sign;
        right = transform.rotation * localRightAxis;
        forward = Vector3.Cross(right, up).normalized;
       // Debug.DrawLine(transform.position, transform.position + forward * 10.0f, Color.blue);
    }

    #region Movement Methods
    public abstract void DoMovement();

    public abstract void Rotate(Vector3 desiredUp);

    public abstract void Rotate(Quaternion rot);

    public abstract void DoTranslation();

    public abstract void AddBuoyancy();

    public abstract void Swim(Vector3 swimDirection);

    public abstract void Jump(Vector3 direction, float power);

    public abstract void OnAir();

    public abstract void OnGrounded();

    public abstract void OnSubmerge();

    public abstract void OnMove();   

    public abstract void OnIdle();

    public abstract void OnDeath();

    //returns the angle in degrees of the desired forward so that it can be fed into the char controller as its target rotation
    public float RotationCorrection(Vector3 desiredForward)
    {       
        return Vector3.SignedAngle(Vector3.forward, MathFunctions.FlattenVector(desiredForward).normalized, startRot * localUpAxis);       
    }

    #endregion

    #region Grounding Methods

    public bool Ground(out Vector3 hitPoint, float rayDistance)
    {
        /* Ray ray = new Ray(transform.position, Vector3.down);
         if (Physics.Raycast(ray, out groundHitInfo, groundHitDistance + heightPadding, 1 << groundLayer, QueryTriggerInteraction.Ignore))
         {
             grounded = true;
             return;
         }*/
        //groundHitInfo.normal = Vector3.up;

        Ray ray = new Ray(transform.position + Vector3.up * colliderYOffset, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, colliderYOffset + rayDistance, 1 << groundLayer, QueryTriggerInteraction.Ignore))
        {
            hitPoint = hit.point;
            grounded = true;
            return true;
        }

        hitPoint = transform.position;
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer == 12 || collision.gameObject.layer == 13)
        {
            //impactForce = (collision.rigidbody.mass * collision.rigidbody.velocity.sqrMagnitude) / Time.fixedDeltaTime;
            impactForce = collision.impulse.magnitude;
            agent._Stats.ApplyDamage(impactForce, CollisionType.InterAgent);
        }

        if(collision.gameObject.layer == groundLayer)
        {
            impactForce = collision.impulse.magnitude;
            //impactForce = rb.mass * rb.velocity.magnitude / Time.fixedDeltaTime;
            agent._Stats.ApplyDamage(impactForce, CollisionType.Ground);
        }


        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

   /* private void OnCollisionExit(Collision collision)
    {
        float mass;
        Vector3 velocity;

        if (collision.gameObject.layer == 12 || collision.gameObject.layer == 13)
        {
            mass = collision.rigidbody.mass;
            velocity = collision.rigidbody.velocity;
            impactDuration = Time.time - impactStartTime;
            impactForce = (mass * velocity.sqrMagnitude) / impactDuration;
            Debug.Log("non ground collision: " + transform.parent.name +   " collidee: " + collision.transform.parent.name + " force: " + impactForce + " velocity = " + velocity.sqrMagnitude + " mass = " + mass);
        }

        else
        {
            mass = 0f;
            velocity = rb.velocity;
        }

        impactDuration = Time.time - impactStartTime;
        impactForce = (mass * velocity.sqrMagnitude) / impactDuration;
        agent._Stats.ApplyDamage(impactForce);
    }*/


    private void EvaluateCollision(Collision collision)
    {
        if(collision.gameObject.layer != groundLayer) { return; }
        Vector3 accumulatedNormal = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            lastGroundedTime = Time.time;
            Vector3 normal = collision.GetContact(i).normal;
            accumulatedNormal += normal;
            grounded = true;
        }
        hitNormal = accumulatedNormal.normalized;
    }
    #endregion

    #region Submergence Methods

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == waterLayer)
        {
            EvaluateSubmergence();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == waterLayer)
        {
            EvaluateSubmergence();
        }
    }

    private void EvaluateSubmergence()
    {
       /* Ray ray = new Ray(transform.position + Vector3.up * submergenceOffset, -Vector3.up);
        if (Physics.Raycast(
            ray, out RaycastHit hit, submergenceRange + 1,
            1 << waterLayer, QueryTriggerInteraction.Collide
        ))
        {
            submergence = 1 - hit.distance / submergenceRange;
        }

        else if (Physics.CheckSphere(
                    transform.position + Vector3.up * submergenceOffset, 0.01f, 1 << waterLayer, QueryTriggerInteraction.Collide))
        {
            submergence = 1f;
        }*/

        Ray ray = new Ray(transform.position + (Vector3.up * (submergenceOffset + colliderYOffset)), -Vector3.up);
        if (Physics.Raycast(
            ray, out RaycastHit hit, submergenceRange,
            1 << waterLayer, QueryTriggerInteraction.Collide
        ))
        {
            submergence = 1f - hit.distance / submergenceRange;
        }

        else if (Physics.CheckSphere(
                    transform.position + Vector3.up * submergenceOffset, 0.01f, 1 << waterLayer, QueryTriggerInteraction.Collide))
        {
            submergence = 1f;
        }

    }


    #endregion


}
