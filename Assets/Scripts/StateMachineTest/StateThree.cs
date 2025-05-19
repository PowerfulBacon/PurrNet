using System;
using PurrNet.StateMachine;
using UnityEngine;

public class StateThree : StateNode
{
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        Debug.Log($"Entered state {this} asServer: {asServer}");
        if(isController && !asServer)
            machine.Next();
    }
    
    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
        
        Debug.Log($"Exited state {this} asServer: {asServer}");
    }
}
