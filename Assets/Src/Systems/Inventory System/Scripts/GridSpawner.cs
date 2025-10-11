using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [Header("Object to Spawn")]
    public InventorySlot prefab;
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    [Header("Grid Settings")]
    public Vector2Int xRange = new Vector2Int(-2, 2);
    public Vector2Int yRange = new Vector2Int(-3, 3);
    public int spacing = 2;
    
    public void Init()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab not assigned!");
            return;
        }

        SpawnGrid();
    }

    void SpawnGrid()
    {
        var SlotID = 0;
        for (int y = yRange.y; y >= yRange.x; y -= spacing)
        {
            for (int x = xRange.x; x <= xRange.y; x += spacing)
            {
                Vector3 localPosition = new Vector3(x, y, 0);
                InventorySlot obj = Instantiate(prefab, transform);
                inventorySlots.Add(obj);
                obj.transform.localPosition = localPosition;
                obj.InventorySlotID = SlotID;
                SlotID++;
            }
        }
    }
}