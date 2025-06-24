using PurrNet.StateMachine;
using UnityEngine;

public class StateOne : StateNode
{
    [SerializeField] private bool _force;
    [SerializeField] private StateNode _stateToAdd;
    [SerializeField] private int _indexTest;
    
    protected override void OnSpawned()
    {
        base.OnSpawned();

        machine.onStateChanged += OnStateChanged;
    }

    private void OnStateChanged(StateNode previousState, StateNode newState)
    {
        //Debug.Log($"Changed from state {previousState} to state {newState}");
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

    [ContextMenu("Add state")]
    private void AddState()
    {
        machine.AddState(_stateToAdd);
    }
    
    [ContextMenu("Remove state")]
    private void RemoveState()
    {
        machine.RemoveState(_stateToAdd);
    }
    
    [ContextMenu("Insert state")]
    private void InsertState()
    {
        machine.InsertState(_stateToAdd, _indexTest);
    }
    
    [ContextMenu("Remove state at index")]
    private void RemoveStateAtIndex()
    {
        machine.RemoveStateAt(_indexTest);
    }
}
