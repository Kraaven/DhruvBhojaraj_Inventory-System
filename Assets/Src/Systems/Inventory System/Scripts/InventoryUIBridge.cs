using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUIBridge : MonoBehaviour
{
    private VisualElement root;

    private List<VisualElement> UISlots;
    public static InventoryUIBridge instance;

    private void Awake()
    {
        if(instance == null) instance = this;
    }

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        UISlots = root.Query<VisualElement>(className: "inventory-slot__root").ToList();
    }

    public void SetHoverState(int slotID, bool state)
    {
        if(state) UISlots[slotID].AddToClassList("hover");
        else UISlots[slotID].RemoveFromClassList("hover");
    }

    public void SetActiveState(int slotID, bool state)
    {
        if(state) UISlots[slotID].AddToClassList("active");
        else UISlots[slotID].RemoveFromClassList("active");
    }

    public void SetAmountState(int slotID, int amount)
    {
        if(amount == 0) UISlots[slotID].RemoveFromClassList("stackable");
        else
        {
            UISlots[slotID].AddToClassList("stackable");
            UISlots[slotID].Q<Label>(className:"inventory-slot__item-amount-value").text = $"x{amount}";
        }
    }

}
