using UnityEngine;

namespace PurrNet.Examples.TopDownShooter
{
    public class PlayerHealth : NetworkBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private Reference<TextMesh> _healthText;
        [SerializeField] SyncVar<int> _health = new SyncVar<int>();

        protected override void OnSpawned(bool asServer)
        {
            if (asServer)
                _health.value = maxHealth;
        }

        protected override void OnSpawned()
        {
            UpdateHealthUI();
            _health.onChanged += OnHealthChanged;
        }

        protected override void OnDespawned()
        {
            _health.onChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int newVal)
        {
            UpdateHealthUI();
        }

        private void Update()
        {
            if (!_healthText.value)
                return;

            Vector3 direction = _healthText.value.transform.position - Camera.main.transform.position;
            direction.x = 0;
            _healthText.value.transform.rotation = Quaternion.LookRotation(direction);
        }

        private void UpdateHealthUI()
        {
            if (!_healthText.value)
                return;
            _healthText.value.text = _health.value.ToString();
        }

        public void ChangeHealth(int change)
        {
            if (_health + change <= 0)
            {
                Destroy(gameObject);
                return;
            }

            ChangeHealth_Server(change);
        }

        [ServerRpc(requireOwnership: false)]
        private void ChangeHealth_Server(int change)
        {
            _health.value += change;
        }
    }
}
