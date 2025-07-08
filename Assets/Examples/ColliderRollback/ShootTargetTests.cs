using PurrNet;
using UnityEngine;

public class ShootTargetTests : NetworkBehaviour
{
    [SerializeField] private Camera _camera;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var shootTarget = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));
            var ray = new Ray(transform.position, shootTarget - transform.position);

            transform.localRotation = Quaternion.LookRotation(ray.direction);

#if UNITY_PHYSICS_3D
            var mouseWorldPoint = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));
            mouseWorldPoint.z = 0f; // Ensure the z-coordinate is zero for 2D physics

            if (Physics.BoxCast(ray.origin, new Vector3(0.1f, 0.1f, 1f), ray.direction, out var hit, transform.rotation))
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.red, 5f);

            if (Physics.CheckSphere(mouseWorldPoint, 0.1f))
                Debug.DrawLine(mouseWorldPoint, Vector3.up, Color.red, 5f);
#endif

#if UNITY_PHYSICS_2D
            var hit2D = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit2D.collider != null)
                Debug.DrawLine(hit2D.point, hit2D.point + hit2D.normal, Color.blue, 5f);
#endif

            Shoot(rollbackTick, ray, transform.rotation, mouseWorldPoint);
        }
    }

    [ServerRpc(requireOwnership: false)]
    private void Shoot(double preciseTick, Ray ray, Quaternion rot, Vector3 mousePos)
    {
#if UNITY_PHYSICS_3D
        if (rollbackModule.BoxCast(preciseTick, ray, new Vector3(0.1f, 0.1f, 1f), rot, out var hit))
            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green, 5f);

        if (rollbackModule.CheckSphere(preciseTick, mousePos, 0.1f))
            Debug.DrawLine(mousePos, Vector3.up, Color.red, 5f);
#endif

#if UNITY_PHYSICS_2D
        var ray2d = new Ray2D(ray.origin, ray.direction);
        if (rollbackModule.Raycast(preciseTick, ray2d, out var hit2D))
            Debug.DrawLine(hit2D.point, hit2D.point + hit2D.normal, Color.yellow, 5f);
#endif
    }
}
