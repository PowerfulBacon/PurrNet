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
    public class SyncArray<T> : NetworkModule, IList<T>, ISerializationCallbackReceiver
    {
        [SerializeField] private bool _ownerAuth;
        [SerializeField] private List<T> _serializedItems = new List<T>();
        [SerializeField] private int _length;
        
        private T[] _array;

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
            _length = length;
            _array = new T[length];
            _serializedItems = new List<T>(length);
            for (int i = 0; i < length; i++)
                _serializedItems.Add(default);
        }
        
        public void OnBeforeSerialize()
        {
            _serializedItems.Clear();
            for (int i = 0; i < _length && i < _array.Length; i++)
            {
                _serializedItems.Add(_array[i]);
            }
        }
        
        public void OnAfterDeserialize()
        {
            _array = new T[_length];
            
            for (int i = 0; i < _serializedItems.Count && i < _length; i++)
                _array[i] = _serializedItems[i];
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

        public override void OnInitializeModules()
        {
            base.OnInitializeModules();
            if (!IsController(_ownerAuth)) return;
            
            if (isServer)
                SendInitialSizeToAll(_length);
            else 
                SendInitialSizeToServer(_length);
                
            for (int i = 0; i < _length; i++)
            {
                if (isServer)
                    SendSetToAll(i, _array[i]);
                else
                    SendSetToServer(i, _array[i]);
            }
        }

        public override void OnObserverAdded(PlayerID player)
        {
            SendInitialSizeToTarget(player, _length);
            
            for (int i = 0; i < _length; i++)
            {
                SendSetToTarget(player, i, _array[i]);
            }
        }

        [TargetRpc(Channel.ReliableOrdered)]
        private void SendInitialSizeToTarget(PlayerID player, int length)
        {
            HandleInitialSize(length);
        }
        
        [TargetRpc(Channel.ReliableOrdered)]
        private void SendSetToTarget(PlayerID player, int index, T value)
        {
            if (index >= 0 && index < _length)
            {
                _array[index] = value;
                InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Set, value, index));
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendInitialSizeToAll(int length)
        {
            HandleInitialSize(length);
        }

        private void HandleInitialSize(int length)
        {
            if (!isHost)
            {
                if (_length != length)
                {
                    Array.Resize(ref _array, length);
                    _length = length;
                    
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Resized));
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Cleared));
                }
            }
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendInitialSizeToServer(int length)
        {
            if (!_ownerAuth) return;
            SendInitialSizeToOthers(length);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendInitialSizeToOthers(int length)
        {
            if (!isServer || isHost)
            {
                if (_length != length)
                {
                    Array.Resize(ref _array, length);
                    _length = length;
                    
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Resized));
                    InvokeChange(new SyncArrayChange<T>(SyncArrayOperation.Cleared));
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