using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouthOpenState : State
{
    public override void DoActions(AgentType agent)
    {
        agent._ActionDictionary[BiteStateActions.OpenMouth]();
    }

    public override void EnterState(AgentType agent)
    {
        agent._ActionDictionary[BiteStateActions.StopCoroutine]();
    }

    public override void ExitState(AgentType agent)
    {
        agent._ActionDictionary[BiteStateActions.CloseMouth]();
    }
}

/*public class MouthOpenSubState : SubState
{
    public override void EnterSubState(SimpleCharControls controls)
    {
        controls.OpenMouth();
    }

    public override void ExitSubState(SimpleCharControls controls)
    {
       // controls.biteCollider.enabled = false;
    }

    public override void UpdateSubState(SimpleCharControls controls)
    {

        if (Input.GetMouseButtonDown(0))
        {
            controls.TransitionSubState(controls.SubStateMouthClosed);
            return;
        }

        if (controls.engaged)
        {
            controls.TransitionSubState(controls.SubStateBiting);
            return;
        }

        if(controls.Jaws >= .9f)
        {
            controls.EnableBiteColliders(true);
            
        }

        controls.ac.SetFloat("Bite", controls.Jaws);

    }
}*/
