using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuwaFuwaMovement : MonoBehaviour
{
    Rigidbody rootBone;
    Transform[] bonesTransform;
    float[] bonesLength;
    void Start()
    {
        if(rootBone == null) rootBone = GetComponent<Rigidbody>();
        bonesTransform = rootBone.transform.GetComponentsInChildren<Transform>();
        Transform parentBone = null;
        for(int i=1; i< bonesTransform.Length;i++ )
        {
            var t = bonesTransform[i];
            if (t.parent == bonesTransform[i - 1]) parentBone = bonesTransform[i - 1];
        }
    }

    void Update()
    {
        CaluculateFuwafuwa(Vector3.zero);
    }
    void CaluculateFuwafuwa(Vector3 force)
    {

    }
}

