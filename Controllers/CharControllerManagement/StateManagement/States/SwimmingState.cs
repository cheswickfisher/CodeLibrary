using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimState : State
{
    public override void DoActions(AgentType agent)
    {
        agent._Controls.Rotate(agent.inputModel.Swim());
        agent._Controls.DoTranslation();
        agent._Controls.AddBuoyancy();
        //agent._Controls.Swim(Camera.main.transform.forward);  
    }

    public override void EnterState(AgentType agent)
    {
        agent._Controls._AC.SetBool("Swim", true);
        agent._Controls.OnSubmerge();
    }

    public override void ExitState(AgentType agent)
    {
        agent._Controls._AC.SetBool("Swim", false);
    }
}

