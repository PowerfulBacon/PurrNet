using System;
using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Packing;
using UnityEngine;

namespace PurrNet.StateMachine
{
    [DefaultExecutionOrder(-1000)]
    public sealed class StateMachine : NetworkBehaviour
    {
        [SerializeField] private bool ownerAuth = false;

        [SerializeField] List<StateNode> _states;

        public IReadOnlyList<StateNode> states => _states;

        /// <summary>
        /// Invoked for clients when receiving changes to the state machine from the server
        /// </summary>
        public event Action onReceivedNewData;

        /// <summary>
        /// Invoked for both server and client when state changes
        /// </summary>
        public event StateChangedDelegate onStateChanged;
        public delegate void StateChangedDelegate(StateNode previousState, StateNode newState);
        

        StateMachineState _currentState;
        private int _previousStateId = -1;

        public StateMachineState currentState => _currentState;
        public int previousStateId => _previousStateId;

        public StateNode currentStateNode => _currentState.stateId < 0 || _currentState.stateId >= _states.Count
            ? null
            : _states[_currentState.stateId];
        
        public StateNode previousStateNode => _previousStateId < 0 || _previousStateId >= _states.Count
            ? null
            : _states[_previousStateId];

        private bool _initialized;

        private void Awake()
        {
            _currentState.stateId = -1;

            for (var i = 0; i < _states.Count; i++)
            {
                var state = _states[i];
                state.Setup(this);
            }
        }

        private void Update()
        {
            if (_currentState.stateId < 0 || _currentState.stateId >= _states.Count)
                return;

            var node = _states[_currentState.stateId];
            node.StateUpdate(isServer);
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            
            if (!IsController(ownerAuth))
                return;

            if (_initialized)
                return;

            if (_states.Count > 0)
                SetState(_states[0]);

            _initialized = true;
        }

        protected override void OnObserverAdded(PlayerID player)
        {
            base.OnObserverAdded(player);
            
            if (!isServer)
                return;
            
            if (_currentState.stateId < 0 || _currentState.stateId >= _states.Count)
                return;
            
            var stateNode = _states[_currentState.stateId];
            SendStateToObserver(player, stateNode);
        }

        private void SendStateToObserver(PlayerID player, StateNode stateNode)
        {
            Type dataType = GetDataType(_currentState.stateId);
    
            if (dataType != null && _currentState.data != null && dataType.IsInstanceOfType(_currentState.data))
            {
                RpcStateChange_Target(player, _currentState, true, _currentState.data);
            }
            else
            {
                RpcStateChange_Target<ushort>(player, _currentState, false, 0);
            }
        }

        public Type GetDataType(int stateId)
        {
            if (stateId < 0 || stateId >= _states.Count)
                return null;

            var node = _states[stateId];
            var type = node.GetType();
            var generics = type.BaseType!.GenericTypeArguments;

            return generics.Length == 0 ? null : generics[0];
        }

        [ServerRpc]
        private void RpcStateChange_Server<T>(StateMachineState state, bool hasData, T data)
        {
            RpcStateChange<T>(state, hasData, data);
        }

        [ObserversRpc(bufferLast: true)]
        private void RpcStateChange<T>(StateMachineState state, bool hasData, T data)
        {
            if (IsController(ownerAuth)) return;

            var activeState = _currentState.stateId < 0 || _currentState.stateId >= _states.Count
                ? null
                : _states[_currentState.stateId];

            try
            {
                if (activeState != null)
                {
                    if (isServer)
                        activeState.Exit(true);
                    if (isClient)
                        activeState.Exit(false);
                }
            }
            catch(Exception e)
            {
                PurrLogger.LogException(e);
            }
            

            _currentState = state;
            _currentState.data = data;

            if (_currentState.stateId < 0 || _currentState.stateId >= _states.Count)
                return;

            var newState = _states[_currentState.stateId];
            var prevState = previousStateNode;

            try
            {
                if (hasData && newState is StateNode<T> node)
                {
                    if(isServer)
                        node.Enter(data, true);
                    if(isClient)
                        node.Enter(data, false);
                }
                else
                {
                    if(isServer)
                        newState.Enter(true);
                    if(isClient)
                        newState.Enter(false);
                }
            }
            catch(Exception e)
            {
                PurrLogger.LogException(e);
            }

            onStateChanged?.Invoke(prevState, newState);
            onReceivedNewData?.Invoke();
        }
        
        [TargetRpc]
        private void RpcStateChange_Target<T>(PlayerID target, StateMachineState state, bool hasData, T data)
        {
            if (IsController(ownerAuth)) return;

            _currentState = state;
            _currentState.data = data;

            if (_currentState.stateId < 0 || _currentState.stateId >= _states.Count)
                return;

            var newState = _states[_currentState.stateId];
            var prevState = previousStateNode;

            try
            {
                if (hasData && newState is StateNode<T> node)
                {
                    node.Enter(data, false);
                }
                else
                {
                    newState.Enter(false);
                }
            }
            catch(Exception e)
            {
                PurrLogger.LogException(e);
            }

            onStateChanged?.Invoke(prevState, newState);
            onReceivedNewData?.Invoke();
        }

        private void UpdateStateId(StateNode node)
        {
            var idx = node == null ? -2 : _states.IndexOf(node);

            if (idx == -1)
                PurrLogger.LogException($"State '{node.name}' of type {node.GetType().Name} not in states list");

            var newStateId = idx < 0 ? -1 : idx;

            var oldState = _currentState.stateId < 0 || _currentState.stateId >= _states.Count
                ? null
                : _states[_currentState.stateId];

            try
            {
                if (oldState)
                {
                    if(isServer)
                        oldState.Exit(true);
                    if (isClient)
                        oldState.Exit(false);
                }
            }
            catch (Exception e)
            {
                PurrLogger.LogException(e);
            }

            _previousStateId = _currentState.stateId;
            _currentState.stateId = newStateId;
        }

        /// <summary>
        /// Goes to a specific state in the StateMachine list
        /// </summary>
        /// <param name="state">Reference to the state you want to go to</param>
        /// <param name="data">Data to send with the state</param>
        /// <typeparam name="T">Your data type</typeparam>
        public void SetState<T>(StateNode<T> state, T data)
        {
            if (!IsController(ownerAuth))
            {
                PurrLogger.LogError(
                    $"Only the controller can set state. Non-owner tried to set state to {state.name}:{state.GetType().Name} | OwnerAuth: {ownerAuth}"
                );
                return;
            }

            UpdateStateId(state);
            _currentState.data = data;
            
            var newState = _states[_currentState.stateId];
            var prevState = previousStateNode;

            if (isServer)
                RpcStateChange(_currentState, true, data);
            else
                RpcStateChange_Server(_currentState, true, data);

            try
            {
                if (state)
                {
                    if(isServer)
                        state.Enter(data, true);
                    if(isClient)
                        state.Enter(data, false);
                }
            }
            catch (Exception e)
            {
                PurrLogger.LogException(e);
            }

            onStateChanged?.Invoke(prevState, newState);
        }

        /// <summary>
        /// Goes to a specific state in the StateMachine list
        /// </summary>
        /// <param name="state">Reference to the state you want to go to</param>
        public void SetState(StateNode state)
        {
            if (!IsController(ownerAuth))
            {
                PurrLogger.LogError(
                    $"Only the controller can set state. Non-owner tried to set state to {state.name}:{state.GetType().Name} | OwnerAuth: {ownerAuth}"
                );
                return;
            }

            UpdateStateId(state);
            _currentState.data = null;

            var newState = _states[_currentState.stateId];
            var prevState = previousStateNode;
            
            if (isServer)
                RpcStateChange<ushort>(_currentState, false, 0);
            else
                RpcStateChange_Server<ushort>(_currentState, false, 0);

            try
            {
                if (state)
                {
                    if(isServer)
                        state.Enter(true);
                    if(isClient)
                        state.Enter(false);
                }
            }
            catch (Exception e)
            {
                PurrLogger.LogException(e);
            }

            onStateChanged?.Invoke(prevState, newState);
        }

        /// <summary>
        /// Takes the state machine to the next state in the states list
        /// </summary>
        /// <param name="data">Data to send with the state</param>
        /// <typeparam name="T">The type of your data</typeparam>
        public void Next<T>(T data)
        {
            var nextNodeId = _currentState.stateId + 1;
            if (nextNodeId >= _states.Count)
                nextNodeId = 0;

            var nextNode = _states[nextNodeId];

            if (nextNode is StateNode<T> stateNode)
            {
                SetState(stateNode, data);
            }
            else
            {
                PurrLogger.LogException(
                    $"Node {nextNode.name}:{nextNode.GetType().Name} does not have a generic type argument of type {typeof(T).Name}");
            }
        }

        /// <summary>
        /// Takes the state machine to the next state in the states list
        /// </summary>
        public void Next()
        {
            var nextNodeId = _currentState.stateId + 1;
            if (nextNodeId >= _states.Count)
                nextNodeId = 0;

            SetState(_states[nextNodeId]);
        }

        /// <summary>
        /// Takes the state machine to the previous state in the states list
        /// </summary>
        public void Previous()
        {
            var prevNodeId = _currentState.stateId - 1;
            if (prevNodeId < 0)
                prevNodeId = _states.Count - 1;

            SetState(_states[prevNodeId]);
        }

        /// <summary>
        /// Takes the state machine to the previous state in the states list
        /// </summary>
        /// <param name="data">Data to send to the previous state</param>
        /// <typeparam name="T">The type of your data</typeparam>
        public void Previous<T>(T data)
        {
            var prevNodeId = _currentState.stateId - 1;
            if (prevNodeId < 0)
                prevNodeId = _states.Count - 1;

            var prevNode = _states[prevNodeId];

            if (prevNode is StateNode<T> stateNode)
            {
                SetState(stateNode, data);
            }
            else
            {
                PurrLogger.LogException(
                    $"Node {prevNode.name}:{prevNode.GetType().Name} does not have a generic type argument of type {typeof(T).Name}");
            }
        }
    }

    public struct StateMachineState : IPacked
    {
        public int stateId;
        public object data;

        public void Write(BitPacker packer)
        {
            Packer<int>.Write(packer, stateId);
            Packer<object>.Write(packer, data);
        }

        public void Read(BitPacker packer)
        {
            Packer<int>.Read(packer, ref stateId);
            Packer<object>.Read(packer, ref data);
        }
    }
}