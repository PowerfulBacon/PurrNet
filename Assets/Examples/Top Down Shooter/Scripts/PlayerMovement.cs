using PurrNet.Logging;
using UnityEngine;

namespace PurrNet.Examples.TopDownShooter
{
    public class PlayerMovement : NetworkIdentity
    {
        [SerializeField] private Transform _visuals;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float acceleration = 4f;
        [SerializeField] private float jumpForce = 1.5f;
        [SerializeField] private float gravity = 9.81f;

        private Transform _lastGround;
        private Vector3 _lastGroundPos;

        private CharacterController _controller;
        private float rotationSpeed;
        private float _verticalVelocity;
        private Vector3 currentMove;

        private void Awake()
        {
            if (!TryGetComponent(out _controller))
                PurrLogger.LogError($"Failed to get component '{nameof(CharacterController)}' on '{name}'.", this);
        }

        protected override void OnSpawned(bool asServer)
        {
            if (!asServer)
                enabled = isOwner;
        }

        private void CheckGround()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, .1f) && hit.transform.GetComponent<NetworkIdentity>())
            {
                if (_lastGround != hit.transform)
                {
                    _lastGround = hit.transform;
                    _visuals.SetParent(_lastGround, true);
                }
                _lastGroundPos = _lastGround.position;
            }
            else if (_lastGround)
            {
                _lastGround = null;
                _visuals.SetParent(transform, true);
            }
        }

        private void Update()
        {
            if (!_controller)
                return;


            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector3 targetMove = new Vector3(input.x, 0, input.y).normalized;

            currentMove = Vector3.Lerp(currentMove, targetMove, acceleration * Time.deltaTime);

            if (_controller.isGrounded)
                _verticalVelocity = -gravity * Time.deltaTime;
            else
                _verticalVelocity -= gravity * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space) && _controller.isGrounded)
                _verticalVelocity = jumpForce;

            currentMove.y = _verticalVelocity;
            Vector3 extra = default;

            if (_lastGround)
            {
                extra = _lastGround.position - _lastGroundPos;
                extra.y = 0;
            }

            CheckGround();

            _controller.Move(currentMove * (moveSpeed * Time.deltaTime) + extra);

            if (input != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(currentMove.x, currentMove.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }
        }

        private void LateUpdate()
        {
            _visuals.position = transform.position;
            _visuals.rotation = transform.rotation;
        }
    }
}
