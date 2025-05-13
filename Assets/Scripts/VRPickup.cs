using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class VRPickup : MonoBehaviour
{
   Transform lastParent;
    [HideInInspector]
    public bool canHold;
    [HideInInspector]
    public bool holdingRock;
    [SerializeField] public string interactionText;
    void Start()
    {
        lastParent = transform.parent;
    }

    public void PickUp(GameObject target)
    {
        Debug.Log(target + " grab!!!!!");
        transform.SetParent(target.transform, true);
        transform.position = target.transform.position;
        holdingRock = true;
        hand = target;
    }
    GameObject hand;
    public void Drop(GameObject target , bool alldrop)
    {
        if (!alldrop)
        {
            if (hand != target) { return; }
            if (canHold && holdingRock) {holdingRock = false; return; }
        }
        transform.SetParent(lastParent, true);
    }
}
