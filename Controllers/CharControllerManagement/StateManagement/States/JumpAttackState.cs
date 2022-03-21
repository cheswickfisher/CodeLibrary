using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : State
{
    public override void DoActions(AgentType agent)
    {
        
    }

    public override void EnterState(AgentType agent)
    {

        agent._ActionDictionary[JumpStateActions.Jump]();
    }

    public override void ExitState(AgentType agent)
    {

        agent._ActionDictionary[JumpStateActions.StopJump]();
    }
}
