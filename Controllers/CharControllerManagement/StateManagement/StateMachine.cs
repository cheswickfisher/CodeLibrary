using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StateMachine 
{
    protected string stateMachineName;
    public string _StateMachineName { set { stateMachineName = value; } }
    protected State currentState;
    public State _CurrentState { get { return currentState; } }
    protected State defaultState;
    public State _DefaultState { get { return defaultState; } set { defaultState = value; } }

    protected List<StateMachine> subStateMachines = new List<StateMachine>();

    public void Init(State defaultState, AgentType agent)
    {
        if(currentState != null) { Debug.LogError("StateMachine already initialized"); return;  }
        this.defaultState = defaultState;
        currentState = defaultState;
        currentState.EnterState(agent);
    }

    public virtual void UpdateState(AgentType agent)
    {
        currentState.DoActions(agent);
        CheckTransitions(agent);
    }

    public void UpdateSubStateMachines(AgentType agent)
    {
        foreach(StateMachine subStateMachine in subStateMachines)
        {
            subStateMachine.UpdateState(agent);
            subStateMachine.UpdateSubStateMachines(agent);
        }
    }

    protected virtual void TransitionState(State nextState, AgentType agent)
    {
        if (currentState == null) { currentState = nextState; return; }
        currentState.ExitState(agent);
        currentState = nextState;
        currentState.EnterState(agent);
    }

    protected virtual void CheckTransitions(AgentType agent)
    {
        foreach (Transition t in currentState._Transitions)
        {
            if (t.Decision() && t.toState.ToString() != ToString())
            {
                TransitionState(t.toState, agent);
                return;
            }
        }
    }

    public void AddSubStateMachine(StateMachine subStateMachine)
    {
        subStateMachines.Add(subStateMachine);
    }

    public void ConsoleOutputStateInfo()
    {
        Debug.Log("currentStateMachine: " + stateMachineName + " currentState:" + currentState.ToString());
    }

}
