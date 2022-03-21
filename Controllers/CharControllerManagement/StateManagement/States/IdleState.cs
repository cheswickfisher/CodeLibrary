using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : GroundedState 
    
{
    public override void DoActions(AgentType agent)
    {
        agent._Controls.DoMovement();
        base.DoActions(agent);
    }

    public override void EnterState(AgentType agent)
    {
        agent._Controls.OnIdle();
        agent._Controls._AC.SetBool("Idle", true);
        base.EnterState(agent);
    }

    public override void ExitState(AgentType agent)
    {
        agent._Controls._AC.SetBool("Idle", false);
        base.ExitState(agent);
    }
}
