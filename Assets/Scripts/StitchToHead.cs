using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StichToHead : MonoBehaviour
{
    [SerializeField] Transform headTransform;
    void Update()
    {
        transform.position = headTransform.position;
        transform.rotation = headTransform.rotation;
    }
}
