using PurrNet.StateMachine;
using UnityEngine;

public class StateOne : StateNode
{
    [SerializeField] private bool _force;
    
    protected override void OnSpawned()
    {
        base.OnSpawned();

        machine.onStateChanged += OnStateChanged;
    }

    private void OnStateChanged(StateNode previousState, StateNode newState)
    {
        Debug.Log($"Changed from state {previousState} to state {newState}");
    }

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

    [ContextMenu("Next state")]
    private void NextState()
    {
        var goesNext = machine.Next(_force);
        Debug.Log($"Went to next state (forced: {_force}) and was successful: {goesNext}");
    }
}
