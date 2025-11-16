using UnityEngine;

namespace Script
{
    public class CameraFollow : MonoBehaviour
    {
        public float smoothTime = 0.25f;
        // Offset from the target (Vector3.zero will not work)
        private readonly Vector3 _offset = new(0f, 0f, -10f);
        private Vector3 _velocity = Vector3.zero;

        [SerializeField] private Transform target;

        private void Update()
        {
            // Skip if target is null
            if (!target) return;
            var targetPosition = target.position + _offset;
            // Smoothly move towards the target
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothTime);
        }
    }
}