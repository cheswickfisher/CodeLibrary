using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInput : InputModel
{
    
    public enum NavigationSystem
    {
        Unity,
        AStarPathfindingProject
    }

    EnemyAI enemyAI;
    [SerializeField]
    NavigationSystem navigationSystem;

    #region Debugging
    AgentType agentType;
    #endregion

    public void Awake()
    {
        if(navigationSystem == NavigationSystem.Unity)
        {
            enemyAI = gameObject.AddComponent<EnemyAIUnityNavmesh>();
        }

        if(navigationSystem == NavigationSystem.AStarPathfindingProject)
        {
            enemyAI = gameObject.AddComponent<EnemyAIAStarPathfindingProject>();
        }

        ///for debugging only
        agentType = GetComponent<AgentType>();
    }


    public override void Fall()
    {

    }

    public override void Idle()
    {
    }

    public override void Jump()
    {
        
    }

    public override bool _Jumping()
    {
        return false;
    }


    public override int _Spinning()
    {
        return 0;
    }

    public override Quaternion Swim()
    {
        Vector3 desiredUp = Vector3.up;

        if (controls._Grounded && controls._HeadingDirection.y > 0)
        {
            desiredUp = controls._HitNormal;
        }

        Quaternion desiredRotation = Quaternion.identity;

        //desiredUp = controls._StartRot * controls._LocalUpAxis;
        Vector3 forwardDirection = Quaternion.Euler(0, rotationInput, 0) * controls._LocalForwardAxis;
        forwardDirection = Vector3.ProjectOnPlane(forwardDirection, desiredUp);
        desiredRotation = Quaternion.LookRotation(forwardDirection, desiredUp) * controls._StartRot;

        return desiredRotation;
    }

    public override void Walk()
    {
    }

    public override bool _Walking()
    {
        walking = !enemyAI._ReachedEndOfPath();
        return walking;
    }

    public override bool _MouthOpen()
    {
        return false;
    }


    public override void UpdateInputs()
    {
        enemyAI.UpdateInputs(out rotationInput);
        movementInput = System.Convert.ToInt32(_Walking());

        if (enemyAI._ReachedEndOfPath()) { enemyAI.SetRandomTarget(10.0f + Random.Range(0, 20.0f)); }

        ///for debugging only
       //agentType._StateMachine.ConsoleOutputStateInfo();
    }

}
