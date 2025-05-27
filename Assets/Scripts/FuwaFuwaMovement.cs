using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuwaFuwaMovement : MonoBehaviour
{
    [SerializeField] Transform rootBone;
    Transform[] bonesTransform;
    float[] bonesLength;
    Vector3 _lastPos;
    Vector3 _lastDir;
    Vector3[] _defDir;
    Vector3 _lastForceDir;
    float[] _lastGap;
    float[] _lastForce;
    float[] _delta;
    bool[] _doBounce ;
    float[] _deltaRet;
    Vector3[] _rebounceDir;
    Quaternion[] _defRot;

    int grabIndex = -1;
    [SerializeField] Transform[] grabPoint;
    Transform[] _grabedParent;

    Vector2 _screenCenter;
    private int lastScreenWidth;
    private int lastScreenHeight;

    float maxLength;

    void Start()
    {
        if (rootBone == null) { rootBone = transform; }
        bonesTransform = rootBone.transform.GetComponentsInChildren<Transform>();
        bonesLength = new float[bonesTransform.Length];
        _lastForce = new float[bonesTransform.Length];
        _defDir = new Vector3[bonesTransform.Length];
        _defRot = new Quaternion[bonesTransform.Length];
        _delta = new float[bonesTransform.Length];
        _lastGap = new float[bonesTransform.Length];
        _doBounce = new bool[bonesTransform.Length];
        _rebounceDir = new Vector3[bonesTransform.Length];
        _deltaRet = new float[bonesTransform.Length];
        _grabedParent = new Transform[grabPoint.Length];

        for (int i = 0; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];
            if(i != 0)
            {
                var k = 1;
                while (bonesTransform[i - k] != t.parent) k++;
                bonesLength[i] = bonesLength[i - k] + Vector3.Distance(t.parent.position, t.position)+1;
                if (bonesLength[i]> maxLength ) maxLength = bonesLength[i];
            }
            else
            {
                bonesLength[i] = 1;
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
        float force = forceDir.magnitude*10;
        bool isDirectionChange = Vector3.Dot(_lastForceDir.normalized, forceDir.normalized) > 0.7f;
        if (grabIndex >= 0)
        {
            FuwaFuwa( grabIndex ,forceDir, force, isDirectionChange);
        }
        else
        {
            FuwaFuwa( 0, forceDir, force, isDirectionChange);
        }
        
        _lastPos = rootBone.position;
        _lastDir = rootBone.up;
        _lastForceDir = forceDir;

        if(lastScreenHeight != Screen.height || lastScreenWidth != Screen.width) _screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        lastScreenHeight = Screen.height;
        lastScreenWidth = Screen.width;

    }
    void FuwaFuwa( in int rootIndex , in Vector3 forceDir, in float force, in bool isDirectionChange)
    {
        for (int i = rootIndex; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];

            if (force > 0.0001f && isDirectionChange) { _lastForce[i] = 0; }
            _lastForce[i] += force / bonesLength[i];

            _lastForce[i] = Mathf.Min(_tolerate, _lastForce[i]);
            _maxForce = Mathf.Max(_maxForce, _lastForce[i]);

            float forceGap = _maxForce - _lastForce[i];

            Vector4 forceDir4 = forceDir.normalized * _lastForce[i];
            forceDir4.w = i;
            DoFuwa(forceDir4);

            if (_lastGap[i] - forceGap > 0 && force > 0.0001f) { _delta[i] = _lastForce[i]; _deltaRet[i] = _lastForce[i]; _rebounceDir[i] = (_defDir[i] - t.up).normalized; _doBounce[i] = true; }
            if (_lastGap[i] - forceGap > 0 && _delta[i] == 0) _maxForce = 0;
            if (forceGap < 0.1f)
            {
                if (_doBounce[i])
                {
                    _delta[i] = Mathf.Max(0, _delta[i] - _deltaAdd);



                }
                if (_delta[i] == 0) { _lastForce[i] = 0; _doBounce[i] = false; }
                //if (i == 3) Debug.Log(_delta[i] * rebounceDir + " " + (_delta[i] * rebounceDir).magnitude);
                //if (i == 3) Debug.Log("delta " + _delta[i]);
                // if (i == 3) Debug.Log(_lastForce[i] + " " + _delta[i]);
                ReFuwa(_delta[i] * _rebounceDir[i], i, _deltaRet[i] - _delta[i]);
            }

            if (_lastForce[i] < _maxForce) { _lastForce[i] = Mathf.Lerp(_lastForce[i], _maxForce, 0.1f); }
            //if (i == 3) Debug.Log(forceGap);

            _lastGap[i] = forceGap;
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
    
    void DoFuwa(in Vector4 swingDir4)
    {
        var i = (int)swingDir4.w;
        var t = bonesTransform[i];
        t.localRotation =  CaluculateRotate(swingDir4, i);
    }

    
    void ReFuwa(in Vector3 swingDir, in int i , in float deltaTime)
    {
        Transform t = bonesTransform[i];
        Quaternion returnRot = Quaternion.RotateTowards(t.localRotation, _defRot[i], deltaTime);

        // t.localRotation =  returnRot * Quaternion.Inverse(t.localRotation) * CaluculateRotate(swingDir, i);
        t.localRotation = returnRot ;
    }
    Quaternion CaluculateRotate(in Vector3 swingDir, in int i )
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

    [SerializeField] float glabDistance = 0.3f;
    public void SetHold(in Transform glabedTransform, in Vector3 glabPos, in Transform glabHand)
    {
        _isHold = true;
        if (grabIndex < 0)
            for (int i = 0; i < grabPoint.Length; i++)
            {
                if ((grabPoint[i].position - glabedTransform.position).sqrMagnitude < glabDistance * glabDistance) 
                {
                    for (int j = 0; j < bonesTransform.Length; i++)
                        if (grabPoint[i] == bonesTransform[j])
                        {
                            grabIndex = j;
                            break;
                        }
                    grabPoint[i].parent = glabHand;
                    _grabedParent[i] = grabPoint[i].parent;
                    break;
                }
            }

    }
    
    public void SetHold(in Transform glabedTransform, in Vector3 glabPos)
    {
        _isHold = true;
        if (grabIndex < 0)
            for(int i = 0; i < grabPoint.Length; i++)
            {
                if (grabPoint[i].position.x - _screenCenter.x < glabDistance )
                    if(grabPoint[i].position.y - _screenCenter.y < glabDistance || grabPoint[i].position.y - _screenCenter.y * 2 / 4 < glabDistance || grabPoint[i].position.y - _screenCenter.y * 2 * 3 / 4 < glabDistance)
                    {
                        for (int j = 0; j < bonesTransform.Length; j++)
                        {
                            Debug.Log(j);
                            if (grabPoint[i] == bonesTransform[j])
                            {
                                grabIndex = j;
                                Debug.Log(grabIndex);
                                break;
                            }
                        }
                        break;
                    }
            }

    }
    bool _isHold;
    public void SetRelease()
    {
        if (!_isHold) return;
        _isHold = false;
        for (int i = 0; i < grabPoint.Length; i++)
            if (grabPoint[i] == bonesTransform[grabIndex]) grabPoint[i].parent = _grabedParent[i];
        grabIndex = -1;
    }
}

