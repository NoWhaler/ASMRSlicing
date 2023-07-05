using System.Collections.Generic;
using Deform;
using EzySlice;
using UnityEngine;

namespace UnityTemplateProjects
{
    public class CollisionDetection : MonoBehaviour
    {
        [SerializeField] private Material _crossSharedMaterial;

        [SerializeField] private SkewDeformer _skewPrefab;

        [field: SerializeField] private List<Collider> _allColliders = new List<Collider>();

        [field: SerializeField] private List<GameObject> _cutObjects = new List<GameObject>();

        private void OnTriggerEnter(Collider other)
        {
            var sliceObject = other.GetComponent<SliceableObject>();
            if (sliceObject != null)
            {
                var knife = GetComponentInParent<KnifeController>();
                knife.IsStartedCut = true;

                _allColliders.Add(other);
                Cut();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            var table = other.gameObject.GetComponent<Table>();
            if (table != null)
            {
                gameObject.GetComponent<Collider>().enabled = false;
                PushCutObject();
            }
        }

        private void Cut()
        {
            foreach (var objectCollider in _allColliders)
            {
                SlicedHull cutObject = Slice(objectCollider.gameObject, _crossSharedMaterial);

                if (cutObject != null)
                {
                    GameObject cutup = cutObject.CreateUpperHull(objectCollider.gameObject, _crossSharedMaterial);
                    GameObject cutDown = cutObject.CreateLowerHull(objectCollider.gameObject, _crossSharedMaterial);

                    AddHulls(cutup, false, objectCollider.GetComponent<SliceableObject>().MaxThickness);

                    AddHulls(cutDown, true, objectCollider.GetComponent<SliceableObject>().MaxThickness);

                    Destroy(objectCollider.gameObject);
                }
            }

            _allColliders.Clear();
        }
        
        private SlicedHull Slice(GameObject obj, Material mat = null)
        {
            return obj.Slice(transform.position, Vector3.back, mat);
        }

        private void PushCutObject()
        {
            foreach (var slicedObject in _cutObjects)
            {
                var rigidBody = slicedObject.GetComponent<Rigidbody>();
                rigidBody.useGravity = true;
                rigidBody.isKinematic = false;
                
                Vector3 forceDirection = new Vector3(0f, 0f, -1f);
                float forceMagnitude = 2f; 

                Vector3 force = forceDirection.normalized * forceMagnitude;
                Vector3 downwardForce = new Vector3(0f, -forceMagnitude, 0f);
                
                rigidBody.AddForce(force, ForceMode.VelocityChange);
                
                rigidBody.AddForce(downwardForce, ForceMode.VelocityChange);
                
                Destroy(slicedObject, 2f);
            }
            
            _cutObjects.Clear();
        }
        
        
        private void AddHulls(GameObject hull, bool stay, float thickness)
        {
            if (!stay)
            { 
                var rigidBody = hull.AddComponent<Rigidbody>();
                hull.AddComponent<BoxCollider>();
                
                var sliceable = hull.AddComponent<SliceableObject>();
                
                rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
                
                Deformable deformer = hull.AddComponent<Deformable>();
                
                SkewDeformer skewDeformer = Instantiate(_skewPrefab, hull.transform.position, Quaternion.identity, hull.transform);

                deformer.DeformerElements = new List<DeformerElement>() {new DeformerElement()};

                deformer.DeformerElements[0].Component = skewDeformer;

                skewDeformer.Factor = 0f;

                _cutObjects.Add(hull);

                sliceable.MaxThickness = thickness;
                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;
            }
            else
            {
                hull.gameObject.layer = 6;
                BoxCollider boxCollider = hull.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                
                var sliceable = hull.AddComponent<SliceableObject>();

                sliceable.MaxThickness = boxCollider.bounds.size.z * 2f;
            }
        }
    }
}