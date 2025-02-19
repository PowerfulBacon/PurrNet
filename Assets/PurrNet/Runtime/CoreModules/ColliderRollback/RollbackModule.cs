using System.Collections.Generic;
using PurrNet.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrNet.Modules
{
    public class RollbackModule : INetworkModule
    {
        PhysicsScene _physicsScene;

        readonly TickManager _tickManager;
        readonly HashSet<Component> _trackedColliders = new ();
        
        private readonly List<Collider> _colliders3D = new ();
        private readonly List<Collider2D> _colliders2D = new ();
        
        readonly Dictionary<Collider, SimpleHistory<Collider3DState>> _collider3DStates = new ();
        readonly Dictionary<Collider2D, SimpleHistory<Collider2DState>> _collider2DStates = new ();

        public RollbackModule(TickManager tick, Scene scene)
        {
            _tickManager = tick;
            _physicsScene = scene.GetPhysicsScene();
        }
        
        public void Enable(bool asServer)
        {
        }

        public void Disable(bool asServer)
        {
        }
        
        static readonly RaycastHit[] _raycastHits = new RaycastHit[1024];
        
        /// <summary>
        /// Casts a ray, from point origin, in direction direction, of length maxDistance, against all colliders in the scene.
        /// </summary>
        public int Raycast(double preciseTick, Ray ray, RaycastHit[] raycastHits, 
            float maxDistance = float.PositiveInfinity, 
            int layerMask = Physics.AllLayers,
            QueryTriggerInteraction queryTriggers = QueryTriggerInteraction.UseGlobal)
        {
            if (!_physicsScene.IsValid())
                return 0;
            
            int hitCount = _physicsScene.Raycast(ray.origin, ray.direction, raycastHits, maxDistance, layerMask, queryTriggers);
            
            uint tick = (uint)preciseTick;
            uint tickNext = tick + 1;
            float tickFraction = (float)(preciseTick - tick);
            
            int colliderCount = _colliders3D.Count;
            
            // remove any colliders that we are handling manually
            hitCount = FilterColliders(hitCount, raycastHits);
            
            // handle raycast hits manually
            hitCount = DoManualRaycasts(ray, raycastHits, maxDistance, layerMask, colliderCount, hitCount, tick, tickNext, tickFraction, queryTriggers);
            
            return hitCount;
        }
        
        public bool TryGetColliderState(double preciseTick, Collider collider, out Collider3DState state)
        {
            if (_collider3DStates.TryGetValue(collider, out var history))
            {
                uint tick = (uint)preciseTick;
                uint tickNext = tick + 1;
                float tickFraction = (float)(preciseTick - tick);

                bool hasStateA = history.TryGet(tick, out var stateA);
                bool hasStateB = history.TryGet(tickNext, out var stateB);
                
                switch (hasStateA)
                {
                    case true when hasStateB:
                        stateA = stateA.Interpolate(stateB, tickFraction);
                        break;
                    case false when hasStateB:
                        stateA = stateB;
                        break;
                    case false:
                    {
                        state = default;
                        return false;
                    }
                    case true:
                        break;
                }
                
                state = stateA;
                return true;
            }
            
            state = default;
            return false;
        }

        /// <summary>
        /// Casts a ray, from point origin, in direction direction, of length maxDistance, against all colliders in the scene.
        /// </summary>
        public bool Raycast(double preciseTick, Ray ray, out RaycastHit hit, 
            float maxDistance = float.PositiveInfinity, 
            int layerMask = Physics.AllLayers,
            QueryTriggerInteraction queryTriggers = QueryTriggerInteraction.UseGlobal)
        {
            if (!_physicsScene.IsValid())
            {
                hit = default;
                return false;
            }
            
            int hitCount = Raycast(preciseTick, ray, _raycastHits, maxDistance, layerMask, queryTriggers);
            
            // return the closest hit
            if (hitCount > 0)
            {
                hit = _raycastHits[0];
                for (var i = 1; i < hitCount; i++)
                {
                    if (_raycastHits[i].distance < hit.distance)
                        hit = _raycastHits[i];
                }
                return true;
            }
            
            hit = default;
            return false;
        }

        private int DoManualRaycasts(Ray ray, RaycastHit[] hits, float maxDistance, int layerMask, int colliderCount,
            int hitCount, uint tick, uint tickNext, float tickFraction, QueryTriggerInteraction queryTriggers)
        {
            if (queryTriggers == QueryTriggerInteraction.UseGlobal)
                queryTriggers = Physics.queriesHitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
            
            for (var i = 0; i < colliderCount; i++)
            {
                if (hitCount >= hits.Length)
                    break;
                
                var col = _colliders3D[i];
                
                if (!col)
                    continue;
                
                if (col.isTrigger && queryTriggers == QueryTriggerInteraction.Ignore)
                    continue;
                
                bool isPartOfLayerMask = ((1 << col.gameObject.layer) & layerMask) != 0;
                
                if (!isPartOfLayerMask)
                    continue;
                
                if (!_collider3DStates.TryGetValue(col, out var history))
                    continue;

                bool hasStateA = history.TryGet(tick, out var stateA);
                bool hasStateB = history.TryGet(tickNext, out var stateB);

                switch (hasStateA)
                {
                    case true when hasStateB:
                        stateA = stateA.Interpolate(stateB, tickFraction);
                        break;
                    case false when hasStateB:
                        stateA = stateB;
                        break;
                    case false: continue;
                    case true:
                        break;
                }
                
                if (!stateA.enabled)
                    continue;
                
                var trs = col.transform;

                // Get the transform matrix for the historical position
                var historicalWorldMatrix = Matrix4x4.TRS(stateA.position, stateA.rotation, stateA.scale);
                var worldToHistorical = historicalWorldMatrix.inverse;

                // Transform world ray to historical local space
                var rayHistoricalLocal = new Ray(
                    worldToHistorical.MultiplyPoint3x4(ray.origin), 
                    worldToHistorical.MultiplyVector(ray.direction)
                );

                // Transform historical local ray to current world space for the actual raycast
                var currentWorldMatrix = trs.localToWorldMatrix;
                var rayCurrentWorld = new Ray(
                    currentWorldMatrix.MultiplyPoint3x4(rayHistoricalLocal.origin),
                    currentWorldMatrix.MultiplyVector(rayHistoricalLocal.direction)
                );

                if (col.Raycast(rayCurrentWorld, out var hit, maxDistance))
                {
                    // Transform hit from current world space to current local space
                    var currentToLocal = trs.worldToLocalMatrix;
                    hit.point = currentToLocal.MultiplyPoint3x4(hit.point);
                    hit.normal = currentToLocal.MultiplyVector(hit.normal);
    
                    // Transform hit from current local space to historical world space
                    hit.point = historicalWorldMatrix.MultiplyPoint3x4(hit.point);
                    hit.normal = historicalWorldMatrix.MultiplyVector(hit.normal);
    
                    hits[hitCount++] = hit;
                }
            }

            return hitCount;
        }

        private int FilterColliders(int hitCount, RaycastHit[] hits)
        {
            for (var i = 0; i < hitCount; i++)
            {
                var col = hits[i].collider;
                if (col && _trackedColliders.Contains(col))
                    hits[i--] = hits[--hitCount];
            }

            return hitCount;
        }

        public void OnPostTick()
        {
            for (var i = 0; i < _colliders3D.Count; i++)
            {
                var col = _colliders3D[i];
                
                if (!col) continue;
                
                if (!_collider3DStates.TryGetValue(col, out var history))
                {
                    PurrLogger.LogWarning($"Collider '{col.name}' not found in history, " +
                                          $"make sure only one ColliderRollback acts on this collider.", col);
                    continue;
                }
                
                history.Write(_tickManager.localTick, new Collider3DState(col));
            }
            
            for (var i = 0; i < _colliders2D.Count; i++)
            {
                var col = _colliders2D[i];
                
                if (!col) continue;
                
                if (!_collider2DStates.TryGetValue(col, out var history))
                {
                    PurrLogger.LogWarning($"Collider '{col.name}' not found in history, " +
                                          $"make sure only one ColliderRollback acts on this collider.", col);
                    continue;
                }
                
                history.Write(_tickManager.localTick, new Collider2DState(col));
            }
        }

        public void Register(ColliderRollback component)
        {
            var colliders3d = component.colliders3D;
            var colliders2d = component.colliders2D;
            
            int maxEntries = Mathf.CeilToInt(_tickManager.tickRate * component.storeHistoryInSeconds);

            if (colliders3d != null)
            {
                for (var i = 0; i < colliders3d.Length; i++)
                {
                    var collider = colliders3d[i];
                    if (collider == null)
                        continue;
                    
                    _trackedColliders.Add(collider);
                    _collider3DStates.Add(collider, new SimpleHistory<Collider3DState>(maxEntries));
                    _colliders3D.Add(collider);
                }
            }
            
            if (colliders2d != null)
            {
                for (var i = 0; i < colliders2d.Length; i++)
                {
                    var collider = colliders2d[i];
                    if (collider == null)
                        continue;
                    
                    _trackedColliders.Add(collider);
                    _collider2DStates.Add(collider, new SimpleHistory<Collider2DState>(maxEntries));
                    _colliders2D.Add(collider);
                }
            }
        }
        
        public void Unregister(ColliderRollback component)
        {
            var colliders3d = component.colliders3D;
            var colliders2d = component.colliders2D;
            
            if (colliders3d != null)
            {
                for (var i = 0; i < colliders3d.Length; i++)
                {
                    var collider = colliders3d[i];
                    if (collider == null)
                        continue;
                    
                    _trackedColliders.Remove(collider);
                    _collider3DStates.Remove(collider);
                    _colliders3D.Remove(collider);
                }
            }
            
            if (colliders2d != null)
            {
                for (var i = 0; i < colliders2d.Length; i++)
                {
                    var collider = colliders2d[i];
                    if (collider == null)
                        continue;
                    
                    _trackedColliders.Remove(collider);
                    _collider2DStates.Remove(collider);
                    _colliders2D.Remove(collider);
                }
            }
        }
    }
}
