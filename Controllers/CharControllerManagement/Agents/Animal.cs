using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Animal : AgentType
{
    #region Fields
    protected bool walking;
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        stats.UpdateHealth();
        base.FixedUpdate();
    }

    /* protected override void UpdateInputPlayer()
     {
         float nextRotation = (Input.GetAxis("Horizontal") * Time.deltaTime * 300f);
         rotationFactor = Convert.ToInt32(Input.GetMouseButton(0)) + (2 * (Convert.ToInt32(Input.GetMouseButton(1)) * -1));
         rotationInput += nextRotation;
         moveVector = (Input.GetAxis("Vertical") * controls._Forward).normalized * Mathf.Abs(Input.GetAxisRaw("Vertical"));
         walking = Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0 ? true : false;        
     }

     protected override void UpdateInputEnemy()
     {
         enemyAI.UpdateInputs();
         rotationInput = enemyAI._RotationAmt();
         moveVector = Vector3.ProjectOnPlane(enemyAI._MoveVector(), controls._HitNormal).normalized;
         walking = !enemyAI._ReachedEndOfPath();
     }*/


    #region SubStateMachines

    #endregion

    #region States/Decisions

    IdleState idleState = new IdleState();
    WalkState walkState = new WalkState();
    SwimState swimState = new SwimState();
    AirState airState = new AirState();

    Func<bool> GroundedToAirDecision() => () => !controls._Grounded;
    Func<bool> AirToGroundIdleDecision() => () => controls._Grounded && !inputModel._Walking();
    Func<bool> AirToGroundWalkDecision() => () => controls._Grounded && inputModel._Walking();
    Func<bool> AirToWaterDecision() => () => controls.InWater;
    Func<bool> IdleToWalkDecision() => () => inputModel._Walking();
    Func<bool> WalkToIdleDecision() => () => !inputModel._Walking();
    Func<bool> EnterSwimStateDecision() => () => controls.InWater;
    Func<bool> ExitSwimStateDecision() => () => !controls.InWater;

    public override void StateMachineInit(List<StateMachine> subStates)
    {
        #region SubState Transitions
        #endregion

        #region State Transitions
        //State
        idleState.SetTransition(new Transition(walkState, IdleToWalkDecision()));
        idleState.SetTransition(new Transition(swimState, EnterSwimStateDecision()));
        idleState.SetTransition(new Transition(airState, GroundedToAirDecision()));
        walkState.SetTransition(new Transition(idleState, WalkToIdleDecision()));
        walkState.SetTransition(new Transition(swimState, EnterSwimStateDecision()));
        walkState.SetTransition(new Transition(airState, GroundedToAirDecision()));
        swimState.SetTransition(new Transition(walkState, ExitSwimStateDecision()));
        airState.SetTransition(new Transition(idleState, AirToGroundIdleDecision()));
        airState.SetTransition(new Transition(walkState, AirToGroundWalkDecision()));
        airState.SetTransition(new Transition(swimState, AirToWaterDecision()));
        #endregion

        #region StateMachine Set-Up

        stateMachine = new StateMachine();
        stateMachine._StateMachineName = "StateMachine";
        stateMachine.Init(idleState, this);

        if (subStates != null)
        {
            foreach (StateMachine sm in subStates)
            {
                stateMachine.AddSubStateMachine(sm);
            }
        }

        #endregion

    }

    #endregion



}
