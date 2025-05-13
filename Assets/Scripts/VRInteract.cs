using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRInteract : MonoBehaviour
{
    [SerializeField] private UnityEvent interactEvent = new UnityEvent();

    public void OnInteract()
    {
        interactEvent?.Invoke();
    }
}
