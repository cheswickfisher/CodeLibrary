using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class State   
{  
    private List<Transition> transitions = new List<Transition>();
    public List<Transition> _Transitions { get { return transitions; } }

    private StateMachine stateMachine;
    public StateMachine _StateMachine { get { return stateMachine; } set { stateMachine = value; } }

    private Action customAction; 

    public abstract void EnterState(AgentType agent);

    public abstract void ExitState(AgentType agent);

    public abstract void DoActions(AgentType agent);   

    public void SetTransition(Transition t)
    {
        foreach(Transition transition in transitions)
        {
            if(transition.toState == t.toState)
            {
                return;
            }
        }
        transitions.Add(t);
    }

    public void SetCustomAction(Action a)
    {
        customAction = a;
    }

    protected void CustomAction()
    {
        if(customAction == null) { return; }
        customAction();
    }
}


