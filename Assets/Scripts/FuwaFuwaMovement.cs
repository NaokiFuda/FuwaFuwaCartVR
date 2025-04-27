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
        if (rootBone == null) { rootBone = transform;}
        bonesTransform = rootBone.transform.GetComponentsInChildren<Transform>();
        bonesLength = new float[bonesTransform.Length];
        for(int i=1; i< bonesTransform.Length;i++ )
        {
            var t = bonesTransform[i];
            var k = 1;
            while (bonesTransform[i - k] != t.parent) k++;
            bonesLength[i] = bonesLength[i - k] + Vector3.Distance(t.parent.position, t.position);
        }
    }
    Vector3 lastPos;
    Vector3 lastDir;
    void Update()
    {
        for(int i= 0; i< bonesTransform.Length;i++)
            DoFuwa( CaluculateSwingDirection(i) );

        lastPos = rootBone.position;
        lastDir = rootBone.up;
    }
    Vector4 CaluculateSwingDirection( int i )
    {
        Vector3 swingDir = rootBone.position - lastPos;
        swingDir += (rootBone.up - lastDir) * bonesLength[i];
        swingDir /= 2;
        Vector4 swingDir4 = -swingDir;
        swingDir4.w = i;

        return swingDir4;
    }
    [SerializeField] float bounceness;
    [SerializeField] float hardness = 0.1f;
    void DoFuwa(Vector4 swingDir4)
    {
        Vector3 forceDir = swingDir4;
        var i = (int)swingDir4.w;
        var stiffness = hardness - bonesLength[i];
        bonesTransform[i].rotation = Quaternion.Slerp(bonesTransform[i].rotation, Quaternion.FromToRotation(bonesTransform[i].up, bonesTransform[i].up+forceDir), 1 - stiffness);
    }
    void ReFuwa(Vector4 swingDir4)
    {

    }
    async void DelayFuwa()
    {
        await Task.Delay(1);
        await Task.Run(() => DelayFuwa());
    }
}

