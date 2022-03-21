using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public enum InputAgent
{
    Player,
    Enemy
}

public abstract class AgentType : MonoBehaviour
{
    [SerializeField]
    InputAgent inputAgent;
    [SerializeField]
    protected CharController controls;
    protected StateMachine stateMachine;
    protected float rotationInput;
    protected int rotationFactor;
    protected Vector3 moveVector;
    public StateMachine _StateMachine { get => stateMachine; }
    protected Dictionary<Enum, Action> actionDictionary = new Dictionary<Enum, Action>();
    public Dictionary<Enum,Action> _ActionDictionary { get { return actionDictionary; } }
    public CharController _Controls { get => controls; }
    public float _RotationInput { get => rotationInput; set => rotationInput = value; }
    protected int RotationFactor { get => rotationFactor; set => rotationFactor = value; }
    public Vector3 _MoveVector { get => moveVector; set => moveVector = value; }
    public InputAgent InputAgent { get => inputAgent; }    
    public InputModel inputModel;
    [SerializeField]
    protected Stats stats;
    public Stats _Stats { get => stats; }

    protected List<Transition> anyStateTransitions = new List<Transition>();   

    protected virtual void Awake()
    {
        inputModel._Controls = controls;
    }

    protected virtual void Start()
    {
        StateMachineInit(new List<StateMachine>());
    }

    protected virtual void FixedUpdate()
    {
        controls.CalculateAlignmentVectors();
        inputModel.UpdateInputs();
        UpdateStateMachines();
        controls.ResetCollisionInfo();
    }


    public abstract void StateMachineInit(List<StateMachine> subStates);
    public virtual void UpdateStateMachines()
    {
        if(stateMachine == null) { return; }
        stateMachine.UpdateState(this);
        stateMachine.UpdateSubStateMachines(this);
    }


    public void HaltCoroutine(IEnumerator enumerator)
    {
        if (enumerator != null) { StopCoroutine(enumerator); }
    }

    public void BeginCoroutine(IEnumerator enumerator)
    {
        if(enumerator != null) { StartCoroutine(enumerator); }
    }

}
