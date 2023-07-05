using UnityEngine;

namespace UnityTemplateProjects
{
    public class KnifeController : MonoBehaviour
    {
        private Vector3 _startPoint;
        private Vector3 _finishPoint;
        private Vector3 _currentPosition;

        [SerializeField] private float _movementSpeed;

        private const float MIN_Y_COORD = 0.05f;

        public bool IsKnifeMoving { get; set; }
        
        public bool IsStartedCut { get; set; }
        
        private void Awake()
        {
            var knifePosition = transform.position;
            _startPoint = knifePosition;
            _finishPoint = new Vector3(knifePosition.x, MIN_Y_COORD, knifePosition.z);
            _currentPosition = _startPoint;
        }

        private void Update()
        {
            HandleInput();
        }

        private void MoveKnife(Vector3 currentPosition, Vector3 otherPoint)
        {
            var step = _movementSpeed * Time.deltaTime;

            currentPosition = Vector3.MoveTowards(currentPosition, otherPoint, step);
            var knifeTransform = transform;
            knifeTransform.position = currentPosition;
            
            _currentPosition = knifeTransform.position;

            if (currentPosition == _startPoint)
            {
                IsKnifeMoving = false;
                gameObject.GetComponentInChildren<Collider>().enabled = true;
            }
            
            if (currentPosition == _finishPoint)
            {
                IsStartedCut = false;
            }
        }

        private void HandleInput()
        {
            if (Input.GetMouseButton(0) 
                || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                MoveKnife(_currentPosition, _finishPoint);
                IsKnifeMoving = true;
            }
            else
            {
                if (_currentPosition != _startPoint)
                {
                    MoveKnife(_currentPosition, _startPoint);
                }
            }
        }
    }
}