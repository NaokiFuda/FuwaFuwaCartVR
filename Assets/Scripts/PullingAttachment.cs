using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PullingAttachment : MonoBehaviour
{
    [SerializeField] GameObject pullingObj;
   // [SerializeField] float tolerance = 0.03f;
    Vector3 objCross;
    bool toAbove;
    bool toBelow;
    Vector3 offset;
    Transform pullingParent;
    private void OnEnable()
    {
        pullingParent = pullingObj.transform.parent.transform;
        offset = pullingParent.transform.position - transform.position;
    }
    void Update()
    {
        objCross = Vector3.Cross(transform.position - pullingObj.transform.position, -pullingObj.transform.forward);
        if (pullingObj.transform.rotation.x > 0.73f )
        {
            toAbove = false;
            toBelow = true;
        }
        else if (pullingObj.transform.rotation.x < -0.3f)
        {
            toBelow = false;
            toAbove = true;
        }
        else if (objCross.x ==0)
        {
            toAbove = false;
            toBelow = false;
        }
        else if ( objCross.x < 0)
        {
            toAbove = true;
        }
            
        else if (objCross.x > 0)
        {
            toBelow = true;
        }
    }
    void FixedUpdate()
    {
        if (toAbove)
        {
            pullingObj.transform.rotation *= new Quaternion(0.01f, 0, 0, 0.99f);
        }
        
        if (toBelow)
        {
            pullingObj.transform.rotation *= new Quaternion(-0.01f, 0, 0, 0.99f);
        }
        pullingParent.localPosition = new Vector3(offset.x, pullingParent.localPosition.y,offset.z);
        pullingParent.rotation = transform.rotation;
    }

}
