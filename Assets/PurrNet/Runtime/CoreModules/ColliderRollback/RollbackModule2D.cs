using UnityEngine;

namespace PurrNet.Modules
{
    public partial class RollbackModule
    {
        static readonly RaycastHit2D[] _raycastHits2D = new RaycastHit2D[1024];
        static readonly RaycastHit2D[] _raycastHits2DCache = new RaycastHit2D[32];

        /// <summary>
        /// Casts a ray, from point origin, in direction direction, of length maxDistance, against all colliders in the scene.
        /// </summary>
        public int Raycast(double preciseTick, Ray2D ray, RaycastHit2D[] raycastHits, 
            float maxDistance = float.PositiveInfinity, 
            ContactFilter2D contactFilter = default)
        {
            if (!_physicsScene2D.IsValid())
                return 0;
            
            int hitCount = _physicsScene2D.Raycast(ray.origin, ray.direction, maxDistance, contactFilter, raycastHits);
            int colliderCount = _colliders2D.Count;
            
            // remove any colliders that we are handling manually
            hitCount = FilterColliders(hitCount, raycastHits);
            
            // handle raycast hits manually
            hitCount = DoManualRaycasts(ray, raycastHits, maxDistance, colliderCount, hitCount, preciseTick, contactFilter);
            
            return hitCount;
        }

        /// <summary>
        /// Casts a ray, from point origin, in direction direction, of length maxDistance, against all colliders in the scene.
        /// </summary>
        public bool Raycast(double preciseTick, Ray2D ray, out RaycastHit2D hit, 
            float maxDistance = float.PositiveInfinity, 
            ContactFilter2D contactFilter = default)
        {
            if (!_physicsScene2D.IsValid())
            {
                hit = default;
                return false;
            }
            
            int hitCount = Raycast(preciseTick, ray, _raycastHits2D, maxDistance, contactFilter);
            
            // return the closest hit
            if (hitCount > 0)
            {
                hit = _raycastHits2D[0];
                for (var i = 1; i < hitCount; i++)
                {
                    if (_raycastHits2D[i].distance < hit.distance)
                        hit = _raycastHits2D[i];
                }
                return true;
            }
            
            hit = default;
            return false;
        }

        private int DoManualRaycasts(Ray2D ray, RaycastHit2D[] hits, float maxDistance, int colliderCount,
            int hitCount, double preciseTick, ContactFilter2D contactFilter)
        {
            for (var i = 0; i < colliderCount; i++)
            {
                if (hitCount >= hits.Length)
                    break;
                
                var col = _colliders2D[i];
                if (!col || !PassesFilters(col, contactFilter))
                    continue;

                if (!TryGetColliderState(preciseTick, col, out var state))
                    continue;

                // Transform the collider bounds to historical position
                var rotation = Quaternion.Euler(0, 0, state.rotation);
                var historicalWorldMatrix = Matrix4x4.TRS(state.position, rotation, state.scale);
                var bounds = col.bounds;
                var historicalBounds = TransformBounds(bounds, historicalWorldMatrix);

                // First check if ray intersects the historical bounds
                if (!RayIntersectsBounds2D(ray, historicalBounds))
                    continue;

                // Calculate the closest point on bounds in the opposite direction
                // This mimics Unity's 2D raycast behavior
                Vector2 closestPoint = historicalBounds.ClosestPoint(ray.origin - ray.direction);
                float distance = Vector2.Distance(ray.origin, closestPoint);

                if (distance > maxDistance)
                    continue;

                // Now we can do the actual intersection test
                var worldToHistorical = historicalWorldMatrix.inverse;
                var localRay = new Ray2D(
                    worldToHistorical.MultiplyPoint3x4(closestPoint),
                    worldToHistorical.MultiplyVector(ray.direction)
                );

                int colCount = col.Raycast(localRay.direction, _raycastHits2DCache);
                if (colCount > 0)
                {
                    for (var j = 0; j < colCount; j++)
                    {
                        var hit = _raycastHits2DCache[j];
                        
                        // Adjust the hit distance to account for the offset
                        hit.distance += distance;

                        // Transform results back
                        hit.point = historicalWorldMatrix.MultiplyPoint3x4(hit.point);
                        hit.normal = historicalWorldMatrix.MultiplyVector(hit.normal);

                        hits[hitCount++] = hit;
                    }
                }
            }

            return hitCount;
        }

        private bool PassesFilters(Collider2D col, ContactFilter2D filter)
        {
            if (!filter.isFiltering)
                return true;

            return !filter.IsFilteringTrigger(col) && 
                   !filter.IsFilteringLayerMask(col.gameObject);
        }

        private static bool RayIntersectsBounds2D(Ray2D ray, Bounds bounds)
        {
            Vector2 min = bounds.min;
            Vector2 max = bounds.max;

            float tmin = float.NegativeInfinity;
            float tmax = float.PositiveInfinity;

            if (Mathf.Abs(ray.direction.x) < float.Epsilon)
            {
                if (ray.origin.x < min.x || ray.origin.x > max.x)
                    return false;
            }
            else
            {
                float tx1 = (min.x - ray.origin.x) / ray.direction.x;
                float tx2 = (max.x - ray.origin.x) / ray.direction.x;

                tmin = Mathf.Max(tmin, Mathf.Min(tx1, tx2));
                tmax = Mathf.Min(tmax, Mathf.Max(tx1, tx2));
            }

            if (Mathf.Abs(ray.direction.y) < float.Epsilon)
            {
                if (ray.origin.y < min.y || ray.origin.y > max.y)
                    return false;
            }
            else
            {
                float ty1 = (min.y - ray.origin.y) / ray.direction.y;
                float ty2 = (max.y - ray.origin.y) / ray.direction.y;

                tmin = Mathf.Max(tmin, Mathf.Min(ty1, ty2));
                tmax = Mathf.Min(tmax, Mathf.Max(ty1, ty2));
            }

            return tmax >= tmin && tmax >= 0;
        }

        private static Bounds TransformBounds(Bounds bounds, Matrix4x4 matrix)
        {
            var center = matrix.MultiplyPoint3x4(bounds.center);
            var extents = bounds.extents;
            var right = matrix.MultiplyVector(new Vector3(extents.x, 0, 0));
            var up = matrix.MultiplyVector(new Vector3(0, extents.y, 0));
            
            var newExtents = new Vector3(
                Mathf.Abs(right.x) + Mathf.Abs(up.x),
                Mathf.Abs(right.y) + Mathf.Abs(up.y),
                0
            );

            return new Bounds(center, newExtents * 2);
        }

        private int FilterColliders(int hitCount, RaycastHit2D[] hits)
        {
            for (var i = 0; i < hitCount; i++)
            {
                var col = hits[i].collider;
                if (col && _trackedColliders.Contains(col))
                    hits[i--] = hits[--hitCount];
            }

            return hitCount;
        }
    }
}
