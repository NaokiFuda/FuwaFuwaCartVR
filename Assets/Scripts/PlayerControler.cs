using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.XR;
using UnityEngine.Events;

public class PlayerControler : MonoBehaviour
{
    Action moveMethod;
    Action jumpMethod;

    public SteamVR_Action_Boolean _moveForward = SteamVR_Input.GetBooleanAction("MoveForward");
    public SteamVR_Action_Boolean _moveBackward = SteamVR_Input.GetBooleanAction("MoveBackward");
    public SteamVR_Action_Boolean _moveRightward = SteamVR_Input.GetBooleanAction("MoveRightward");
    public SteamVR_Action_Boolean _moveLeftward = SteamVR_Input.GetBooleanAction("MoveLeftward");

    public SteamVR_Action_Vector2 _moveAction = SteamVR_Input.GetVector2Action("Move");
    public float speed = 8f;

    public SteamVR_Action_Boolean _turnRight = SteamVR_Input.GetBooleanAction("SmoothTurnRight");
    public SteamVR_Action_Boolean _turnLeft = SteamVR_Input.GetBooleanAction("SmoothTurnLeft");

    public SteamVR_Action_Vector2 _turnAction = SteamVR_Input.GetVector2Action("Turn");

    public SteamVR_Action_Boolean _jump = SteamVR_Input.GetBooleanAction("Jump");
    Rigidbody rb;
    CapsuleCollider col;
    Player player;
    [SerializeField] bool isFootFollowHead = false;
    [SerializeField] float leanAngle = 0.05f;

    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        col = GetComponent<CapsuleCollider>();
    }
    void Update()
    {
        if (XRSettings.isDeviceActive) Debug.Log("Yes");
        else Debug.Log("no");
        Vector2 input = _moveAction.GetAxis(SteamVR_Input_Sources.LeftHand);

        bool forwardInput = _moveForward.GetStateDown(SteamVR_Input_Sources.LeftHand);
        bool backwardInput = _moveBackward.GetStateDown(SteamVR_Input_Sources.LeftHand);
        bool rightwardInput = _moveRightward.GetStateDown(SteamVR_Input_Sources.LeftHand);
        bool leftwardInput = _moveLeftward.GetStateDown(SteamVR_Input_Sources.LeftHand);

        if (forwardInput) moveDirection = player.hmdTransform.forward;

        else if (backwardInput) moveDirection = -player.hmdTransform.forward;

        else if (rightwardInput) moveDirection = player.hmdTransform.right;

        else if (leftwardInput) moveDirection = -player.hmdTransform.right;

        else if (input.x != 0 || input.y != 0)
        {
            moveDirection = player.hmdTransform.forward * input.y;
            moveDirection += player.hmdTransform.right * input.x;
        }
        else moveDirection = Vector3.zero;

        moveDirection.y = 0;
        if (moveDirection != Vector3.zero) isMove = true;
        else isMove = false;


        bool rightTurnInput = _turnRight.GetState(SteamVR_Input_Sources.RightHand);
        bool leftTurnInput = _turnLeft.GetState(SteamVR_Input_Sources.RightHand);

        bool isJump = _jump.GetStateDown(SteamVR_Input_Sources.RightHand);
        Vector2 turnInput = _turnAction.GetAxis(SteamVR_Input_Sources.RightHand);
        if (isJump)
        {
            // ÉWÉÉÉìÉvèàóù
        }
        if (rightTurnInput|| leftTurnInput)
        {
            Turn(turnInput.x);
        }
        if (isFootFollowHead && Mathf.Abs(player.hmdTransform.rotation.x) < leanAngle && Mathf.Abs(player.hmdTransform.rotation.z) < leanAngle)
        {
            var hPos = player.hmdTransform.localPosition;
            col.center = new Vector3(hPos.x, rb.centerOfMass.y, hPos.z);
        }
       
    }
    Vector3 moveDirection;
    bool isMove;

    private void FixedUpdate()
    {
        if (isMove)
        {
            jumpMethod?.Invoke();
        }
    }
    private void Turn(float angle)
    {
        moveMethod?.Invoke();
    }
}
