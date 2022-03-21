using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputModel : MonoBehaviour
{

    protected CharController controls;
    public CharController _Controls { set => controls = value; }

    protected float rotationInput;
    protected float movementInput;
    protected Vector3 moveVector;
    protected bool walking;


    public float _RotationInput { get { return rotationInput; } }
    public float _MovementInput { get { return movementInput; } }

    public abstract bool _Walking();
    public abstract bool _Jumping();
    public abstract bool _MouthOpen();
    public abstract void Idle();
    public abstract void Walk();
    public abstract Quaternion Swim();
    public abstract void Fall();
    public abstract void Jump();
    public abstract int _Spinning();
    public abstract void UpdateInputs();


}
