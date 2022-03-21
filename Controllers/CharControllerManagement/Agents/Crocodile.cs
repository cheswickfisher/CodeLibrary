using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RossMaths;
using System;

public enum SpinStateActions
{
    Spin,
    ResetSpinRotation
}

public enum JumpStateActions
{
    Jump,
    StopJump
}

public enum BiteStateActions
{
    OpenMouth,
    CloseMouth,
    StopCoroutine
}

public class Crocodile : Animal
{
    public Transform zAlignTransform;
    float spinAmt;
    [SerializeField]
    float jumpPwr;
    [SerializeField]
    float maxJumpPwr = 100;
    float jumpPwrChargeRate = 50.0f;
    [SerializeField]
    float jumpAngle;
    float maxJumpAngle = 80;
    [SerializeField]
    float spinSpeed = 10.0f;
    [SerializeField]
    Transform head;
    [SerializeField]
    Bite3 biteInteraction;
    Vector3 headForwardBase;
    Vector3 launchDirection;
    int currentRotationDirection;
    public bool spin;
    bool mouthOpen;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        headForwardBase = head.up;
        launchDirection = Vector3.up;
        base.Start();
    }

   /* protected override void UpdateInputPlayer()
    {
        spin = Input.GetMouseButton(0) == true ? true : false;
        jump = Input.GetKeyDown(KeyCode.Space) ? true : false;
        mouthOpen = Input.GetKey(KeyCode.F) ? true : false;
        base.UpdateInputPlayer();
    }

    protected override void UpdateInputEnemy()
    {
        base.UpdateInputEnemy();
    }*/


    #region SubStateMachines
    StateMachine spinSubStateMachine;
    StateMachine jumpSubStateMachine;
    StateMachine biteSubStateMachine;
    #endregion

    #region States/Decisions

    SpinState spinState = new SpinState();
    JumpState jumpState = new JumpState();
    MouthOpenState mouthOpenState = new MouthOpenState();

    Func<bool> EnterSpinStateDecision() => () => inputModel._Spinning() != 0; 
    Func<bool> ExitSpinStateDecision() => () => inputModel._Spinning() == 0; 
    Func<bool> EnterJumpStateDecision() => () => inputModel._Jumping() && controls._Grounded;
    Func<bool> ExitJumpStateDecision() => () => !inputModel._Jumping();
    Func<bool> EnterMouthOpenStateDecision() => () => inputModel._MouthOpen();
    Func<bool> ExitMouthOpenStateDecision() => () => !inputModel._MouthOpen();

    public override void StateMachineInit(List<StateMachine> subStates)
    {
        //SubState
        EmptyState spinEmptyState = new EmptyState();
        spinEmptyState.SetTransition(new Transition(spinState, EnterSpinStateDecision()));
        spinState.SetTransition(new Transition(spinEmptyState, ExitSpinStateDecision()));
            AssignSpinActions();
            actionDictionary.Add(SpinStateActions.Spin, SpinAction);
            actionDictionary.Add(SpinStateActions.ResetSpinRotation, ResetSpinRotationAction);
        EmptyState jumpEmptyState = new EmptyState();
        jumpEmptyState.SetTransition(new Transition(jumpState, EnterJumpStateDecision()));
        jumpState.SetTransition(new Transition(jumpEmptyState, ExitJumpStateDecision()));
            AssignJumpActions();
            actionDictionary.Add(JumpStateActions.Jump, JumpAction);
            actionDictionary.Add(JumpStateActions.StopJump, StopJumpAction);
        EmptyState biteEmptyState = new EmptyState();
        biteEmptyState.SetTransition(new Transition(mouthOpenState, EnterMouthOpenStateDecision()));
        mouthOpenState.SetTransition(new Transition(biteEmptyState, ExitMouthOpenStateDecision()));
            AssignBiteActions();
            actionDictionary.Add(BiteStateActions.OpenMouth, OpenMouthAction);
            actionDictionary.Add(BiteStateActions.CloseMouth, CloseMouthAction);
            actionDictionary.Add(BiteStateActions.StopCoroutine, StopCoroutineAction);

        spinSubStateMachine = new StateMachine();
        spinSubStateMachine._StateMachineName = "SpinSubStateMachine";
        spinSubStateMachine.Init(spinEmptyState, this);

        jumpSubStateMachine = new StateMachine();
        jumpSubStateMachine._StateMachineName = "JumpAttackSubStateMachine";
        jumpSubStateMachine.Init(jumpEmptyState, this);

        biteSubStateMachine = new StateMachine();
        biteSubStateMachine._StateMachineName = "BiteSubStateMachine";
        biteSubStateMachine.Init(biteEmptyState, this);

        subStates.Add(spinSubStateMachine);
        subStates.Add(jumpSubStateMachine);
        subStates.Add(biteSubStateMachine);

        base.StateMachineInit(subStates);
    }

    #endregion

    #region Spin

    #region Actions
    Action SpinAction;
    Action ResetSpinRotationAction;

    private void AssignSpinActions()
    {
        SpinAction = Spin;
        ResetSpinRotationAction = ResetSpinRotation;
    }
    #endregion

    #region Spin Methods
    private void Spin()
    {
        if (resetSpinCoroutine != null)
        {
            StopCoroutine(resetSpinCoroutine);
        }


        Quaternion pp = Quaternion.Euler(controls._LocalUpAxis * inputModel._RotationInput * controls._RotationFactor);
        Quaternion upRot = Quaternion.FromToRotation(Vector3.up, controls._HitNormal);
        Vector3 _u = controls._HitNormal;
       // Vector3 _u = controls._Up;
        Vector3 _f = upRot * pp * Vector3.forward;
       // Vector3 _f = controls._Forward;
        Quaternion lr = Quaternion.LookRotation(_f, _u);
        spinAmt += spinSpeed * inputModel._Spinning(); 
        Quaternion s = Quaternion.Euler(controls._LocalForwardAxis * spinAmt);
        lr = lr * s;
       // controls._Rb.AddTorque(controls._CurrentRotation * controls._LocalForwardAxis * spinSpeed, ForceMode.Impulse);
        controls.Rotate(lr);
        //controls.Rotate(s);


    }

    private void ResetSpinRotation()
    {
        if (resetSpinCoroutine != null)
        {
            StopCoroutine(resetSpinCoroutine);
        }
        resetSpinCoroutine = ResetSpinRotationCoroutine();

        StartCoroutine(resetSpinCoroutine);
    }
    #endregion

    #region Coroutines
    private IEnumerator resetSpinCoroutine;

    private IEnumerator ResetSpinRotationCoroutine()
    {
        float v = v = zAlignTransform.localRotation.eulerAngles.magnitude;
        while (Mathf.Abs(v - Quaternion.identity.eulerAngles.magnitude) > 1.0f)
        {
            zAlignTransform.localRotation = Quaternion.Slerp(zAlignTransform.localRotation, Quaternion.identity, Time.deltaTime * spinSpeed);
            v = zAlignTransform.localRotation.eulerAngles.magnitude;
            yield return null;
        }
        resetSpinCoroutine = null;
    }
    #endregion

    #endregion

    #region Jump Attack

    #region Actions
    Action JumpAction;
    Action StopJumpAction;

    private void AssignJumpActions()
    {
        JumpAction = StartJumpCoroutines;
        StopJumpAction = StopJumpCoroutines;
    }
    #endregion

    #region Jump Methods

    private void Jump()
    {
        Vector3 right = controls._Right;
        Vector3 forward = controls._Forward;
       
        Quaternion pp = Quaternion.AngleAxis(jumpAngle, right);
        //launchDirection = biting ? head.up : agent._Controls._HitNormal;    
        launchDirection = controls._HitNormal;
        Vector3 f = pp * forward;
        controls.Jump(f, jumpPwr);
    }

    #endregion

    #region Coroutine

    private IEnumerator trackInputEnumerator;
    private IEnumerator trackPowerAndAngleEnumerator;

    private void StartJumpCoroutines()
    {
        if (!controls._Grounded) { return; }
        if(trackInputEnumerator != null) { StopCoroutine(trackInputEnumerator); }
        trackInputEnumerator = TrackInputCoroutine();
        StartCoroutine(trackInputEnumerator);
    }

    private void StopJumpCoroutines()
    {
        Jump();
        if (trackInputEnumerator != null) { StopCoroutine(trackInputEnumerator); }
        if (trackPowerAndAngleEnumerator != null) { StopCoroutine(trackPowerAndAngleEnumerator); }
    }

    private IEnumerator TrackInputCoroutine()
    {
        if(trackPowerAndAngleEnumerator != null) { StopCoroutine(trackPowerAndAngleEnumerator); }
        trackPowerAndAngleEnumerator = TrackPowerAndAngleCoroutine();
        StartCoroutine(trackPowerAndAngleEnumerator);

        while (Input.GetMouseButton(1))
        {
            yield return null;
        }

    }

    private IEnumerator TrackPowerAndAngleCoroutine()
    {
        float startTime = Time.time;
        float elapsedTime = 0;
        //jumpAngle = 0;
        while (true)
        {
            elapsedTime = Time.time - startTime;
           // jumpPwr = Mathf.Lerp(0, maxJumpPwr, MathFunctions.EaseOut((elapsedTime * jumpPwrChargeRate) / 100, 2.0f));

            //jumpAngle += Input.GetAxis("Mouse Y");
            //jumpAngle = Mathf.Min(jumpAngle, maxJumpAngle);
            Vector3 headForward = head.up;
            float angle = Vector3.SignedAngle(headForwardBase, headForward, head.right);
            //jumpAngle = angle;
            yield return null;
        }
    }


    #endregion


    #endregion

    #region Bite

    #region Actions

    Action OpenMouthAction;
    Action CloseMouthAction;
    Action StopCoroutineAction;

    private void AssignBiteActions()
    {
        OpenMouthAction = OpenMouth;
        CloseMouthAction = StartCloseMouthCoroutine;
        StopCoroutineAction = StopCloseMouthCoroutine;
    }

    #endregion
    #region Fields
    public float bite;
    bool biting;
    #endregion
    #region Methods
    private void OpenMouth()
    {
        if (biteInteraction._Engaged && !biting) { biteInteraction.Release(); }
        bite += Time.unscaledDeltaTime;
        bite = Mathf.Clamp01(bite);
        controls._AC.SetFloat("Bite", bite);
        biting = true;
        launchDirection = head.up;
        if(bite > 0.9f) { biteInteraction.SetBiteColliderActive(true); }
    }

    #endregion
    #region Coroutines
    private IEnumerator closeMouthEnumerator;
    
    private void StartCloseMouthCoroutine()
    {
        StopCloseMouthCoroutine();
        closeMouthEnumerator = CloseMouthCoroutine();
        StartCoroutine(closeMouthEnumerator);
    }

    private void StopCloseMouthCoroutine()
    {
        if (closeMouthEnumerator != null) { StopCoroutine(closeMouthEnumerator); }
    }

    private IEnumerator CloseMouthCoroutine()
    {
        launchDirection = Vector3.up;
        biting = false;
        while(bite > 0f)
        {
            bite -= Time.unscaledDeltaTime * 6f;
            controls._AC.SetFloat("Bite", bite);
            yield return null;
        }
        bite = Mathf.Clamp01(bite);
        controls._AC.SetFloat("Bite", bite);
        biteInteraction.SetBiteColliderActive(false);
    }
    #endregion

    #endregion

}
