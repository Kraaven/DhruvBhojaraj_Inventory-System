using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventorySlot : MonoBehaviour
{
    [Header("Slot Data")]
    public int InventorySlotID;
    public bool isOccupied => slotItems.Count > 0;

    [Header("References")]
    public BoxCollider slotCollider;

    private Stack<InventoryItem> slotItems = new Stack<InventoryItem>();
    private static HashSet<InventoryItem> itemsBeingPlaced = new HashSet<InventoryItem>();

    private void Awake()
    {
        if (!slotCollider)
            slotCollider = GetComponent<BoxCollider>();
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
        {
            InventoryUIBridge.instance.SetHoverState(InventorySlotID, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
            return;
        }

        if (!other.TryGetComponent(out InventoryItem inventoryItem)) return;

        if (itemsBeingPlaced.Contains(inventoryItem))
            return; 
        
        inventoryItem.grabInteractableReference.selectExited.RemoveListener(OnItemReleased);
        
        if (slotItems.Count > 0 && slotItems.Peek() == inventoryItem)
        {
            RemoveTopItem(false);
            InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
        }
    }

    private void OnItemReleased(SelectExitEventArgs args)
    {
        var item = args.interactableObject.transform.GetComponent<InventoryItem>();
        if (item == null) return;
        
        InventoryAPI.instance.TryAddItemToSlot(InventorySlotID, item);
        item.grabInteractableReference.selectExited.RemoveListener(OnItemReleased);
    }
    
    public bool CanAcceptItem(InventoryItem item)
    {
        if (slotItems.Count == 0) return true;

        var top = slotItems.Peek();
        return top.itemType == InventoryItem.ItemType.Accumulative && top.itemID == item.itemID;
    }
    
    public void PlaceItem(InventoryItem item, bool animate = true)
    {
        if (!CanAcceptItem(item)) return;
        
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
        
        InventoryUIBridge.instance.SetActiveState(InventorySlotID, true);
        InventoryUIBridge.instance.SetAmountState(InventorySlotID, slotItems.Count);
        InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
        
        if (animate)
        {
            item.transform.DOMove(slotCollider.bounds.center, 0.3f).SetEase(Ease.OutSine);
            item.transform.DORotateQuaternion(transform.rotation, 0.3f).SetEase(Ease.OutSine)
                .OnComplete(() => itemsBeingPlaced.Remove(item));
        }
        else
        {
            item.transform.position = slotCollider.bounds.center;
            item.transform.rotation = transform.rotation;
            itemsBeingPlaced.Remove(item);
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
        }
        
        if (slotItems.Count > 0)
        {
            var top = slotItems.Peek();
            if (top.itemType == InventoryItem.ItemType.Accumulative)
                top.gameObject.SetActive(true);
        }
        
        InventoryUIBridge.instance.SetActiveState(InventorySlotID, isOccupied);
        InventoryUIBridge.instance.SetAmountState(InventorySlotID, slotItems.Count);

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
    }
}
