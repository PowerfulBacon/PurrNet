using JetBrains.Annotations;
using PurrNet.Packing;
using UnityEditor;
using UnityEngine;

namespace PurrNet.Profiler.Deltas
{
    [UsedImplicitly]
    public class NormalizedFloatProfile : DeltaProfiler<NormalizedFloat>
    {
        public override void EvaluateValue(int index, BitPacker packer, EvaluationMode mode)
        {
            using (new ScopedRandom(index * 1000))
            {
                float lerp = (float)index / (MAX_ITERATIONS - 1);
                NormalizedFloat value = mode switch
                {
                    EvaluationMode.PerlinNoise => Mathf.PerlinNoise(index * 0.01f, (float)EditorApplication.timeSinceStartup * 0.5f),
                    EvaluationMode.Linear => lerp * 2f - 1f,
                    EvaluationMode.Quadratic => index * index * 0.01f,
                    EvaluationMode.Cubic => index * index * index * 0.001f,
                    EvaluationMode.Exponential => Mathf.Pow(2, index) * 0.01f,
                    EvaluationMode.Random => Random.Range(0.0f, 1.0f),
                    _ => 0.0f
                };

                Packer<NormalizedFloat>.Write(packer, value);
            }
        }

        public override double EvaluateHeight(int a, EvaluationMode mode)
        {
            return Evaluate(a, mode);
        }

        public override string ToString(int a, EvaluationMode mode)
        {
            return Evaluate(a, mode).GetValue().ToString("F3");
        }
    }
}
