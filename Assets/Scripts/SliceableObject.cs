using Deform;
using UnityEngine;


namespace UnityTemplateProjects
{
    public class SliceableObject : MonoBehaviour
    {
        private KnifeController _knifeController;
        
        private BoxCollider _boxCollider;

        private float _skewFactor;

        private float _minSkewFactor;

        private const float MAX_SKEW_FACTOR = 0f;

        private const float BASE_SPEED = 0.3f;

        private const float FINAL_DESTINATION_OFFSET = 3f;

        private float Speed { get; set; }
        
        private Vector3 FinalDestination { get; set; }
        
        public float MaxThickness { get; set; }

        
        private void Awake()
        {
            var sliceablePosition = transform.position;
            
            FinalDestination = new Vector3(sliceablePosition.x, sliceablePosition.y,
                sliceablePosition.z - FINAL_DESTINATION_OFFSET);

            _knifeController = FindObjectOfType<KnifeController>();

            MaxThickness = GetComponent<BoxCollider>().bounds.size.z * 2f;
        }

        private void OnEnable()
        {
            _boxCollider = GetComponent<BoxCollider>();

            Speed = BASE_SPEED;
        }

        private void Update()
        {
            Move();
            
            AdjustSkewFactor();
        }

        private void Move()
        {
            if (!_knifeController.IsKnifeMoving && !_knifeController.IsStartedCut)
            {
                transform.position = Vector3.MoveTowards(transform.position, FinalDestination,
                    Speed * Time.deltaTime);
            }
        }

        private void AdjustSkewFactor()
        {
            SkewDeformer skewDeformer = GetComponentInChildren<SkewDeformer>();
            
            if (_boxCollider != null && _knifeController != null && skewDeformer != null)
            {
                Vector3 closestPoint = _boxCollider.ClosestPoint(_knifeController.transform.position);

                Vector3 edgePoint = _boxCollider.ClosestPoint(closestPoint - transform.forward);
                
                Vector3 closestPointFromKnifeToSliceable =
                    _knifeController.GetComponentInChildren<BoxCollider>().ClosestPoint(transform.position);

                float distance = Vector3.Distance(closestPointFromKnifeToSliceable,
                    new Vector3(closestPoint.x, -closestPoint.y, closestPoint.z));
                
                float maxDistance = _boxCollider.bounds.size.y * 2f;
                
                var thickness = Vector3.Distance(edgePoint, closestPoint);

                var normalizedThickness = Mathf.Clamp01(thickness / MaxThickness);
               
                _minSkewFactor = 1f - normalizedThickness;

                if (distance <= maxDistance)
                {
                    float normalizedDistance = Mathf.Clamp01(distance / maxDistance);

                    float factor = -Mathf.Lerp(_minSkewFactor, MAX_SKEW_FACTOR, normalizedDistance);

                    if (factor < skewDeformer.Factor)
                    {
                        skewDeformer.Factor = factor;
                    }
                }
            }
        }
    }
}