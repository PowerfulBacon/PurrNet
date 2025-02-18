using System.Collections.Generic;
using PurrNet.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrNet.Modules
{
    public class RollbackModule : INetworkModule
    {
        readonly Scene _scene;
        readonly PhysicsScene _physicsScene;
        readonly TickManager _tickManager;
        readonly HashSet<Component> _trackedColliders = new ();
        
        private readonly List<Collider> _colliders3D = new ();
        private readonly List<Collider2D> _colliders2D = new ();
        
        readonly Dictionary<Collider, SimpleHistory<Collider3DState>> _collider3DStates = new ();
        readonly Dictionary<Collider2D, SimpleHistory<Collider2DState>> _collider2DStates = new ();

        public RollbackModule(TickManager tick, Scene scene)
        {
            _tickManager = tick;
            _scene = scene;
            _physicsScene = scene.GetPhysicsScene();
        }
        
        public void Enable(bool asServer)
        {
        }

        public void Disable(bool asServer)
        {
        }
        
        static readonly RaycastHit[] _raycastHits = new RaycastHit[1];

        public bool Raycast(double preciseTick, Ray ray, out RaycastHit hit, 
            float maxDistance = float.PositiveInfinity, 
            int layerMask = Physics.AllLayers,
            QueryTriggerInteraction queryTriggers = QueryTriggerInteraction.UseGlobal)
        {
            int hitCount = _physicsScene.Raycast(ray.origin, ray.direction, _raycastHits, maxDistance, layerMask, queryTriggers);
            // TODO: Implement precise tick raycast
            hit = default;
            return false;
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
                
                history.Write(_tickManager.tick, new Collider3DState(col));
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
                
                history.Write(_tickManager.tick, new Collider2DState(col));
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
