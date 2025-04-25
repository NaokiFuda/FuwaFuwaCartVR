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
        for(int i=1; i< bonesTransform.Length;i++ )
        {
            var t = bonesTransform[i];
            bonesLength[i] = Vector3.distance(rootBone.transform.position - t.position);
        }
    }

    void Update()
    {
        CaluculateFuwafuwa(Vector3.zero);
    }
    void CaluculateFuwafuwa(Vector3 forceDir)
    {
        for (int i = 1; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];
            
        }
    }
    public void DoFuwafuwa(Vector3 forceDir)
    {
        
    }
}

