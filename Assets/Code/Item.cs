using UnityEngine;

public class Item : MonoBehaviour
{
    [field: SerializeField]
    public string itemName { get; set; }
    [field: SerializeField]
    public ItemType itemType { get; set; }
    [SerializeField]
    private int    itemCost;
    [SerializeField]
    private GameObject itemVisual;
    [SerializeField]
    private Rigidbody rb;
    public Rigidbody Rb => rb;
    [SerializeField]
    private Collider collider;
    public Collider Collider => collider;
    [SerializeField]
    private int weight = 1;
    public int Weight { get { return weight; } }
    public int hp = 5;
    
    [field: SerializeField]
    public Highlighter highlighter { get; private set; }

    public enum ItemType
    {
        Coal,
        Rock
    }
    
    private void Start()
    {
        Init();
    }
    
    private void Init()
    {
        highlighter.Init();
    }
    
    public void OnPickUp()
    {
        EventBus.Publish(new EventBusItemPickedUp {item = this});
        gameObject.SetActive(false);
    }

    private void OnSpawn()
    {
        
    }
    
}
