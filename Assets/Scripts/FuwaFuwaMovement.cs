using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class FuwaFuwaMovement : MonoBehaviour
{
    [SerializeField] Transform rootBone;
    Transform[] bonesTransform;
    float[] bonesLength;
    Vector3 _lastPos;
    Vector3 _lastDir;
    Vector3[] _defDir;
    float[] _lastForce;
    float[] _deltaForce;
    float maxLength;

    Quaternion[] _defRot;
    [SerializeField] private bool suppressX = false, suppressY = false, suppressZ = false;
    [SerializeField][Range(0, 1)] private float[] suppressionStrengths = new float[3] { 0, 0, 0 };

    int grabIndex = -1;
    [SerializeField] Transform[] grabPoint;
    Transform[] _grabedParent;



    [SerializeField] AnimationCurve hardness = AnimationCurve.EaseInOut(timeStart: 0f, valueStart: 0.1f, timeEnd: 1f, valueEnd: 0.1f);
    [SerializeField] AnimationCurve rebounceness = AnimationCurve.EaseInOut(timeStart: 0f, valueStart: 0.5f, timeEnd: 1f, valueEnd: 0.5f);
    [SerializeField] AnimationCurve angleLimitness = AnimationCurve.EaseInOut(timeStart: 0f, valueStart: 1f, timeEnd: 1f, valueEnd: 1f);
    [SerializeField] float angleLimit = 90f;



    [SerializeField] float glabDistance = 0.3f;



    void OnEnable()
    {
        if (rootBone == null) { rootBone = transform; }
        bonesTransform = rootBone.transform.GetComponentsInChildren<Transform>();
        bonesLength = new float[bonesTransform.Length];
        _lastForce = new float[bonesTransform.Length];
        _defDir = new Vector3[bonesTransform.Length];
        _defRot = new Quaternion[bonesTransform.Length];
        _deltaForce = new float[bonesTransform.Length];

        for (int i = 0; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];
            if (i != 0)
            {
                var k = 1;
                while (bonesTransform[i - k] != t.parent) k++;
                bonesLength[i] = bonesLength[i - k] + Vector3.Distance(t.parent.position, t.position);
                if (bonesLength[i] > maxLength) maxLength = bonesLength[i];
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


    void FixedUpdate()
    {
        Vector3 forceDir = CaluculateSwingDirection();
        float force = forceDir.magnitude;
        if (grabIndex >= 0)
        {
            FuwaFuwa(grabIndex, forceDir, force);
        }
        else
        {
            FuwaFuwa(0, forceDir, force);
        }

        _lastPos = rootBone.position;
        _lastDir = rootBone.up;
    }

    void FuwaFuwa(in int rootIndex, in Vector3 forceDir, in float force)
    {
        for (int i = rootIndex; i < bonesTransform.Length; i++)
        {
            var t = bonesTransform[i];

            if (i == 0) continue;
            _lastForce[i] = force / bonesLength[i];

            Vector4 forceDir4 = ApplyDirectionalSuppression(forceDir.normalized * _lastForce[i], i);
            forceDir4.w = i;

            DoFuwa(forceDir4);

            _deltaForce[i] = Vector3.Distance(t.up, _defDir[i]);
            ReFuwa(i);

        }
    }



    void DoFuwa(in Vector4 swingDir4)
    {
        var i = (int)swingDir4.w;
        var t = bonesTransform[i];
        t.localRotation = CalculateRotate(swingDir4, i);
    }
    void ReFuwa(in int i)
    {
        Transform t = bonesTransform[i];
        float rebounceStrength = Mathf.Clamp(rebounceness.Evaluate(bonesLength[i] / maxLength), 0, 1);

        Quaternion returnRot = Quaternion.RotateTowards(t.localRotation, _defRot[i], _deltaForce[i] * rebounceStrength * 10);

        t.localRotation = returnRot;

    }


    Vector3 CaluculateSwingDirection()
    {
        Vector3 swingDir = (rootBone.position - _lastPos);
        swingDir += rootBone.up - _lastDir;

        return -swingDir;
    }

    private Vector3 ApplyDirectionalSuppression(Vector3 originalVector, int i)
    {
        Vector3 result = originalVector;


        for (int j = 0; j < suppressionStrengths.Length; j++)
        {
            Vector3 normalizedDir = Vector3.zero;
            if (j == 0 && suppressZ) normalizedDir = bonesTransform[i].forward;
            if (j == 1 && suppressX) normalizedDir = bonesTransform[i].right;
            if (j == 2 && suppressY) normalizedDir = bonesTransform[i].up;
            float dotProduct = Vector3.Dot(result, normalizedDir);
            Vector3 componentToSuppress = normalizedDir * dotProduct;

            // 抑制を適用
            result -= componentToSuppress * suppressionStrengths[j];
        }

        // 再正規化
        if (result.sqrMagnitude > 0.0001f)
        {
            result.Normalize();
        }

        return result;
    }

    Quaternion CalculateRotate(in Vector3 swingDir, in int i)
    {
        Transform t = bonesTransform[i];

        // ワールド座標系で軸と角度を計算
        Vector3 worldAxis = Vector3.Cross(t.up, t.up + swingDir).normalized;
        float angle = Vector3.SignedAngle(t.up, t.up + swingDir, worldAxis);

        // 軸が無効な場合（平行ベクトル）は現在の回転を返す
        if (worldAxis.sqrMagnitude < 0.001f)
        {
            return t.localRotation;
        }

        float hardnessStrength = 1 - Mathf.Clamp(hardness.Evaluate(bonesLength[i] / maxLength), 0, 1);
        float al = angleLimit * Mathf.Clamp(angleLimitness.Evaluate(bonesLength[i] / maxLength), 0, 1);
        angle = Mathf.Clamp(angle, -al, al);

        Quaternion fixedTwist = _defRot[i] * Quaternion.Inverse(t.localRotation);
        fixedTwist.x = 0;
        fixedTwist.z = 0;
        fixedTwist = fixedTwist.normalized;
        Quaternion fixedCurrentRot = fixedTwist * t.localRotation;

        Vector3 localAxis = t.InverseTransformDirection(worldAxis);

        Quaternion localTargetRotation = Quaternion.AngleAxis(angle, localAxis);
        Quaternion targetRot = localTargetRotation * fixedCurrentRot;

        return Quaternion.Slerp(fixedCurrentRot, targetRot, hardnessStrength);
    }

    [SerializeField] float glabDistance = 0.3f;
    public void SetHold(in Vector3 glabPos, in Transform glabHand)
    {
        _isHold = true;
        if (grabIndex < 0)
            for (int i = 0; i < grabPoint.Length; i++)
            {
                if ((grabPoint[i].position - glabPos).sqrMagnitude < glabDistance * glabDistance)
                {
                    for (int j = 0; j < bonesTransform.Length; i++)
                        if (grabPoint[i] == bonesTransform[j])
                        {
                            grabIndex = j;
                            break;
                        }
                    break;
                }
            }
    }

    public void SetHold(in Transform glabedTransform, in Vector3 glabPos)
    {
        _isHold = true;
        if (grabIndex < 0)
        {
            if (glabPos.y < Screen.height  / 4)
            {
                 SetGrabIndex(0);
            }
            else if (glabPos.y < Screen.height * 2 / 4)
            {
                SetGrabIndex(1);
            }
            else
            {
                SetGrabIndex(2);
            }
        }
    }

    void SetGrabIndex(in int index)
    {
        for (int i = 0; i < bonesTransform.Length; i++)
        {
            if (grabPoint[index] == bonesTransform[i])
            {
                grabIndex = i;
                break;
            }
        }
    }
    bool _isHold;
    public void SetRelease()
    {
        Debug.Log(grabIndex);
        if (!_isHold) return;
        _isHold = false;
        if(SteamVR.active)
        for (int i = 0; i < grabPoint.Length; i++)
            if (grabPoint[i] == bonesTransform[grabIndex]) grabPoint[i].parent = _grabedParent[i];
        grabIndex = -1;
    }
}

