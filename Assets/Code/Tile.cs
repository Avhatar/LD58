using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [field: SerializeField]
    public Highlighter highlighter { get; private set; }
    [field: SerializeField]
    public AudioSource audioSourceDig { get; private set; }
    [field: SerializeField]
    public AudioSource audioSourceDestroy { get; private set; }
    [field: SerializeField]
    public List<Item>  relatedItems { get; private set; }
    [field: SerializeField]
    public int itemsRandomAmount { get; private set; } = 0;
    
    private enum TileType
    {
        Rock,
        Coal
    }
    
    [SerializeField]
    private TileType tileType;
    
    [SerializeField]
    private Collider collider;
    public Collider Collider  => collider;

    [SerializeField]
    private GameObject visual;
    
    [SerializeField]
    private int hp = 5;
    
    void Start()
    {
        Init();
    }
    
    private void Init()
    {
        highlighter = GetComponent<Highlighter>();
        highlighter.Init();
    }

    public void Damage(int damage)
    {
        hp -= damage;
        audioSourceDig.clip = Core.instance.globalSettings.onDigHitSounds[Random.Range(0, Core.instance.globalSettings.onDigHitSounds.Count)];
        audioSourceDig.Play();
        Debug.Log("Tile damaged " + damage);
        if (hp <= 0)
            TileDestroy();
    }

    private void TileDestroy()
    {
        EventBus.Publish(new EventBusTileDestroyed { tile = this });
        Core.instance.globalAudioSource.clip = Core.instance.globalSettings.onTileCrushedSound;
        Core.instance.globalAudioSource.spatialBlend = audioSourceDestroy.spatialBlend;
        Core.instance.globalAudioSource.volume = audioSourceDestroy.volume;
        Core.instance.globalAudioSource.pitch = audioSourceDestroy.pitch;
        Core.instance.globalAudioSource.Play();
        foreach (Item item in relatedItems)
        {
            int randomAmount = Random.Range(1, Mathf.Clamp(itemsRandomAmount, 1, itemsRandomAmount));
            Debug.Log("Random amount of " + item.itemName + " : " + randomAmount);
            for (int i = 0; i < randomAmount; i++)
            {
                Instantiate(item,
                    new Vector3(transform.position.x + Random.Range(-0.4f, 0.4f), transform.position.y + 2 + Random.Range(-0.4f, 0.4f),
                        transform.position.z + Random.Range(-0.4f, 0.4f)), Quaternion.identity);
            }
        }

        Debug.Log("Tile destroyed");
        Destroy(gameObject);
    }
}
