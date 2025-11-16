using UnityEngine;
using System;

namespace milan.Core
{
    public class SwipeDetector : MonoBehaviour
    {
        public static SwipeDetector Instance { get; private set; }

        public event Action OnSwipeUp;
        public event Action OnSwipeLeft;
        public event Action OnSwipeRight;
        public event Action OnSwipeDown;

        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private float maxSwipeTime = 0.5f;
        [SerializeField] private float directionThreshold = 0.3f;

        private Vector2 touchStartPos;
        private float touchStartTime;
        private bool isTouchActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Application.isMobilePlatform)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    StartSwipe(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended && isTouchActive)
                {
                    EndSwipe(touch.position);
                }
            }
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartSwipe(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && isTouchActive)
            {
                EndSwipe(Input.mousePosition);
            }
        }

        private void StartSwipe(Vector2 position)
        {
            touchStartPos = position;
            touchStartTime = Time.time;
            isTouchActive = true;
        }

        private void EndSwipe(Vector2 endPosition)
        {
            isTouchActive = false;

            float swipeTime = Time.time - touchStartTime;
            if (swipeTime > maxSwipeTime)
                return;

            Vector2 swipeDelta = endPosition - touchStartPos;
            float swipeDistance = swipeDelta.magnitude;

            if (swipeDistance < minSwipeDistance)
                return;

            Vector2 swipeDirection = swipeDelta.normalized;

            if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
            {
                if (swipeDirection.x > directionThreshold)
                {
                    OnSwipeRight?.Invoke();
                }
                else if (swipeDirection.x < -directionThreshold)
                {
                    OnSwipeLeft?.Invoke();
                }
            }
            else
            {
                if (swipeDirection.y > directionThreshold)
                {
                    OnSwipeUp?.Invoke();
                }
                else if (swipeDirection.y < -directionThreshold)
                {
                    OnSwipeDown?.Invoke();
                }
            }
        }
    }
}
