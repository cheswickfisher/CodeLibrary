using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RossMaths;
using System;

public class PlayerInput : InputModel
{
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
        return Input.GetKeyDown(KeyCode.Space) ? true : false;
    }

    public override int _Spinning()
    {
        return Convert.ToInt32(Input.GetMouseButton(0)) * (1 + (2 * (Convert.ToInt32(Input.GetMouseButton(1)) * -1)));
    }

    public override Quaternion Swim()
    {
        Vector3 desiredUp = Vector3.up;

        if (controls._Grounded && controls._HeadingDirection.y > 0)
        {
            desiredUp = controls._HitNormal;
        }

        Quaternion desiredRotation = Quaternion.identity;
        if (Input.GetMouseButton(1))
        {
            Quaternion desiredForward = Quaternion.LookRotation(Camera.main.transform.forward);
            desiredRotation = Quaternion.Slerp(transform.rotation, desiredForward * controls._StartRot, MathFunctions.GetInterpolationAlpha(10.0f));
        }

        else
        {
            //desiredUp = controls._StartRot * controls._LocalUpAxis;
            Vector3 forwardDirection = Quaternion.Euler(0, rotationInput, 0) * controls._LocalForwardAxis;
            forwardDirection = Vector3.ProjectOnPlane(forwardDirection, desiredUp);
            desiredRotation = Quaternion.LookRotation(forwardDirection, desiredUp) * controls._StartRot;
        }

        return desiredRotation;
    }

    public override void Walk()
    {
    }

    public override bool _Walking()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0 ? true : false;
    }

    public override bool _MouthOpen()
    {
        return Input.GetKey(KeyCode.LeftShift) ? true : false; 
    }

    public override void UpdateInputs()
    {
        rotationInput += (Input.GetAxis("Horizontal") * Time.deltaTime * 300f);
        movementInput = Input.GetAxis("Vertical") * Mathf.Abs(Input.GetAxisRaw("Vertical"));
    }
}
