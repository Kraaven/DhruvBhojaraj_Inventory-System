using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Crossbow : MonoBehaviour
{
    private BaseGrabInteractable grabInteractable;
    public Transform attachPoint;

    private void Start()
    {
        grabInteractable = GetComponent<BaseGrabInteractable>();
        
        grabInteractable.activated.AddListener(ShootCrossBow);
    }

    private void ShootCrossBow(ActivateEventArgs arg0)
    {
        var arrow = InventoryAPI.instance.RetrieveItemFromAnyInventorySlot(1);
        if (arrow == null) return;
        
        arrow.transform.SetParent(attachPoint);
        arrow.transform.localPosition = Vector3.zero;
        arrow.transform.localRotation = Quaternion.identity;
        
        arrow.transform.SetParent(null);
        arrow.rigidbodyReference.AddForce(arrow.transform.up * -10, ForceMode.Impulse);
    }

    private void OnDestroy()
    {
        grabInteractable.activated.RemoveListener(ShootCrossBow);
    }
}
