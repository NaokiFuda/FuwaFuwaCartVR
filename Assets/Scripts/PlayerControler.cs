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
    public Action<Vector3> moveMethod;
    public Action jumpMethod;
    [SerializeField] Transform playerLeftHand;
    [SerializeField] Transform playerRightHand;

    [SerializeField] Transform[] desktopHands;
    [SerializeField] Transform glabPoint;

    [SerializeField] FuwaFuwaMovement fuwaFuwaMovement;

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

        Cursor.visible = false;           // カーソルを非表示
        Cursor.lockState = CursorLockMode.Locked;  // 画面中央にロック（任意）

    }
    void Update()
    {
        bool isJump = false;
        bool rightTurnInput = false;
        bool leftTurnInput = false;
        Vector2 turnInput = Vector2.zero;

        if (SteamVR.active)
        {

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
                moveDirection = player.hmdTransform.right * input.x;
            }
            else moveDirection = Vector3.zero;

            moveDirection.y = 0;
            


            rightTurnInput = _turnRight.GetState(SteamVR_Input_Sources.RightHand);
            leftTurnInput = _turnLeft.GetState(SteamVR_Input_Sources.RightHand);

            isJump = _jump.GetStateDown(SteamVR_Input_Sources.RightHand);
            turnInput = _turnAction.GetAxis(SteamVR_Input_Sources.RightHand);
            
            
            if (isFootFollowHead && Mathf.Abs(player.hmdTransform.rotation.x) < leanAngle && Mathf.Abs(player.hmdTransform.rotation.z) < leanAngle)
            {
                var hPos = player.hmdTransform.localPosition;
                col.center = new Vector3(hPos.x, rb.centerOfMass.y, hPos.z);
            }
        }
        var verticalInput = Input.GetAxis("Vertical");
        var horizontalInput = Input.GetAxis("Horizontal");
        if (verticalInput != 0) moveDirection.z = verticalInput;
        if(horizontalInput != 0) moveDirection.x = horizontalInput;

        var mouseMovement = Input.GetAxis("Mouse X");
        if (mouseMovement != 0) Turn(mouseMovement * mouseSensitivity);

        if (moveDirection != Vector3.zero) isMove = true;
        else isMove = false;
        if (isJump)
        {
            jumpMethod?.Invoke();
        }
        if (rightTurnInput || leftTurnInput)
        {
            Turn(turnInput.x);
        }

        bool leftClickHold = Input.GetMouseButton(0);
        bool leftClickUp = Input.GetMouseButtonUp(0);
        if(leftClickHold)
        {
            if (SteamVR.active)
                fuwaFuwaMovement.SetHold(glabPoint, playerLeftHand.position, playerLeftHand);
            else
                fuwaFuwaMovement.SetHold(glabPoint, Input.mousePosition);
        }
        if(leftClickUp)
            { fuwaFuwaMovement.SetRelease();  }
    }
    Vector3 moveDirection;
    bool isMove;
    [SerializeField] float mouseSensitivity = 0.5f;

    private void FixedUpdate()
    {
        if (isMove)
        {
            moveMethod?.Invoke(moveDirection);
        }
    }
    private void Turn(float angle)
    {
        player.transform.RotateAround(player.hmdTransform.position, Vector3.up, angle);
    }
}
