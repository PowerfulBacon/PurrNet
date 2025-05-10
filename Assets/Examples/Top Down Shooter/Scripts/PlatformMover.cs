using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    [SerializeField] private Vector3 _movement = new Vector3(0, 0, 2);
    [SerializeField] private float _speed = 1f;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _time;

    private void Awake()
    {
        _startPos = transform.position - _movement;
        _endPos = transform.position + _movement;
    }

    void Update()
    {
        _time += Time.deltaTime;
        var pingpong = Mathf.PingPong(_time * _speed, 1);
        transform.position = Vector3.Lerp(_startPos, _endPos, pingpong);
    }
}
