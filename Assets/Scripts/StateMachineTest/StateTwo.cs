using PurrNet.StateMachine;
using UnityEngine;

public class StateTwo : StateNode<int>
{
    [SerializeField] private bool canEnter;
    
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        
        //Debug.Log($"Entered state {this} asServer: {asServer}");
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
        
        //Debug.Log($"Exited state {this} asServer: {asServer}");
    }

    public override bool CanEnter()
    {
        return canEnter;
    }

    [ContextMenu("Next state")]
    private void NextState()
    {
        var goesNext = machine.Next();
        Debug.Log($"{goesNext}");
    }
}
