using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Transition
   
   
{
    public State toState;
    public Func<bool> Decision;

    public Transition(State toState, Func<bool> Decision)
    {
        this.toState = toState;
        this.Decision = Decision;
    }

}
