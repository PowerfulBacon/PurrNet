using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

[DontPack]
public class SafeList<T> : IEnumerable<T>
{
    private List<T> _list = new();
    private List<QueueItem> _queue = new();

    public void Add(T item)
    {
        _queue.Add(new QueueItem(item, Instruction.Add));
    }

    public void Remove(T item)
    {
        _queue.Add(new QueueItem(item, Instruction.Remove));
    }

    public void UpdateList()
    {
        foreach (var item in _queue)
        {
            switch (item.instruction)
            {
                case Instruction.Add:
                    _list.Add(item.item);
                    break;
                case Instruction.Remove:
                    _list.Remove(item.item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _queue.Clear();
    }

    private readonly struct QueueItem : IEquatable<QueueItem>
    {
        public readonly T item;
        public readonly Instruction instruction;

        public bool Equals(QueueItem other) => EqualityComparer<T>.Default.Equals(item, other.item) && instruction == other.instruction;
        public override bool Equals(object? obj) => obj is QueueItem other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(item, (int)instruction);

        public QueueItem(T item, Instruction instruction)
        {
            this.item = item;
            this.instruction = instruction;
        }
    }

    private enum Instruction
    {
        Add,
        Remove
    }

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
