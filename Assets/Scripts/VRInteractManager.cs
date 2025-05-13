using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRInteractManager : MonoBehaviour
{
    SteamVR_Action_Boolean interacted = SteamVR_Actions._default.InteractUI;
    SteamVR_Action_Boolean drop = SteamVR_Actions._default.GrabGrip;
    
    [SerializeField]  GameObject leftHand;
    [SerializeField]  GameObject rightHand;
    [SerializeField] float _rayRange;
    [SerializeField] LayerMask layerMask;
    [SerializeField] 
    LayerMask allMask = -1;
    List<GameObject> holdObjectInLeftHand = new List<GameObject>();
    List<GameObject> holdObjectInRightHand = new List<GameObject>();
    private void Start()
    {
        layerMask = layerMask | (1 << LayerMask.NameToLayer("Pickup"));
    }
    private void Update()
    {
        Ray leftInteractingRay = new Ray(leftHand.transform.position, leftHand.transform.GetChild(1).forward);
        ;
        Ray rightInteractingRay = new Ray(rightHand.transform.position, rightHand.transform.GetChild(1).forward);
        ;
        RaycastHit hit = new RaycastHit();
        if (interacted.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            if (Physics.Raycast(leftInteractingRay, out hit, _rayRange, layerMask) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Pickup") && hit.collider.gameObject.GetComponent<VRPickup>())
            {
                holdObjectInLeftHand.Add(hit.collider.gameObject);
                hit.collider.gameObject.GetComponent<VRPickup>().PickUp(leftHand);
            }
            if(Physics.Raycast(leftInteractingRay, out hit, _rayRange , allMask) && hit.collider.gameObject.GetComponent<VRInteract>())
            {
                hit.collider.gameObject.GetComponent<VRInteract>().OnInteract();
            }
        }
        if (interacted.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            if (Physics.Raycast(rightInteractingRay, out hit, _rayRange, layerMask) && hit.collider.gameObject.layer == LayerMask.NameToLayer("Pickup") && hit.collider.gameObject.GetComponent<VRPickup>())
            {
                holdObjectInRightHand.Add(hit.collider.gameObject);
                hit.collider.gameObject.GetComponent<VRPickup>().PickUp(rightHand);
            }
            if (Physics.Raycast(rightInteractingRay, out hit, _rayRange, allMask) && hit.collider.gameObject.GetComponent<VRInteract>())
            {
                hit.collider.gameObject.GetComponent<VRInteract>().OnInteract();
            }
        }
        if (interacted.GetStateUp(SteamVR_Input_Sources.LeftHand))
        {
            if ( holdObjectInLeftHand.Count>0)
            {
                var VRPick = holdObjectInLeftHand[0].GetComponent<VRPickup>();
                if (VRPick.canHold && !VRPick.holdingRock || !VRPick.canHold)
                {
                    holdObjectInLeftHand.Remove(holdObjectInLeftHand[0]);
                }
                VRPick.Drop(leftHand, false);
            }
        }
        if (interacted.GetStateUp(SteamVR_Input_Sources.RightHand))
        {
            if (holdObjectInRightHand.Count > 0)
            {
                var VRPick = holdObjectInRightHand[0].GetComponent<VRPickup>();
                if (VRPick.canHold && !VRPick.holdingRock || !VRPick.canHold) holdObjectInRightHand.Remove(holdObjectInRightHand[0]);
                VRPick.Drop(rightHand, false);
            }
        }
        if (drop.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            if (holdObjectInLeftHand.Count > 0)
            {
                foreach(GameObject grabItems in holdObjectInLeftHand)
                {
                    var VRPick = grabItems.GetComponent<VRPickup>();
                    VRPick.Drop(leftHand, true);
                }
                holdObjectInLeftHand.Clear();
            }
        }
        if (drop.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            if (holdObjectInRightHand.Count > 0)
            {
                foreach(GameObject grabItems in holdObjectInRightHand)
                {
                    var VRPick = grabItems.GetComponent<VRPickup>();
                    VRPick.Drop(rightHand, true);
                }
                holdObjectInRightHand.Clear();
            }
        }
    }
}
