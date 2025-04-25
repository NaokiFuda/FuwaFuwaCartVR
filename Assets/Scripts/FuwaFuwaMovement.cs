using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FuwaFuwaMovement : MonoBehaviour
{
    [SerializeField]Transform rootBone;
    Transform[] bonesTransform;
    float[] bonesLength;
    void Start()
    {
        if(rootBone == null) rootBone = transform;
        bonesTransform = rootBone.transform.GetComponentsInChildren<Transform>();
        bonesLength = new float[bonesLength.Length];
        var parentTransform = rootBone.transform;
        for(int i=1; i< bonesTransform.Length;i++ )
        {
            var t = bonesTransform[i];
            var startPos = parentTransform.position;
            var k = 1;
            if (parentTransform != t.parent)
            {
                parentTransform = t.parent; 
                startPos = bonesTransform[i - 1].position; 
            }
            else 
            {
                startPos = parentTransform.position;
                while(bonesTransform[i-k] != parentTransform) k++;
            }
            bonesLength[i] = bonesLength[i - k] + Vector3.Distance(startPos, t.position);
        }
    }
    Vector3 lastPos;
    Vector3 lastDir;
    void Update()
    {
        //CaluculateFuwafuwa( CaluculateSwingDirection() );
    }
    Vector3 CaluculateSwingDirection()
    {
        var swingDir = rootBone.position - lastPos;
        swingDir += (rootBone.forward - lastDir).normalized * Vector3.Angle(lastDir, rootBone.forward);
        
        lastPos = rootBone.position;
        lastDir = rootBone.forward;
        return -swingDir;
    }
    void CaluculateFuwafuwa(Vector3 forceDir)
    {
        for (int i = 1; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];
            DoFuwafuwa(forceDir);
        }
    }
    async void DoFuwafuwa(Vector3 forceDir)
    {

        await Task.Delay(1);
        await Task.Run(() => DoFuwafuwa(forceDir));
    }
}

