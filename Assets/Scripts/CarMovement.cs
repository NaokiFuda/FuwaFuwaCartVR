using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [SerializeField] PlayerControler playerControler;
    [SerializeField] WheelCollider[] tire;
    [SerializeField] bool getLogMessage;

    public float moterForce = 1000f;
    public float steerAngle = 30f;

    private void OnEnable()
    {
        playerControler.moveMethod += Moving;
    }
    private void OnDisable()
    {
        playerControler.moveMethod -= Moving;
    }
    void Moving(Vector3 force)
    {
        SetDebugLogMessage("moving");
        float motor = moterForce * force.z;
        float angle = steerAngle * force.x;
        foreach (var t in tire) 
        { 
            t.motorTorque = Mathf.Lerp(0,motor,0.1f);
            t.steerAngle = angle;
        }
    }
    void SetDebugLogMessage(object message)
    {
        if (!getLogMessage) return;
        Debug.Log(message);
    }
}
