using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RossMaths;
using System;

public class PhysicsCharController : CharController
    
{
    [SerializeField]
    protected float jump = 500;
    [SerializeField]
    protected bool stabilize;
    [SerializeField][Range(0, 90.0f)]
    float boostForceKickInAngle = 45.0f;
    [SerializeField] [Range(0,1)]
    float movingFriction = 0.2f;
    [SerializeField] [Range(0, 1)]
    float idleFriction = 0.5f;
    [SerializeField]
    float buoyancyForce = 500;

    ConstantForce downForce;
    ConfigurableJoint joint;
    float drag;
    float angularDrag;
    float positionSpring;
    float startPosSpring;
    float lastPosSpring;
    float positionDamper;
    float startPosDamper;
    float lastPosDamper;
    float movementMtp = 1.0f;
    Quaternion jointStartRot;
    float b;
    float bounciness;
    float friction;
    float downForceY;

    private IEnumerator BounceEnumerator;
    private IEnumerator AirTimeEnumerator;

    //public Transform alignmentObject;
   // public ConfigurableJoint alignmentObjectJoint;

    public ConfigurableJoint _Joint { get => joint; }

    protected override void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();
        downForce = GetComponent<ConstantForce>();
        downForceY = downForce.force.y;
        drag = rb.drag;
        angularDrag = rb.angularDrag;
        joint.configuredInWorldSpace = true;
        //positionSpring = joint.slerpDrive.positionSpring;
       // positionDamper = joint.slerpDrive.positionDamper;

        lastPosSpring = joint.slerpDrive.positionSpring;
        lastPosDamper = joint.slerpDrive.positionDamper;

        startPosSpring = lastPosSpring;
        startPosDamper = lastPosDamper;

        base.Awake();
    }

    protected override void Start()
    {
        float angle = Vector3.Dot(localUpAxis, Vector3.up);
        rotationFactor = angle <= 0 ? -1 : 1;
       // groundHitDistance = 10.0f;
        //Ground(out hitNormal, heightPadding);
       // up = groundHitInfo.normal;
       // groundHitDistance = Mathf.Min(groundHitInfo.distance, 10.0f);
        base.Start();
    }

    protected override void FixedUpdate()
    {
        var j = joint.slerpDrive;
        j.positionSpring = startPosSpring * agent._Stats.CurrentDamageFactor();
        j.positionDamper = startPosDamper * agent._Stats.CurrentDamageFactor();
        joint.slerpDrive = j;


        base.FixedUpdate();
    }


    public override void DoMovement()
    {
        Rotate(hitNormal);
        if (canMove)
        {
            DoTranslation();
        }       
    }

    public override void Rotate(Vector3 desiredUp)
    {
       // CalculateAlignmentVectors();

        if (stabilize)
        {

            Quaternion pp = Quaternion.Euler(localUpAxis * agent.inputModel._RotationInput * rotationFactor);
            Quaternion upRot = Quaternion.FromToRotation(Vector3.up, desiredUp);
            Vector3 _u = desiredUp;            
            Vector3 _f = upRot * pp * Vector3.forward;
            currentRotation = Quaternion.LookRotation(_f, _u);
            nextForward = _f;
            //should be able to avoid this method by inverting currentRotation and applying it directly to the joint's targetRotation.
            ConfigurableJointExtensions.SetTargetRotation(joint, currentRotation, startRot);
           // Debug.DrawLine(transform.position, transform.position + _u * 8.0f, Color.green);
            //Debug.DrawLine(transform.position, transform.position + _f * 8.0f, Color.blue);

        }
        else
        {
           // Quaternion n = Quaternion.Euler(up * agent._Input.rotationInput * rotationFactor);
           // Rotate(n);
        }

        //Alternate rotation method that is the same as Swim rotation method. Downside of this is that the controls are bad on steep inclines because of the Vector projection.
        /* CalculateAlignmentVectors();
         Quaternion rot = Quaternion.identity;
         //desiredUp = startRot * localUpAxis;
         float sign = System.Math.Sign(Vector3.Dot(desiredUp, Vector3.up));
         desiredUp *= sign;
         Vector3 forwardDirection = Quaternion.Euler(0, agent._Input.rotationInput, 0) * localUpAxis;
         forwardDirection = Vector3.ProjectOnPlane(forwardDirection, desiredUp);
         Quaternion desiredRotation = Quaternion.LookRotation(forwardDirection, desiredUp) * startRot;
         rot = Quaternion.Slerp(transform.rotation, desiredRotation, MathFunctions.GetInterpolationAlpha(10.0f));*/

        //Alternate rotation method. Works well, but is not physics based and doesn't allow for character to flip over.
        /*Quaternion rot = Quaternion.identity;
          Quaternion pp = Quaternion.Euler(localUpAxis * agent._Input.rotationInput * rotationFactor);
          CalculateAlignmentVectors();
          Quaternion aa = Quaternion.FromToRotation(Vector3.up, Vector3.Slerp(up, desiredUp, Time.deltaTime * 6.0f));
          rot = aa * startRot * pp;
         //Rotate(rot);*/
    }

    public override void Rotate(Quaternion rotation)
    {
        //CalculateAlignmentVectors();
        ConfigurableJointExtensions.SetTargetRotation(joint, rotation, startRot);
    }

    public override void DoTranslation()
    {
        rb.AddForce(forward * agent.inputModel._MovementInput * movementMtp * speed * agent._Stats.CurrentDamageFactor(), ForceMode.Impulse);
        float offForwardAngle = Mathf.Cos(boostForceKickInAngle * Mathf.Deg2Rad);
        if (offForwardAngle > hitNormal.y && headingDirection.y > 0)
        {
            //rb.AddForce(InclineHelper(agent._MoveVector), ForceMode.Force);
            rb.AddForce(InclineHelper(forward), ForceMode.Force);
        }
    }

    private Vector3 InclineHelper(Vector3 moveVector)
    {
        float mass = rb.mass;
        float gravity = Mathf.Abs(Physics.gravity.y);
        float angle = Vector3.Angle(hitNormal, Vector3.up);
        float staticFriction = col.material.staticFriction;
        float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);        
        float force;
        force = (mass * gravity * sin) + ((staticFriction) * mass * gravity * cos);
        float velocity = rb.velocity.magnitude * rb.drag;           
        float dragCoefficient = (1 - Time.deltaTime * rb.drag);
        float addedForce = ((velocity + force) / dragCoefficient);
        return (moveVector * ((velocity + force) / dragCoefficient));
    }

    public override void Jump(Vector3 direction, float power)
    {
        rb.AddForce(direction * power, ForceMode.VelocityChange);
    }

    public override void AddBuoyancy()
    {
        rb.AddForce(Vector3.up * buoyancyForce * submergence, ForceMode.Force);
    }

    public override void Swim(Vector3 swimDirection)
    {
        Vector3 desiredUp = Vector3.up;
        if (grounded && headingDirection.y > 0)
        {
            desiredUp = hitNormal;
        }

        Quaternion rot = Quaternion.identity;
        if (Input.GetMouseButton(1))
        {
            Quaternion desiredForward = Quaternion.LookRotation(swimDirection);
            rot = Quaternion.Slerp(transform.rotation, desiredForward * startRot, MathFunctions.GetInterpolationAlpha(10.0f));
            agent._RotationInput = RotationCorrection(transform.rotation * localForwardAxis);
            ConfigurableJointExtensions.SetTargetRotation(joint, desiredForward, startRot);
        }
        else
        {
            desiredUp = startRot * localUpAxis;
            Vector3 forwardDirection = Quaternion.Euler(0, agent._RotationInput, 0) * localForwardAxis;
            forwardDirection = Vector3.ProjectOnPlane(forwardDirection, desiredUp);
            Quaternion desiredRotation = Quaternion.LookRotation(forwardDirection, desiredUp) * startRot;
            rot = Quaternion.Slerp(transform.rotation, desiredRotation, MathFunctions.GetInterpolationAlpha(6.0f));
            ConfigurableJointExtensions.SetTargetRotation(joint, desiredRotation, startRot);
        }
        CalculateAlignmentVectors();
        DoTranslation();
        rb.AddForce(Vector3.up * buoyancyForce * submergence, ForceMode.Force);
    }

    public override void OnGrounded()
    {    
        b = 0;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        var j = joint.slerpDrive;
        // j.positionSpring = positionSpring;
        // j.positionDamper = positionDamper;
        j.positionSpring = lastPosSpring;
        j.positionDamper = lastPosDamper;
        joint.slerpDrive = j;
        movementMtp = 1f;
        stabilize = true;
        if (BounceEnumerator != null) { StopCoroutine(BounceEnumerator); }
        BounceEnumerator = BounceOut();
        downForce.enabled = false;

    }
    public override void OnAir()
    {

        b = 0;
        rb.drag = 0;
        rb.angularDrag = 0;
        var j = joint.slerpDrive;
        lastPosSpring = j.positionSpring;
        lastPosDamper = j.positionDamper;
        j.positionSpring = 0;
        j.positionDamper = 0;
        joint.slerpDrive = j;
        downForce.enabled = true;
        movementMtp = .1f;
        stabilize = false;
        if(BounceEnumerator != null) { StopCoroutine(BounceEnumerator); }
        BounceEnumerator = BounceIn();
        StartCoroutine(BounceEnumerator);
        col.material.dynamicFriction = 0f;
        col.material.staticFriction = 0f;
        col.material.frictionCombine = PhysicMaterialCombine.Minimum;
        friction = 0f;
    }
    public override void OnSubmerge()
    {
        b = 0;
        rb.drag = drag * .5f;
        rb.angularDrag = angularDrag * .5f;
        var j = joint.slerpDrive;
        j.positionSpring = positionSpring;
        j.positionDamper = positionDamper;
        joint.slerpDrive = j;
        movementMtp = .5f;
        downForce.enabled = true;
        stabilize = true;
    }

    public override void OnMove()
    {
        col.material.dynamicFriction = movingFriction;
        col.material.staticFriction = movingFriction;
        col.material.frictionCombine = PhysicMaterialCombine.Minimum;
        friction = movingFriction;
    }

    public override void OnIdle()
    {
        col.material.dynamicFriction = idleFriction;
        col.material.staticFriction = idleFriction;
        col.material.frictionCombine = PhysicMaterialCombine.Average;
        friction = idleFriction;
    }

    public override void OnDeath()
    {
        var j = joint.slerpDrive;
        j.positionSpring = 0f;
        j.positionDamper = 0f;
        joint.slerpDrive = j;
    }

    private IEnumerator BounceIn()
    {
        b = 0;
        bounciness = col.material.bounciness;
        while (bounciness < 1.0f)
        {
            b += Time.fixedDeltaTime;
            bounciness = Mathf.Lerp(bounciness, 1.0f, MathFunctions.EaseIn(b, 2));
            col.material.bounciness = bounciness;
            yield return null;
        }
    }
    private IEnumerator BounceOut()
    {
        b = 0;
        bounciness = col.material.bounciness;
        while (bounciness > 0.0f)
        {
            b += Time.fixedDeltaTime;
            bounciness = Mathf.Lerp(bounciness, 0.0f, MathFunctions.EaseIn(b, 2));
            col.material.bounciness = bounciness;
            yield return null;
        }
    }


}







