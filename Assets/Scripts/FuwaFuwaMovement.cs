using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FuwaFuwaMovement : MonoBehaviour
{
    [SerializeField]Transform rootBone;
    Transform[] bonesTransform;
    float[] bonesLength;
    Vector3[] _lastPos;
    Vector3[] _lastDir;

    void Start()
    {
        if (rootBone == null) { rootBone = transform;}
        bonesTransform = rootBone.transform.GetComponentsInChildren<Transform>();
        bonesLength = new float[bonesTransform.Length];
        _lastPos = new Vector3[bonesTransform.Length];
        _lastDir = new Vector3[bonesTransform.Length];
        _lastForce = new Vector3[bonesTransform.Length];
        for (int i=1; i< bonesTransform.Length;i++ )
        {
            var t = bonesTransform[i];
            var k = 1;
            while (bonesTransform[i - k] != t.parent) k++;
            bonesLength[i] = bonesLength[i - k] + Vector3.Distance(t.parent.position, t.position);
            _lastPos[i] = t.position;
            _lastDir[i] = t.up;
        }
    }
    
    void Update()
    {
        for(int i= 0; i< bonesTransform.Length;i++)
        {
            var t = bonesTransform[i];
            Vector4 forceDir = CaluculateSwingDirection(i);
            Debug.Log(Vector3.Dot(_lastForce[i], forceDir));
            if (Vector3.Dot(_lastForce[i] , forceDir) > 0.98f )
            {
                ReFuwa(-_lastForce[i], i);
                
            }
            else DoFuwa(forceDir);
        }
    }
    Vector4 CaluculateSwingDirection( int i )
    {
        Vector3 swingDir = (rootBone.position - _lastPos[i]);
        swingDir += (rootBone.up - _lastDir[i]) * bonesLength[i];
        swingDir /= 2;
        Vector4 swingDir4 = -swingDir;
        swingDir4.w = i;

        return swingDir4;
    }
    [SerializeField] float bounceness;
    [SerializeField] float hardness = 0.1f;
    Vector3[] _lastForce;
    void DoFuwa(Vector4 swingDir4)
    {
        Vector3 forceDir = swingDir4;
        var i = (int)swingDir4.w;
        var t = bonesTransform[i];
        if (Vector3.Dot(t.up, t.up + forceDir) > 0.999f)
        {
            _lastForce[i] = swingDir4;
        }
        DoRotate(forceDir, i);
    }
    void ReFuwa(Vector3 swingDir, int i)
    {
        var t = bonesTransform[i];
        swingDir *= 0.9f;
        if (swingDir.sqrMagnitude <= 0.01f)
        {
            _lastPos[i] = rootBone.position;
            _lastDir[i] = rootBone.up;
            return;
        }
        else DoRotate(swingDir, i);
    }
    void DoRotate(Vector3 swingDir, int i)
    {
        var t = bonesTransform[i];
        var stiffness = (1 - hardness) * bonesLength[i] * Time.deltaTime * 10;
        var targetDir = t.up + swingDir;
        t.rotation = Quaternion.Slerp(t.rotation, Quaternion.FromToRotation(t.up, t.up + swingDir), stiffness);
    }
    async void DelayFuwa()
    {
        await Task.Delay(1);
        await Task.Run(() => DelayFuwa());
    }
}

