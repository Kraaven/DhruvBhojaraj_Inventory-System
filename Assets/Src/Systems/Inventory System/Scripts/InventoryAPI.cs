using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryAPI : MonoBehaviour
{
    public static InventoryAPI instance { private set; get; }

    private GridSpawner gridSpawner;
    
    private InventorySlot _cached_GetFirstSlotContainingItem;
    private InventorySlot _cached_RetrieveItemFromAnyInventorySlot;

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(this); return; }

        gridSpawner = GetComponent<GridSpawner>();
    }

    private void Start()
    {
        
        _cached_GetFirstSlotContainingItem = gridSpawner.inventorySlots.FirstOrDefault();
        _cached_RetrieveItemFromAnyInventorySlot = gridSpawner.inventorySlots.FirstOrDefault();
    }
    
    public InventorySlot GetSlotByID(int id)
    {
        return gridSpawner.inventorySlots[id];
    }
    
    public IEnumerable<InventorySlot> GetOccupiedSlots()
    {
        return gridSpawner.inventorySlots.Where(s => s.isOccupied);
    }
    
    public IEnumerable<InventorySlot> GetEmptySlots()
    {
        return gridSpawner.inventorySlots.Where(s => !s.isOccupied);
    }
    
    public InventorySlot GetFirstSlotContainingItem(int itemID)
    {
        if (_cached_GetFirstSlotContainingItem != null)
        {
            var topItem = _cached_GetFirstSlotContainingItem.PeekTopItem();
            if (topItem != null && topItem.itemID == itemID)
                return _cached_GetFirstSlotContainingItem;
        }
        
        foreach (var slot in gridSpawner.inventorySlots)
        {
            var topItem = slot.PeekTopItem();
            if (topItem != null && topItem.itemID == itemID)
            {
                _cached_GetFirstSlotContainingItem = slot;
                return slot;
            }
        }

        return null;
    }
    
    public InventoryItem RetrieveItemFromAnyInventorySlot(int itemID)
    {
        InventoryItem item;
        
        if (_cached_RetrieveItemFromAnyInventorySlot != null)
        {
            var peekedItem = _cached_RetrieveItemFromAnyInventorySlot.PeekTopItem();
            if (peekedItem != null && peekedItem.itemID == itemID)
            {
                item = _cached_RetrieveItemFromAnyInventorySlot.RemoveTopItem();
                PrepareItemForWorld(item);
                return item;
            }
        }
        
        var slot = GetFirstSlotContainingItem(itemID);
        if (slot == null)
            return null;

        item = slot.RemoveTopItem();
        _cached_RetrieveItemFromAnyInventorySlot = slot;

        PrepareItemForWorld(item);
        return item;
    }
    
    public bool TryAddItemToSlot(int slotID, InventoryItem item)
    {
        var slot = GetSlotByID(slotID);
        if (slot == null)
        {
            Debug.LogWarning($"InventoryAPI: Slot {slotID} not found.");
            return false;
        }

        if (!slot.CanAcceptItem(item))
            return false;

        slot.PlaceItem(item);
        return true;
    }
    
    public bool TryAddItemToFirstAvailableSlot(InventoryItem item)
    {
        foreach (var slot in gridSpawner.inventorySlots)
        {
            if (slot.CanAcceptItem(item))
            {
                slot.PlaceItem(item);
                return true;
            }
        }

        return false;
    }
    
    private void PrepareItemForWorld(InventoryItem item)
    {
        if (item == null) return;

        item.gameObject.SetActive(true);
        item.rigidbodyReference.isKinematic = false;
        item.ResetScale(true);
    }
    
}
