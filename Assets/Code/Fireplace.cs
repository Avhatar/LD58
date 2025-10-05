using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireplace : MonoBehaviour
{
    [SerializeField]
    private List<Item> coals = new List<Item>();
    public List<Item> Coals { get => coals; }

    [SerializeField]
    private float distanceToWarmCharacterModifier = 4;

    [field: SerializeField]
    public Collider collider { get; private set; }
    
    [field: SerializeField]
    public Highlighter highlighter { get; private set; }

    [SerializeField]
    private BarkDataSo barkOnTooFar;
    
    [SerializeField]
    private ParticleSystem particleSystem;

    [SerializeField]
    private GameObject warmSphere;
    
    private Character character;
    private void Start()
    {
        character = Core.instance.MainCharacter;
        StartCoroutine(Burn());
        Init();
    }

    private void Init()
    {
        highlighter.Init();
        SetWarmSphereSize();
        EventBus.Publish(new EventBusFireplaceBurn{fireplace = this});
    }

    public void AddCoal(Item coal)
    {
        coals.Add(coal);
        coal.gameObject.SetActive(true);
        coal.transform.SetParent(transform);
        coal.transform.localPosition = Vector3.zero;
        coal.Rb.isKinematic = true;
        coal.Collider.enabled = false;
        coal.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(0, 0.4f), Random.Range(-0.4f, 0.4f));
        SetWarmSphereSize();
        EventBus.Publish(new EventBusFireplaceBurn{fireplace = this});
    }

    private IEnumerator Burn()
    {
        while (true)
        {
            yield return new WaitForSeconds(Core.instance.globalSettings.warmInterval);

            if (coals.Count > 0)
            {
                coals[coals.Count - 1].hp -= 1;
                if (coals[coals.Count - 1].hp <= 0)
                {
                    coals[coals.Count - 1].gameObject.SetActive(false);
                    coals.RemoveAt(coals.Count - 1);
                    SetWarmSphereSize();
                }
                if (Vector3.Distance(transform.position, character.transform.position) <= GetWarmDistance())
                    character.Warm(coals.Count);
                EventBus.Publish(new EventBusFireplaceBurn{fireplace = this});
            }
        }
    }

    private void SetWarmSphereSize()
    {
        float radius = GetWarmDistance();
        float diameter = radius * 2f;
        warmSphere.transform.localScale = new Vector3(diameter, diameter, diameter);
    }
    
    public float GetWarmDistance()
    {
        return coals.Count * distanceToWarmCharacterModifier;
    }
    
    private void Update()
    {
        particleSystem.maxParticles = coals.Count * 4;
    }
}
