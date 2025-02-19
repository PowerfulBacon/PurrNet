using PurrNet;
using UnityEngine;

public class ShootTargetTests : NetworkBehaviour
{
    [SerializeField] private Camera _camera;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.red, 5f);
            
            Shoot(rollbackTick, ray);
        }
    }

    [ServerRpc(requireOwnership: false)]
    private void Shoot(double preciseTick, Ray ray)
    {
        if (rollbackModule.Raycast(preciseTick, ray, out var hit))
            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green, 5f);
    }
}
