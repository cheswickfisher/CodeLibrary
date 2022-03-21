using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class SpinState : State
    
{
    public override void DoActions(AgentType agent)
    {
        agent._ActionDictionary[SpinStateActions.Spin]();
    }

    public override void EnterState(AgentType agent)
    {
        agent._Controls._AC.SetBool("Spin", true);
    }

    public override void ExitState(AgentType agent)
    {
        agent._Controls._AC.SetBool("Spin", false);
        agent._ActionDictionary[SpinStateActions.ResetSpinRotation]();
    }
}


/*public class SpinningState : GroundedState
    
{
    
    public override void EnterState(GameAgent agent)
    {
        Physics.IgnoreLayerCollision(8, 11, true);
        //controls._AC.SetBool("Walk", true);
        base.EnterState(agent);
    }

    public override void ExitState(GameAgent agent)
    {
        //controls.ResetSpinRotation();
        Physics.IgnoreLayerCollision(8, 11, false);
       // controls._AC.SetBool("Walk", false);
        base.ExitState(agent);
    }

    public override void Update(GameAgent agent)
    {
        //controls.Spin();
        if (Input.GetMouseButtonUp(1))
        {
           // controls.TransitionState(controls.StateIdle);
        }

        /*if (controls.limbTarget != null)
        {
            if (!controls.CheckBiteTargetDistance(0f))
                controls.ReleaseLimb();
        }


        base.Update(agent);
    }

}*/
