using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkState : GroundedState
   
{
    public override void DoActions(AgentType agent)
    {
        agent._Controls.DoMovement();
        base.DoActions(agent);
    }

    public override void EnterState(AgentType agent)
    {
        agent._Controls.OnMove();
        agent._Controls._AC.SetBool("Walk", true);
        base.EnterState(agent);
    }

    public override void ExitState(AgentType agent)
    {
        agent._Controls._AC.SetBool("Walk", false);
        base.ExitState(agent);
    }
}
