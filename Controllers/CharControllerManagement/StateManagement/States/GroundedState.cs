using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RossMaths;

public class GroundedState : State
{
    public override void DoActions(AgentType agent)
    {
        //agent._Controls.OnGrounded();
    }

    public override void EnterState(AgentType agent)
    {
        agent._Controls.OnGrounded();
    }

    public override void ExitState(AgentType agent)
    {        
    }
}

