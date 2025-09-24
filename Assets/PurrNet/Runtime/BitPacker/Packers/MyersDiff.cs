using System;
using System.Collections.Generic;
using PurrNet.Pooling;

namespace PurrNet.Packing
{
    public enum OperationType
    {
        Add,      // append only, no index
        Insert,   // insert at position
        Delete   // range delete
    }

    public readonly struct DiffOp<T> : IDisposable
    {
        public readonly OperationType Type;
        public readonly int Index;       // for Insert/Delete/Replace
        public readonly int Length;      // for Delete (count) / Replace (old length)
        public readonly DisposableList<T>? Values; // for Insert/Replace/Add

        public DiffOp(OperationType type, int index, int length, DisposableList<T>? values = null)
        {
            Type = type;
            Index = index;
            Length = length;
            Values = values;
        }

        public override string ToString()
        {
            return Type switch
            {
                OperationType.Delete => $"Delete {Length} at [{Index}]",
                OperationType.Insert => $"Insert {{{string.Join(",", Values!.Value)}}} at [{Index}]",
                OperationType.Add => $"Add {{{string.Join(",", Values!.Value)}}}",
                _ => base.ToString()
            };
        }

        public void Dispose()
        {
            Values?.Dispose();
        }
    }

    public static class MyersDiff
    {
        public static DisposableList<DiffOp<T>> Diff<T>(IReadOnlyList<T> a, IReadOnlyList<T> b)
        {
            int n = a.Count, m = b.Count;
            int max = n + m;
            int size = 2 * max + 1;       // total array size per wavefront

            var trace = DisposableList<DisposableArray<int>>.Create(max + 1);
            var v = DisposableArray<int>.Create(size);

            // Forward search
            for (int d = 0; d <= max; d++)
            {
                trace.Add(DisposableArray<int>.Create(v));

                for (int k = -d; k <= d; k += 2)
                {
                    int kIndex = k + max;
                    int x;
                    if (k == -d || (k != d && v[kIndex - 1] < v[kIndex + 1]))
                        x = v[kIndex + 1];   // down
                    else
                        x = v[kIndex - 1] + 1; // right

                    int y = x - k;
                    // follow diagonal (the "snake")
                    while (x < n && y < m && EqualityComparer<T>.Default.Equals(a[x], b[y]))
                    {
                        x++;
                        y++;
                    }

                    v[kIndex] = x;

                    if (x >= n && y >= m)
                    {
                        var res = Backtrack(a, b, trace, d, max);
                        v.Dispose();
                        for (var i = 0; i < trace.Count; i++)
                            trace[i].Dispose();
                        trace.Dispose();
                        return res;
                    }
                }
            }

            v.Dispose();
            for (var i = 0; i < trace.Count; i++)
                trace[i].Dispose();
            trace.Dispose();
            return DisposableList<DiffOp<T>>.Create();
        }

        private static DisposableList<DiffOp<T>> Backtrack<T>(
            IReadOnlyList<T> a,
            IReadOnlyList<T> b,
            DisposableList<DisposableArray<int>> trace,
            int d,
            int offset)
        {
            var elementOps = DisposableList<DiffOp<T>>.Create();
            int x = a.Count;
            int y = b.Count;

            for (int depth = d; depth >= 0; depth--)
            {
                var v = trace[depth];
                int k = x - y;
                int kIdx = k + offset;

                int prevK, prevX;
                bool down;
                if (k == -depth || (k != depth && v[kIdx - 1] < v[kIdx + 1]))
                {
                    // down (insert)
                    prevK = k + 1;
                    prevX = v[prevK + offset];
                    down = true;
                }
                else
                {
                    // right (delete)
                    prevK = k - 1;
                    prevX = v[prevK + offset] + 1;
                    down = false;
                }

                int prevY = prevX - prevK;

                // move along diagonal (matches)
                while (x > prevX && y > prevY)
                {
                    x--;
                    y--;
                }

                if (depth > 0)
                {
                    if (down)
                    {
                        y--;
                        var values = DisposableList<T>.Create();
                        values.Add(b[y]);
                        elementOps.Add(new DiffOp<T>(
                            x == a.Count ? OperationType.Add : OperationType.Insert,
                            x,
                            0,
                            values));
                    }
                    else
                    {
                        x--;
                        elementOps.Add(new DiffOp<T>(OperationType.Delete, x, 1, null));
                    }
                }
            }

            elementOps.Reverse();

            // merge to ranges/Replace
            return MergeOps(elementOps);
        }

        private static DisposableList<DiffOp<T>> MergeOps<T>(DisposableList<DiffOp<T>> ops)
        {
            var result = DisposableList<DiffOp<T>>.Create();

            for (int i = 0; i < ops.Count; i++)
            {
                var op = ops[i];

                switch (op.Type)
                {
                    // Merge consecutive Deletes
                    case OperationType.Delete:
                    {
                        int idx = op.Index;
                        int len = op.Length;
                        while (i + 1 < ops.Count && ops[i + 1].Type == OperationType.Delete &&
                               ops[i + 1].Index == idx + len)
                        {
                            len += ops[i + 1].Length;
                            i++;
                        }

                        result.Add(new DiffOp<T>(OperationType.Delete, idx, len, null));
                        continue;
                    }
                    // Merge Inserts/Adds
                    case OperationType.Insert:
                    case OperationType.Add:
                    {
                        var vals = DisposableList<T>.Create(op.Values!);
                        int idx = op.Index;
                        bool isAdd = op.Type == OperationType.Add;

                        while (i + 1 < ops.Count &&
                               ops[i + 1].Type == op.Type &&
                               (isAdd || ops[i + 1].Index == idx + vals.Count))
                        {
                            vals.AddRange(ops[i + 1].Values!);
                            i++;
                        }

                        result.Add(new DiffOp<T>(op.Type, idx, 0, vals));
                        continue;
                    }
                    default:
                        // Just copy others
                        result.Add(op);
                        break;
                }
            }

            return result;
        }
    }
}
