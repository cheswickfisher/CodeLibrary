using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RossMaths;

public class AirState : State
{
    float startTime;
    float currentTime;
    float duration = .1f;
    bool airCheck;

    public override void DoActions(AgentType agent)
    {
        currentTime = Time.time;
        agent._Controls.DoMovement();
        if (currentTime - startTime > duration && !airCheck)
        {
            agent._Controls._AC.SetBool("Walk", true);
            airCheck = true;
        }
    }

    public override void EnterState(AgentType agent)
    {
        agent._Controls.OnAir();
        airCheck = false;
        startTime = Time.time;
    }

    public override void ExitState(AgentType agent)
    {
        agent._Controls._AC.SetBool("Walk", false);
    }


}
