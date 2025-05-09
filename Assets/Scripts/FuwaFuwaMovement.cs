using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FuwaFuwaMovement : MonoBehaviour
{
    [SerializeField] Transform rootBone;
    Transform[] bonesTransform;
    float[] bonesLength;
    Vector3 _lastPos;
    Vector3 _lastDir;
    Vector3[] _defDir;
    Vector3[] _lastForceDir;
    float[] _lastForce;
    float[] _delta;
    Quaternion[] _defRot;
    float maxLength;

    void Start()
    {
        if (rootBone == null) { rootBone = transform; }
        bonesTransform = rootBone.transform.GetComponentsInChildren<Transform>();
        bonesLength = new float[bonesTransform.Length];
        _lastForce = new float[bonesTransform.Length];
        _defDir = new Vector3[bonesTransform.Length];
        _defRot = new Quaternion[bonesTransform.Length];
        _lastForceDir = new Vector3[bonesLength.Length];
        _delta = new float[bonesLength.Length];
        for (int i = 0; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];
            if(i != 0)
            {
                var k = 1;
                while (bonesTransform[i - k] != t.parent) k++;
                bonesLength[i] = bonesLength[i - k] + Vector3.Distance(t.parent.position, t.position);
                if (bonesLength[i]> maxLength ) maxLength = bonesLength[i];
            }
            else
            {
                bonesLength[i] = 0;
                _lastPos = rootBone.position;
                _lastDir = rootBone.up;
            }
            _defDir[i] = t.up;
            _defRot[i] = t.localRotation;
        }
    }
    [SerializeField]float _deltaAdd = 1f;
    [SerializeField] float _tolerate =0.5f;
    [SerializeField] float _maxForce;
    void Update()
    {
        Vector3 forceDir = CaluculateSwingDirection();
        float force = forceDir.magnitude;

        for (int i = 0; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];

            if (bonesLength[i] != 0)_lastForce[i] += force / bonesLength[i] *100;
            else _lastForce[i] += force;

            float forceGap = _maxForce - _lastForce[i];
            float fixedGap = forceGap / bonesLength[i] * 100;
            if (_lastForce[i] < _maxForce) _lastForce[i] = Mathf.Lerp(_lastForce[i], _maxForce, fixedGap);
            
            _lastForce[i] = Mathf.Min(_tolerate, _lastForce[i]);
            _maxForce = Mathf.Max(_maxForce, _lastForce[i]);
            Vector4 forceDir4 = forceDir.normalized * _lastForce[i];
            forceDir4.w = i;
            DoFuwa(forceDir4);
            _lastForceDir[i] = _defDir[i] - t.up;

            if (_maxForce - _lastForce[i] > 0.001f) { _delta[i] = _lastForce[i]/2;  }
            else
            {
                _delta[i] = Mathf.Max(0, _delta[i] - 0.01f / bonesLength[i] * 100);
                if (_delta[i] == 0) _lastForce[i] = 0;

                ReFuwa(_delta[i] * _lastForceDir[i], i, _deltaAdd);
            }

            _lastPos = rootBone.position;
            _lastDir = rootBone.up;
        }
    }
    
    Vector3 CaluculateSwingDirection()
    {
        Vector3 swingDir = (rootBone.position - _lastPos);
        swingDir += rootBone.up - _lastDir;
        
        return -swingDir;
    }

    [SerializeField] float bounceness;
    [SerializeField] AnimationCurve hardness = AnimationCurve.Constant(timeStart: 0.05f, timeEnd: 0.3f, value: 1f);
    [SerializeField] float angleLimit = 90;
    
    void DoFuwa(Vector4 swingDir4)
    {
        var i = (int)swingDir4.w;
        var t = bonesTransform[i];
        t.localRotation =  CaluculateRotate(swingDir4, i);
    }

    
    void ReFuwa(Vector3 swingDir, int i , float deltaTime)
    {
        Transform t = bonesTransform[i];
        Quaternion returnRot = Quaternion.RotateTowards(t.localRotation, _defRot[i], deltaTime);

        //t.localRotation = CaluculateRotate(swingDir, i) * Quaternion. * returnRot ;
        t.localRotation = returnRot;
    }
    Quaternion CaluculateRotate(Vector3 swingDir, int i )
    {
        Transform t = bonesTransform[i];

        Vector3 currentUp = t.up;
        Vector3 targetDir = t.up + swingDir;
        Vector3 axis = Vector3.Cross(currentUp, targetDir).normalized;
        float hardnessStrength = hardness.Evaluate(bonesLength[i] / maxLength);
        float angle = Vector3.SignedAngle(t.up, t.up + swingDir, axis);
        float angleTest = Vector3.SignedAngle(rootBone.parent.rotation * _defDir[i], t.up + swingDir, axis);
        if (Mathf.Abs(angleTest) > angleLimit)  angle -= Mathf.Sign(angleTest)* (Mathf.Abs(angleTest) - angleLimit);
        Quaternion fixedTwist =  _defRot[i]* Quaternion.Inverse(t.localRotation);
        fixedTwist.x = 0; fixedTwist.z = 0;
        Quaternion fixedCurrentRot = fixedTwist * t.localRotation;
        Quaternion targetRot = Quaternion.AngleAxis(angle, axis) * fixedCurrentRot;
        
        return Quaternion.Slerp(fixedCurrentRot, targetRot, hardnessStrength);
    }
}

