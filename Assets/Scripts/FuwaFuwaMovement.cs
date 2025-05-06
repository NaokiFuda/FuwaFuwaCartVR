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
    Quaternion[] _defRot;
    float[] _delta;
    float maxLength;
    [SerializeField] float movementThreshold = 0.001f; // î˜è¨Ç»ìÆÇ´ÇåüèoÇ∑ÇÈÇΩÇﬂÇÃËáíl

    void Start()
    {
        if (rootBone == null) { rootBone = transform; }
        bonesTransform = rootBone.transform.GetComponentsInChildren<Transform>();
        bonesLength = new float[bonesTransform.Length];
        _lastForce = new float[bonesTransform.Length];
        _defDir = new Vector3[bonesTransform.Length];
        _defRot = new Quaternion[bonesTransform.Length];
        //_lastPos = new Vector3[bonesTransform.Length];
        //_lastDir = new Vector3[bonesTransform.Length];
        _delta = new float[bonesLength.Length];
        _lastForceDir = new Vector3[bonesLength.Length];
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
                _lastPos = rootBone.position;
                _lastDir = rootBone.up;
            }
            _defDir[i] = t.up;
            _defRot[i] = t.localRotation;
        }
    }

    void Update()
    {
        for (int i = 0; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];
            Vector3 forceDir = CaluculateSwingDirection();
            float force = forceDir.magnitude;
            //var dotTest = Vector3.Dot(t.up, t.up + forceDir);
            bool isNotMoving = force < movementThreshold;

            var dotTest = Vector3.Dot(_lastDir, forceDir);
            //Debug.Log(_lastForce[i].sqrMagnitude);
            
            if (isNotMoving && _lastForce[i] >0.01f)
            {
                //_lastForce[i] = Mathf.Max(0,_lastForce[i] - 0.01f);

                ReFuwa(_lastForce[i] * _lastForceDir[i], i);
                
            }
            else
            {
                _lastForce[i] += force;
                if (dotTest < 0f) _lastForce[i] = 0;
                _lastForce[i] = Mathf.Min(0.4f, _lastForce[i]);
                //Debug.Log(_lastForce[i] + " " + i);
                //if (i == 1) Debug.Log(_lastForce[i]);
                Vector4 forceDir4 = forceDir.normalized * _lastForce[i];
                forceDir4.w = i;
                DoFuwa(forceDir4);
                _lastForceDir[i] = _defDir[i] - t.up;
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

        t.localRotation =CaluculateRotate(swingDir4, i) ;
    }
    //[SerializeField , Range(0,1)] float returnPow= 1;
    float _deltaAdd = 0.1f;
    void ReFuwa(Vector3 swingDir, int i)
    {
        var t = bonesTransform[i];
        //if (_delta[i] < 1) _delta[i] = Mathf.Min(_delta[i] + _deltaAdd * Time.deltaTime, 1f);
       // var _dotTest = Vector3.Cross(t.up, swingDir);
        //if (Vector3.Dot(_dotTest, t.forward) < 0) { swingDir *= -1; }
        var returnRot = Quaternion.RotateTowards(t.localRotation, _defRot[i], _deltaAdd);
        //swingDir = swingDir ;
        t.localRotation = CaluculateRotate( swingDir, i);
    }
    Quaternion CaluculateRotate(Vector3 swingDir, int i)
    {
        var t = bonesTransform[i];
        float hardnessStrength = hardness.Evaluate(bonesLength[i] / maxLength);
        var axis = Vector3.Cross(t.up , swingDir);
        var angle = Vector3.SignedAngle(t.up, t.up + swingDir, axis);
        var angleTest = Vector3.SignedAngle(_defDir[i], t.up + swingDir, axis);
        if (Mathf.Abs(angleTest) > angleLimit)  angle -= Mathf.Sign(angleTest)* (Mathf.Abs(angleTest) - angleLimit);
        var targetRot = Quaternion.AngleAxis(angle, axis) * t.localRotation ;

        return Quaternion.Slerp(t.localRotation, targetRot, hardnessStrength);
    }
}

