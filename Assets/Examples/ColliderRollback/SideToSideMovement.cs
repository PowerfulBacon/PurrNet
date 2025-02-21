using PurrNet;
using UnityEngine;

public class SideToSideMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _distance = 1f;

    private void Update()
    {
        if (!isController)
            return;
        
        transform.position = new Vector3(
            Mathf.Sin(Time.time * _speed) * _distance,
            transform.position.y,
            transform.position.z
        );
    }
}
