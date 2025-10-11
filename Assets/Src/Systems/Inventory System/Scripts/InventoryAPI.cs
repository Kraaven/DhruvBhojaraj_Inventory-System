using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryAPI : MonoBehaviour
{
    public static InventoryAPI instance { private set; get; }
    
    private GridSpawner gridSpawner;

    private Dictionary<int, InventorySlot> _itemSlotCache = new Dictionary<int, InventorySlot>();
    public void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this);
        
        gridSpawner = GetComponent<GridSpawner>();
    }

    private void Start()
    {
        _cached_GetFirstSlotContainingItem = gridSpawner.inventorySlots[0];
        _cached_RetrieveItemFromAnyInventorySlot = gridSpawner.inventorySlots[0];
    }

    public InventorySlot GetSlotByID(int id) 
        => gridSpawner.inventorySlots.FirstOrDefault(slot => slot.InventorySlotID == id);
    
    public IEnumerable<InventorySlot> GetOccupiedSlots()
        => gridSpawner.inventorySlots.Where(s => s.isOccupied);
    
    public IEnumerable<InventorySlot> GetEmptySlots()
        => gridSpawner.inventorySlots.Where(s => !s.isOccupied);

    private InventorySlot _cached_GetFirstSlotContainingItem;
    public InventorySlot GetFirstSlotContainingItem(int itemID)
    {
       

        if (_cached_GetFirstSlotContainingItem.slotItems.TryPeek(out InventoryItem item))
            if(item.itemID == itemID) return _cached_GetFirstSlotContainingItem;
        
        foreach (var slot in gridSpawner.inventorySlots.Where(slot => slot.slotItems.TryPeek(out InventoryItem result) && result.itemID == itemID))
        {
            _cached_GetFirstSlotContainingItem = slot;
            return _cached_GetFirstSlotContainingItem;
        }
        
        return null;
    }
    
    private InventorySlot _cached_RetrieveItemFromAnyInventorySlot;
    public InventoryItem RetrieveItemFromAnyInventorySlot(int itemID)
    {
        InventoryItem item;
        if (_cached_RetrieveItemFromAnyInventorySlot.slotItems.TryPeek(out item)) {
            _cached_RetrieveItemFromAnyInventorySlot.RemoveItemFromSlot(item);
            return item; }

        var slot = GetFirstSlotContainingItem(itemID);
        if(!slot.slotItems.TryPeek(out item)) return null; 
        _cached_RetrieveItemFromAnyInventorySlot = slot;
        _cached_RetrieveItemFromAnyInventorySlot.RemoveItemFromSlot(item);
        item.gameObject.SetActive(true);
        item.rigidbodyReference.isKinematic = false;
        item.ResetScale(true);
        return item;
    }

}
