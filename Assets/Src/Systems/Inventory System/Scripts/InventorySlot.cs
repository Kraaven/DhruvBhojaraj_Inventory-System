using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(BoxCollider))]
public class InventorySlot : MonoBehaviour
{
    public int InventorySlotID;
    public bool isOccupied => slotItems.Count > 0;
    public BoxCollider slotCollider;

    private readonly Stack<InventoryItem> slotItems = new Stack<InventoryItem>();
    private static readonly HashSet<InventoryItem> itemsBeingPlaced = new HashSet<InventoryItem>();

    private void Awake()
    {
        if (!slotCollider) slotCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out InventoryItem inventoryItem))
        {
            bool canInsert = CanAcceptItem(inventoryItem);

            if (canInsert)
            {
                InventoryUIBridge.instance.SetHoverState(InventorySlotID, true);
                inventoryItem.grabInteractableReference.selectExited.RemoveListener(OnItemReleased);
                inventoryItem.grabInteractableReference.selectExited.AddListener(OnItemReleased);
                return;
            }
        }

        if (other.CompareTag("Hand"))
            InventoryUIBridge.instance.SetHoverState(InventorySlotID, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
            return;
        }

        if (!other.TryGetComponent(out InventoryItem inventoryItem)) return;
        if (itemsBeingPlaced.Contains(inventoryItem)) return;

        inventoryItem.grabInteractableReference.selectExited.RemoveListener(OnItemReleased);

        if (slotItems.Count > 0 && slotItems.Peek() == inventoryItem)
        {
            RemoveTopItem(true);
            InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
        }
    }

    private void OnItemReleased(SelectExitEventArgs args)
    {
        var item = args.interactableObject.transform.GetComponent<InventoryItem>();
        if (item == null) return;

        if (InventoryAPI.instance != null)
        {
            var added = InventoryAPI.instance.TryAddItemToSlot(InventorySlotID, item);
            if (!added) PlaceItem(item, true);
        }
        else PlaceItem(item, true);

        item.grabInteractableReference.selectExited.RemoveListener(OnItemReleased);
    }

    public bool CanAcceptItem(InventoryItem item)
    {
        if (item == null) return false;
        if (slotItems.Count == 0) return true;

        var top = slotItems.Peek();
        return top.itemType == InventoryItem.ItemType.Accumulative && top.itemID == item.itemID;
    }

    public void PlaceItem(InventoryItem item, bool animate = true)
    {
        if (item == null || !CanAcceptItem(item)) return;

        InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);

        if (slotItems.Count > 0)
        {
            var previous = slotItems.Peek();
            if (previous.itemType == InventoryItem.ItemType.Accumulative && previous.itemID == item.itemID)
                previous.gameObject.SetActive(false);
        }

        itemsBeingPlaced.Add(item);
        item.rigidbodyReference.isKinematic = true;
        item.rigidbodyReference.linearVelocity = Vector3.zero;
        item.rigidbodyReference.angularVelocity = Vector3.zero;

        slotItems.Push(item);
        item.ASSINGNED_SLOT = this;

        InventoryUIBridge.instance.SetActiveState(InventorySlotID, isOccupied);

        var top = slotItems.Peek();
        if (top.itemType == InventoryItem.ItemType.Accumulative)
            InventoryUIBridge.instance.SetAmountState(InventorySlotID, slotItems.Count);
        else
            InventoryUIBridge.instance.SetAmountState(InventorySlotID, 0);

        InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);

        if (animate && slotCollider != null)
        {
            item.transform.DOMove(slotCollider.bounds.center, 0.3f).SetEase(Ease.OutSine);
            item.transform.DORotateQuaternion(transform.rotation, 0.3f).SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    itemsBeingPlaced.Remove(item);
                    item.transform.position = slotCollider.bounds.center;
                    item.transform.rotation = transform.rotation;
                    item.ResetScale(true);
                    InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
                });
        }
        else
        {
            if (slotCollider != null)
            {
                item.transform.position = slotCollider.bounds.center;
                item.transform.rotation = transform.rotation;
            }

            itemsBeingPlaced.Remove(item);
            item.ResetScale(true);
            InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
        }
    }

    public InventoryItem RemoveTopItem(bool reactivate = true)
    {
        if (slotItems.Count == 0) return null;

        var item = slotItems.Pop();
        item.ASSINGNED_SLOT = null;

        if (reactivate)
        {
            item.rigidbodyReference.isKinematic = false;
            item.gameObject.SetActive(true);
            item.ResetScale(true);
        }

        if (slotItems.Count > 0)
        {
            var next = slotItems.Peek();
            next.gameObject.SetActive(true);

            if (slotCollider != null)
            {
                next.transform.position = slotCollider.bounds.center;
                next.transform.rotation = transform.rotation;
            }

            next.ResetScale(true);

            if (next.itemType == InventoryItem.ItemType.Accumulative)
                InventoryUIBridge.instance.SetAmountState(InventorySlotID, slotItems.Count);
            else
                InventoryUIBridge.instance.SetAmountState(InventorySlotID, 0);

            InventoryUIBridge.instance.SetActiveState(InventorySlotID, true);
        }
        else
        {
            InventoryUIBridge.instance.SetActiveState(InventorySlotID, false);
            InventoryUIBridge.instance.SetAmountState(InventorySlotID, 0);
        }

        InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
        return item;
    }

    public InventoryItem PeekTopItem()
    {
        return slotItems.Count > 0 ? slotItems.Peek() : null;
    }

    public int GetItemCount() => slotItems.Count;

    public void Pack()
    {
        foreach (var slotItem in slotItems)
            slotItem.transform.SetParent(transform);
    }

    public void UnPack(Transform parent)
    {
        foreach (var slotItem in slotItems)
            slotItem.transform.SetParent(parent);
        if (slotItems.TryPeek(out InventoryItem topItem))
        {
            topItem.Shrink(InventoryMainBox.GetAutoFitScaleFactor(topItem.objectBounds.size));
            InventoryUIBridge.instance.SetActiveState(InventorySlotID, true);
        }
        InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
        
        
    }
}
