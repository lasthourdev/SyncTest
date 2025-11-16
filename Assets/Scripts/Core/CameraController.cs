using UnityEngine;

namespace milan.Core
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0, 5, -10);

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 targetPosition = _target.position + _offset;
            float t = 1f - Mathf.Exp(-5 * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
        }

    }
}
