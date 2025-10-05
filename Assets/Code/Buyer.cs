using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buyer : MonoBehaviour
{
    [field: SerializeField]
    public Item.ItemType itemType { get; private set; }
    
    [field: SerializeField]
    public int price { get; private set; }
    
    [field: SerializeField]
    public Collider collider { get; private set; }
    
    [field: SerializeField]
    public Highlighter highlighter { get; private set; }
    
    [field: SerializeField]
    public List<Item> boughtItems { get; private set; } = new List<Item>();
    
    [SerializeField]
    private BarkDataSo firstBuyData;
    
    [SerializeField]
    private Animator animator;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        highlighter.Init();
    }

    private bool firstBuy = true;
    public void SellItemToBuyer(Character character)
    {
        Item itemToRemove = null;
        foreach (var item in character.Inventory)
        {
            if (item.itemType == itemType)
            {
                itemToRemove = item;
                break;
            }
        }

        if (itemToRemove == null)
            return;
        character.Inventory.Remove(itemToRemove);
        character.money += price;
        itemToRemove.gameObject.SetActive(false);
        boughtItems.Add(itemToRemove);
        animator.SetTrigger("Move");
        EventBus.Publish(new EventBusItemSold{ item = itemToRemove });
        if (firstBuy)
        {
            EventBus.Publish(new EventBusBarkTriggered { barkData = firstBuyData });
            firstBuy = false;
        }
    }
}
