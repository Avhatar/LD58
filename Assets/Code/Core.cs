using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class Core : MonoBehaviour
{
    public static Core instance { get; private set; }
    
    public List<BarkDataSo> coalLecturesToShow;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        Init();
    }
    
    [field: SerializeField]
    public GlobalSettingsSo globalSettings;
    [field: SerializeField]
    public InputController inputController { get; private set; }
    [field: SerializeField]
    public CameraController cameraController { get; private set; }
    [field: SerializeField]
    public CharactersController charactersController { get; private set; }
    [field: SerializeField] 
    public Character MainCharacter { get; private set; }
    [field: SerializeField] 
    public AudioSource globalAudioSource { get; private set; }
    [field: SerializeField] 
    public AstarPath aStar { get; private set; }
    [field: SerializeField] 
    public Fireplace fireplace { get; private set; }
    
    private void Init()
    {
        inputController = FindAnyObjectByType<InputController>(FindObjectsInactive.Include);
        cameraController = FindAnyObjectByType<CameraController>(FindObjectsInactive.Include);
        charactersController = FindAnyObjectByType<CharactersController>(FindObjectsInactive.Include);
        aStar = FindAnyObjectByType<AstarPath>(FindObjectsInactive.Include);
    }
    
    private void Start()
    {
        Subscribe();
        
    }

    private void Subscribe()
    {
        EventBus.Subscribe<EventBusTileDestroyed>(UpdateNavMesh);
        EventBus.Subscribe<EventBusItemPickedUp>(CoalLecture);
    }

    private void CoalLecture(EventBusItemPickedUp obj)
    {
        if (obj.item.itemType != Item.ItemType.Coal)
            return;
        int randomBark = Random.Range(0, coalLecturesToShow.Count);
        EventBus.Publish(new EventBusBarkTriggered{ barkData = coalLecturesToShow[randomBark]});
        coalLecturesToShow.RemoveAt(randomBark);
    }

    private void Unsubscribe()
    {
        EventBus.Unsubscribe<EventBusTileDestroyed>(UpdateNavMesh);
    }
    
    private void UpdateNavMesh(EventBusTileDestroyed obj)
    {
        Bounds bounds = obj.tile.Collider.bounds;
        GraphUpdateObject graphUpdateObject = new GraphUpdateObject(bounds);
        graphUpdateObject.resetPenaltyOnPhysics = true;
        graphUpdateObject.setWalkability = true;
        aStar.UpdateGraphs(bounds);
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

}
