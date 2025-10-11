using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InventorySlot : MonoBehaviour
{
    public BoxCollider slotCollider;
    public bool isOccupied = false;
    public Stack<InventoryItem> slotItems = new Stack<InventoryItem>();
    public int InventorySlotID;
    public int ItemCount;
    private static HashSet<InventoryItem> itemsBeingPlaced = new HashSet<InventoryItem>();

    private void Start()
    {
        slotCollider = GetComponent<BoxCollider>();
    }

    public void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent(out InventoryItem inventoryItem))
        {
            bool canBeInserted = false;
            if (slotItems.Count == 0) canBeInserted = true;
            else
            {
                var topItem = slotItems.Peek();
                // print($"Entered Item : {inventoryItem.name}, {inventoryItem.itemID}\nExisting Item : {topItem.name}, {topItem.itemID}");
                if (topItem.itemType == InventoryItem.ItemType.Accumulative && topItem.itemID == inventoryItem.itemID)
                    canBeInserted = true;
            }

            if (canBeInserted)
            {
                InventoryUIBridge.instance.SetHoverState(InventorySlotID, true);
                inventoryItem.grabInteractableReference.selectExited.RemoveListener(OnItemReleased);
                inventoryItem.grabInteractableReference.selectExited.AddListener(OnItemReleased);
                return;
            }
        }
        
        if (other.CompareTag("Hand")) InventoryUIBridge.instance.SetHoverState(InventorySlotID, true);

        
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
            return;
        }

        if (!other.TryGetComponent(out InventoryItem inventoryItem)) return;
        
        if (itemsBeingPlaced.Contains(inventoryItem))
        {
            // print($"{other.name} Exited from Slot {InventorySlotID} (ignored - being placed)");
            return;
        }
        
        
    }

    private void OnItemReleased(SelectExitEventArgs args)
    {
        var item = args.interactableObject.transform.GetComponent<InventoryItem>();
        if (item == null) return;
        
        // print($"Released {item.name} into Slot {InventorySlotID}");

        PlaceItemInSlot(item);
        item.grabInteractableReference.selectExited.RemoveListener(OnItemReleased);
        
        // print($"Removed Listener from {item.name} into Slot {InventorySlotID}");
    }

    private void PlaceItemInSlot(InventoryItem item)
    {
        if (slotItems.Count > 0)
        {
            var previous = slotItems.Peek();
            if (previous.itemType == InventoryItem.ItemType.Accumulative && previous.itemID == item.itemID) previous.gameObject.SetActive(false);
        }
        
        itemsBeingPlaced.Add(item);
        
        item.rigidbodyReference.isKinematic = true; 
        item.rigidbodyReference.linearVelocity = Vector3.zero;
        item.rigidbodyReference.angularVelocity = Vector3.zero;
        
        slotItems.Push(item);
        item.ASSINGNED_SLOT = this;
        isOccupied = true;
        
        InventoryUIBridge.instance.SetActiveState(InventorySlotID, true);
        InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);

        ItemCount = slotItems.Count;
        
        if(slotItems.Peek().itemType == InventoryItem.ItemType.Accumulative) 
            InventoryUIBridge.instance.SetAmountState(InventorySlotID, slotItems.Count);
        
        // Start the tween animation
        item.transform.DOMove(slotCollider.bounds.center, 0.3f).SetEase(Ease.OutSine);
        item.transform.DORotateQuaternion(transform.rotation, 0.3f).SetEase(Ease.OutSine)
            .OnComplete(() => {
                itemsBeingPlaced.Remove(item);
                // print($"Item {item.name} placement complete in Slot {InventorySlotID}");
                InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
            });
        
        // print($"Pushed {item.name} into Slot {InventorySlotID}");
    }

    public void RemoveItemFromSlot(InventoryItem item)
    {
        item.grabInteractableReference.selectExited.RemoveListener(OnItemReleased);
        
        print($"{item.name} Exited from Slot {InventorySlotID}");
        
        if (slotItems.TryPeek(out InventoryItem itemBeingRemoved) && itemBeingRemoved == item)
        {
            item.rigidbodyReference.isKinematic = false;
            slotItems.Pop();
            InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
            item.ASSINGNED_SLOT = null;
            ItemCount = slotItems.Count;
            
            // print($"Removed {other.name} from Slot {InventorySlotID}, movement granted");
            
            if (slotItems.Count > 0)
            {
                var nextItem = slotItems.Peek();
                nextItem.gameObject.SetActive(true);
                isOccupied = true;
                // print($"{other.name} Exited from Slot {InventorySlotID}, Slot actively hosting {nextItem.name}");
            }
            else
            {
                isOccupied = false;
                InventoryUIBridge.instance.SetActiveState(InventorySlotID, false);
                slotItems.Clear();
                // print($"{other.name} Exited from Slot {InventorySlotID}, Slot Empty");
            }
            
            InventoryUIBridge.instance.SetAmountState(InventorySlotID, slotItems.Count);
        }
        
        InventoryUIBridge.instance.SetHoverState(InventorySlotID, false);
    }

    public void Pack()
    {
        foreach (var slotItem in slotItems)
        {
            slotItem.transform.SetParent(this.transform);
        }
    }

    public void UnPack(Transform parent)
    {
        foreach (var slotItem in slotItems)
        {
            slotItem.transform.SetParent(parent);
        }
    }
}