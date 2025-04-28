using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FuwaFuwaMovement : MonoBehaviour
{
    [SerializeField] Transform rootBone;
    Transform[] bonesTransform;
    float[] bonesLength;
    Vector3[] _lastPos;
    Vector3[] _lastDir;
    Vector3[] _defDir;

    void Start()
    {
        if (rootBone == null) { rootBone = transform; }
        bonesTransform = rootBone.transform.GetComponentsInChildren<Transform>();
        bonesLength = new float[bonesTransform.Length];
        _lastPos = new Vector3[bonesTransform.Length];
        _lastDir = new Vector3[bonesTransform.Length];
        _lastForce = new Vector3[bonesTransform.Length];
        _defDir = new Vector3[bonesTransform.Length];
        for (int i = 1; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];
            var k = 1;
            while (bonesTransform[i - k] != t.parent) k++;
            bonesLength[i] = bonesLength[i - k] + Vector3.Distance(t.parent.position, t.position);
            _lastPos[i] = t.position;
            _lastDir[i] = t.up;
            _defDir[i] = t.up;
        }
    }

    void Update()
    {
        for (int i = 0; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];
            Vector4 forceDir = CaluculateSwingDirection(i);
            var dotTest = Vector3.Dot(_lastForce[i], forceDir);
            
            if (dotTest < 0.1f && Vector3.SqrMagnitude(forceDir) < 0.01f * 0.01f*Time.deltaTime * Time.deltaTime)
            {
                //ReFuwa(-forceDir, i);
                //Debug.Log(_lastForce[i]);
            }
            else DoFuwa(forceDir);
        }
    }
    Vector4 CaluculateSwingDirection(int i)
    {
        Vector3 swingDir = (bonesTransform[i].position - _lastPos[i]);
        swingDir += (bonesTransform[i].up - _lastDir[i]) * bonesLength[i];
        Vector4 swingDir4 = -swingDir;
        swingDir4.w = i;
        return swingDir4;
    }

    [SerializeField] float bounceness;
    [SerializeField] AnimationCurve hardness = AnimationCurve.Constant(timeStart: 0f, timeEnd: 1f, value: 1f);
    [SerializeField] float angleLimit = 90;
    Vector3[] _lastForce;
    void DoFuwa(Vector4 swingDir4)
    {
        Vector3 forceDir = swingDir4;
        var i = (int)swingDir4.w;
        var t = bonesTransform[i];
        if (Vector3.Dot(t.up, t.up + forceDir) > 0.999f)
        {
            _lastPos[i] = t.position;
            _lastDir[i] = t.up;
            _lastForce[i] = forceDir;
        }
        DoRotate(forceDir, i);
    }
    void ReFuwa(Vector3 swingDir, int i)
    {
        var t = bonesTransform[i];
        //swingDir *= 0.9f;
        
        DoRotate(swingDir, i);
    }
    void DoRotate(Vector3 swingDir, int i)
    {
        var t = bonesTransform[i];
        float hardnessStrength = hardness.Evaluate(bonesLength[i] / bonesLength[bonesLength.Length - 1]);
        var stiffness = (1 - hardnessStrength) * bonesLength[i] * Time.deltaTime * 10;
        var axis = Vector3.Cross(t.up , swingDir);
        var angle = Vector3.Angle(t.up, t.up+ swingDir);
        var angleTest = Vector3.Angle(_defDir[i], t.up + swingDir);
        if (angleTest > angleLimit)  angle -= angleTest - angleLimit;
        var targetRot = Quaternion.AngleAxis(angle, axis) * t.localRotation ;
        
        t.localRotation = Quaternion.Slerp(t.localRotation, targetRot, stiffness);
    }
}

