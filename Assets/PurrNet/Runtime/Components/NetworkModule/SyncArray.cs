using UnityEngine;
using PurrNet.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet.Transports;

namespace PurrNet
{
    public enum SyncArrayOperation
    {
        Set,
        Cleared,
        Resized
    }

    public readonly struct SyncArrayChange<T>
    {
        public readonly SyncArrayOperation operation;
        public readonly T value;
        public readonly int index;

        public SyncArrayChange(SyncArrayOperation operation, T value = default, int index = -1)
        {
            this.operation = operation;
            this.value = value;
            this.index = index;
        }

        public override string ToString()
        {
            string valueStr = $"Value: {value} | Operation: {operation} | Index: {index}";
            return valueStr;
        }
    }

    [Serializable]
    public class SyncArray<T> : NetworkModule, IList<T>
    {
        [SerializeField] private bool _ownerAuth;
        [SerializeField] private T[] _array;
        [SerializeField] private int _length;

        public delegate void SyncArrayChanged<TYPE>(SyncArrayChange<TYPE> change);

        public event SyncArrayChanged<T> onChanged;

        public bool ownerAuth => _ownerAuth;
        
        public int Length
        {
            get => _length;
            set
            {
                ValidateAuthority();
                
                if (_length == value)
                    return;
                
                Array.Resize(ref _array, value);
                _length = value;
                
                var change = new SyncArrayChange<T>(SyncArrayOperation.Resized);
                InvokeChange(change);
                
                if (isSpawned)
                {
                    if (isServer)
                        SendResizeToAll(value);
                    else
                        SendResizeToServer(value);
                }
            }
        }

        public int Count => _length;
        public bool IsReadOnly => false;

        public SyncArray(int length = 0, bool ownerAuth = false)
        {
            _ownerAuth = ownerAuth;
            _array = new T[length];
            _length = length;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                    throw new IndexOutOfRangeException();
                    
                return _array[index];
            }
            set
            {
                ValidateAuthority();
                
                if (index < 0 || index >= _length)
                    throw new IndexOutOfRangeException();
                
                bool bothNull = value == null && _array[index] == null;
                bool bothEqual = value != null && value.Equals(_array[index]);
                
                if (bothNull || bothEqual)
                    return;
                
                _array[index] = value;
                
                var change = new SyncArrayChange<T>(SyncArrayOperation.Set, value, index);
                InvokeChange(change);
                
                if (isSpawned)
                {
                    if (isServer)
                        SendSetToAll(index, value);
                    else
                        SendSetToServer(index, value);
                }
            }
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            
            if (!IsController(_ownerAuth)) return;
            
            if (isServer)
                SendInitialStateToAll(_array, _length);
            else 
                SendInitialStateToServer(_array, _length);
        }

        public override void OnObserverAdded(PlayerID player)
        {
            SendInitialToTarget(player, _array, _length);
        }

        [TargetRpc(Channel.ReliableOrdered)]
        private void SendInitialToTarget(PlayerID player, T[] initialArray, int length)
        {
            HandleInitialState(initialArray, length);
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendInitialStateToAll(T[] initialArray, int length)
        {
            HandleInitialState(initialArray, length);
        }

        private void HandleInitialState(T[] initialArray, int length)
        {
            if (!isHost)
            {
                if (initialArray == null)
                    return;
                
                _array = new T[initialArray.Length];
                Array.Copy(initialArray, _array, Math.Min(initialArray.Length, length));
                _length = length;
                
                InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Cleared));
                
                for (int i = 0; i < _length; i++)
                {
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Set, _array[i], i));
                }
            }
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendInitialStateToServer(T[] initialArray, int length)
        {
            if (!_ownerAuth) return;
            SendInitialStateToOthers(initialArray, length);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendInitialStateToOthers(T[] initialArray, int length)
        {
            if (!isServer || isHost)
            {
                _array = new T[initialArray.Length];
                Array.Copy(initialArray, _array, Math.Min(initialArray.Length, length));
                _length = length;
                
                InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Cleared));
                
                for (int i = 0; i < _length; i++)
                {
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Set, _array[i], i));
                }
            }
        }

        public void Clear()
        {
            ValidateAuthority();
            
            Array.Clear(_array, 0, _length);
            
            var change = new SyncArrayChange<T>(SyncArrayOperation.Cleared);
            InvokeChange(change);
            
            if (isSpawned)
            {
                if (isServer)
                    SendClearToAll();
                else
                    SendClearToServer();
            }
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < _length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_array[i], item))
                    return i;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException("SyncArray does not support insertion. Use Resize and Set operations instead.");
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException("SyncArray does not support removal. Use Resize and Set operations instead.");
        }

        public void Add(T item)
        {
            throw new NotSupportedException("SyncArray does not support Add. Use Resize and Set operations instead.");
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException("SyncArray does not support Remove. Use Resize and Set operations instead.");
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
                
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                
            if (array.Length - arrayIndex < _length)
                throw new ArgumentException("Destination array is not long enough");
                
            Array.Copy(_array, 0, array, arrayIndex, _length);
        }

        public T[] ToArray()
        {
            T[] result = new T[_length];
            Array.Copy(_array, result, _length);
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _length; i++)
            {
                yield return _array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SetDirty(int index)
        {
            if (!isSpawned) return;
            
            ValidateAuthority();
            
            if (index < 0 || index >= _length)
            {
                PurrLogger.LogError($"Invalid index {index} for SetDirty in SyncArray. Array length: {_length}",
                    parent);
                return;
            }
            
            var value = _array[index];
            InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Set, value, index));
            
            if (isServer)
                SendSetDirtyToAll(index, value);
            else
                SendSetDirtyToServer(index, value);
        }

        private void ValidateAuthority()
        {
            if (!isSpawned) return;
            
            bool isController = parent.IsController(_ownerAuth);
            if (!isController)
            {
                PurrLogger.LogError(
                    $"Invalid permissions when modifying '<b>SyncArray<{typeof(T).Name}> {name}</b>' on '{parent.name}'." +
                    $"\nMaybe try enabling owner authority.", parent);
                throw new InvalidOperationException("Invalid permissions");
            }
        }

        private void InvokeChange(SyncArrayChange<T> change)
        {
            onChanged?.Invoke(change);
        }

        #region RPCs

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendSetToServer(int index, T value)
        {
            if (!_ownerAuth) return;
            SendSetToOthers(index, value);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendSetToOthers(int index, T value)
        {
            if (!isServer || isHost)
            {
                if (index >= 0 && index < _length)
                {
                    _array[index] = value;
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Set, value, index));
                }
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendSetToAll(int index, T value)
        {
            if (!isHost)
            {
                if (index >= 0 && index < _length)
                {
                    _array[index] = value;
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Set, value, index));
                }
            }
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendClearToServer()
        {
            if (!_ownerAuth) return;
            SendClearToOthers();
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendClearToOthers()
        {
            if (!isServer || isHost)
            {
                Array.Clear(_array, 0, _length);
                InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Cleared));
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendClearToAll()
        {
            if (!isHost)
            {
                Array.Clear(_array, 0, _length);
                InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Cleared));
            }
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendResizeToServer(int newLength)
        {
            if (!_ownerAuth) return;
            SendResizeToOthers(newLength);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendResizeToOthers(int newLength)
        {
            if (!isServer || isHost)
            {
                if (_length != newLength)
                {
                    Array.Resize(ref _array, newLength);
                    _length = newLength;
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Resized));
                }
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendResizeToAll(int newLength)
        {
            if (!isHost)
            {
                if (_length != newLength)
                {
                    Array.Resize(ref _array, newLength);
                    _length = newLength;
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Resized));
                }
            }
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendSetDirtyToServer(int index, T value)
        {
            if (!_ownerAuth) return;
            SendSetDirtyToOthers(index, value);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendSetDirtyToOthers(int index, T value)
        {
            if (!isServer || isHost)
            {
                if (index >= 0 && index < _length)
                {
                    _array[index] = value;
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Set, value, index));
                }
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendSetDirtyToAll(int index, T value)
        {
            if (!isHost)
            {
                if (index >= 0 && index < _length)
                {
                    _array[index] = value;
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Set, value, index));
                }
            }
        }

        #endregion
    }
}