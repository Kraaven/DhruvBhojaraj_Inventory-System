using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventorySlot : MonoBehaviour
{
    [Header("Settings")]
    public float snapDuration = 0.4f;

    [SerializeField] private BaseGrabInteractable currentItem;
    [SerializeField] private bool isHandHovering;
    public int InventorySlotID;

    private void OnTriggerEnter(Collider other)
    {
        print($"OnTriggerEnter: {other.name}");
        if (other.CompareTag("Hand"))
        {
            InventoryUIBridge.instance.HoverSlot(InventorySlotID);
            isHandHovering = true;
        }

        if (other.CompareTag("Interactable"))
        {
            print("Interactable Entered");
            BaseGrabInteractable grab = other.GetComponent<BaseGrabInteractable>();
            if (grab != null)
            {
                // If slot already has an item, ignore
                if (currentItem != null && currentItem != grab)
                    return;

                grab.selectExited.AddListener(OnGrabReleased);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        print($"OnTriggerExit: {other.name}");
        if (other.CompareTag("Hand"))
        {
            InventoryUIBridge.instance.UnHoverSlot(InventorySlotID);
            isHandHovering = false;
        }

        if (other.CompareTag("Interactable"))
        {
            print("Interactable Exited");
            BaseGrabInteractable grab = other.GetComponent<BaseGrabInteractable>();
            if (grab != null)
            {
                grab.selectExited.RemoveListener(OnGrabReleased);

                if (currentItem == grab)
                {
                    currentItem = null;
                    InventoryUIBridge.instance.UnActiveSlot(InventorySlotID);
                }
            }
        }
    }

    private void OnGrabReleased(SelectExitEventArgs args)
    {
        BaseGrabInteractable releasedItem = args.interactableObject as BaseGrabInteractable;
        if (releasedItem == null)
            return;

        // Check if this slot is already occupied
        if (currentItem != null && currentItem != releasedItem)
            return;

        // Only snap if the object is still inside the slot trigger
        Collider itemCollider = releasedItem.GetComponent<Collider>();
        Collider slotCollider = GetComponent<Collider>();
        if (!itemCollider.bounds.Intersects(slotCollider.bounds))
            return;

        currentItem = releasedItem;

        // Smoothly move & align item to slot
        releasedItem.transform.DOMove(transform.position, snapDuration).SetEase(Ease.OutQuad);
        releasedItem.transform.DORotateQuaternion(transform.rotation, snapDuration);

        // Disable grabbing temporarily while it settles
        releasedItem.enabled = false;
        DOVirtual.DelayedCall(snapDuration, () => {
            if (releasedItem != null)
                releasedItem.enabled = true;
        });
        
        InventoryUIBridge.instance.ActiveSlot(InventorySlotID);
        currentItem.DisableMovement();
    }

    public bool IsOccupied => currentItem != null;

    public BaseGrabInteractable GetCurrentItem() => currentItem;

    public void ClearSlot()
    {
        if (currentItem != null)
        {
            currentItem = null;
            InventoryUIBridge.instance.UnActiveSlot(InventorySlotID);
        }
    }
}
