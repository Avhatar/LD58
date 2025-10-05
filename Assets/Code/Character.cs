using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

public class Character : MonoBehaviour
{
    private enum State
    {
        idle,
        walk,
        goingDig,
        dig,
        deliver
    }

    [field: SerializeField]
    public float freeze { get; private set; } = 0;
    [field: SerializeField]
    public float maxFreeze { get; private set; } = 10;
    [field: SerializeField]
    public FollowerEntity follower { get; private set; }
    [field: SerializeField]
    public Highlighter highlighter { get; private set; }
    [field: SerializeField]
    public Animator animator { get; private set; }
    [field: SerializeField]
    public  float moveSpeed { get; private set; } = 3;
    [SerializeField]
    private State currentState = State.idle;
    [SerializeField]
    private float stopDistance = 0.5f;
    [SerializeField]
    private float stopDistanceDig = 2f;
    
    [SerializeField]
    private List<Item> inventory = new List<Item>();
    public List<Item> Inventory { get { return inventory; } }
    
    [field: SerializeField]
    public float strength { get; private set; } = 10;
    [field: SerializeField]
    public float money { get; set; } = 10;
    
    private Tile      currentTargetTile;
    
    private Coroutine currentActionCoroutine;

    private void Subscribe()
    {
        EventBus.Subscribe<EventBusCharacterDigHit>(DigHitAnimEvent);
        EventBus.Subscribe<EventBusTileDestroyed>(OnTileDestroyed);
        EventBus.Subscribe<EventBusBarkTriggered>(OnBarkTriggered);
        EventBus.Subscribe<EventBusFootstep>(OnFootstep);
    }

    private void OnFootstep(EventBusFootstep obj)
    {
        AudioSource globalSource = Core.instance.globalAudioSource;
        globalSource.clip = Core.instance.globalSettings.footsteps[Random.Range(0, Core.instance.globalSettings.footsteps.Count)];
        globalSource.pitch = 1;
        globalSource.volume = 0.7f;
        globalSource.spatialBlend = 0.8f;
        globalSource.Play();
    }

    private void OnBarkTriggered(EventBusBarkTriggered obj)
    {
        Stop();
    }

    private void Stop()
    {
        CheckCurrentActionCoroutine();
        follower.destination = transform.position;
        ResetTriggers();
        animator.SetTrigger("Idle");
    }
    
    private void Unsubscribe()
    {
        EventBus.Unsubscribe<EventBusCharacterDigHit>(DigHitAnimEvent);
        EventBus.Unsubscribe<EventBusTileDestroyed>(OnTileDestroyed);
        EventBus.Unsubscribe<EventBusBarkTriggered>(OnBarkTriggered);
        EventBus.Unsubscribe<EventBusFootstep>(OnFootstep);
    }
    
    void Start()
    {
        Init();
    }

    private void Init()
    {
        follower = GetComponent<FollowerEntity>();
        highlighter = GetComponent<Highlighter>();
        follower.maxSpeed = moveSpeed;
        highlighter.Init();
        
        ResetTriggers();
        currentState = State.idle;
        StartCoroutine(Freeze());
        Subscribe();
    }

    public void Warm(float amount)
    {
        freeze -= amount;
        EventBus.Publish(new EventBusFreezeUpdate{ currentFreeze = freeze, maxFreeze = maxFreeze});
        if (freeze < 0)
            freeze = 0;
    }
    
    private IEnumerator Freeze()
    {
        while (true)
        {
            yield return new WaitForSeconds(Core.instance.globalSettings.freezeInterval);

            if (Vector3.Distance(Core.instance.fireplace.transform.position, transform.position) > Core.instance.fireplace.GetWarmDistance())
            {
                freeze += 1;
                EventBus.Publish(new EventBusFreezeUpdate { currentFreeze = freeze, maxFreeze = maxFreeze });
                if (freeze > maxFreeze)
                {
                    freeze = maxFreeze;
                    EventBus.Publish(new EventBusEndGame());
                    Debug.Log("You are froze to death");
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void CheckCurrentActionCoroutine()
    {
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }
    }

    public void GoAddCoal(Fireplace fireplace)
    {
        CheckCurrentActionCoroutine();
        SetDestination(fireplace.transform.position);
        ResetTriggers();
        animator.SetTrigger("Walk");
        currentState = State.walk;
        currentActionCoroutine = StartCoroutine(CheckFireplaceDestinationReached(fireplace));
    }
    
    IEnumerator CheckFireplaceDestinationReached(Fireplace fireplace)
    {
        while (Vector3.Distance(transform.position, fireplace.transform.position) > stopDistance)
        {
            yield return new WaitForSeconds(0.1f);
        }
        follower.destination = transform.position;
        Item coal = GetCoalFromInventory();
        if (coal != null)
        {
            inventory.Remove(coal);
            fireplace.AddCoal(coal);
        }
        else
        {
            EventBus.Publish(new EventBusBarkTriggered{ barkData = Core.instance.globalSettings.barkOnNoCoal});
        }
        CheckCurrentActionCoroutine();
        ResetTriggers();
        animator.SetTrigger("Idle");
        currentState = State.idle;
    }

    private Item GetCoalFromInventory()
    {
        foreach (var item in inventory)
        {
            if (item != null && item.itemType == Item.ItemType.Coal)
                return item;
        }
        return null;
    }

    public void GoSell(Buyer buyer)
    {
        CheckCurrentActionCoroutine();
        SetDestination(buyer.transform.position);
        ResetTriggers();
        animator.SetTrigger("Walk");
        currentState = State.walk;
        currentActionCoroutine = StartCoroutine(CheckSellDestinationReached(buyer));
    }
    
    IEnumerator CheckSellDestinationReached(Buyer buyer)
    {
        while (Vector3.Distance(transform.position, buyer.transform.position) > stopDistance)
        {
            yield return new WaitForSeconds(0.1f);
        }
        follower.destination = transform.position;
        ResetTriggers();
        animator.SetTrigger("Idle");
        buyer.SellItemToBuyer(this);
    }
    
    public void GoDig(Tile targetTile)
    {
        CheckCurrentActionCoroutine();
        SetDestination(targetTile.transform.position);
        ResetTriggers();
        animator.SetTrigger("Walk");
        currentState = State.goingDig;
        currentActionCoroutine = StartCoroutine(CheckDigDestinationReached(targetTile));
    }

    public void GoToPoint(Vector3 point)
    {
        CheckCurrentActionCoroutine();
        SetDestination(point);
        ResetTriggers();
        animator.SetTrigger("Walk");
        currentState = State.walk;
        currentActionCoroutine = StartCoroutine(CheckPointDestinationReached(point));
    }

    public void GoToPickup(Item item)
    {
        CheckCurrentActionCoroutine();
        SetDestination(item.transform.position);
        ResetTriggers();
        animator.SetTrigger("Walk");
        currentState = State.walk;
        currentActionCoroutine = StartCoroutine(CheckPickUpDestinationReached(item));
    }
    
    IEnumerator CheckPickUpDestinationReached(Item item)
    {
        while (Vector3.Distance(transform.position, item.transform.position) > stopDistance)
        {
            yield return new WaitForSeconds(0.1f);
        }
        follower.destination = transform.position;
        PickUp(item);
    }

    private void PickUp(Item item)
    {
        CheckCurrentActionCoroutine();
        ResetTriggers();
        animator.SetTrigger("Idle");
        currentState = State.idle;
        inventory.Add(item);
        item.OnPickUp();
    }

    IEnumerator CheckPointDestinationReached(Vector3 point)
    {
        while (Vector3.Distance(transform.position, point) > stopDistance)
            yield return new WaitForSeconds(0.05f);
        ResetTriggers();
        animator.SetTrigger("Idle");
        SetDestination(transform.position);
        currentState = State.idle;
        StopCoroutine(currentActionCoroutine);
    }
    
    IEnumerator CheckDigDestinationReached(Tile targetTile)
    {
        while (Vector3.Distance(transform.position, targetTile.transform.position) > stopDistanceDig)
        {
            yield return new WaitForSeconds(0.1f);
        }
        follower.destination = transform.position;
        StartDig(targetTile);
    }

    private Coroutine rotateCoroutine;
    private void StartDig(Tile targetTile)
    {
        CheckCurrentActionCoroutine();
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }
        rotateCoroutine = StartCoroutine(RotateCoroutine(targetTile.transform.position));
        currentTargetTile = targetTile;
        CheckCurrentActionCoroutine();
        currentState = State.dig;
        ResetTriggers();
        animator.SetTrigger("Dig");
    }

    private IEnumerator RotateCoroutine(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
            yield break;

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float angle = Quaternion.Angle(startRotation, targetRotation);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 150f / Mathf.Max(angle, 0.001f);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.rotation = targetRotation;
    }
    
    private void OnTileDestroyed(EventBusTileDestroyed obj)
    {
        if (currentTargetTile != null && currentTargetTile == obj.tile)
            StopDig();
    }
    
    private void StopDig()
    {
        currentTargetTile = null;
        currentState = State.idle;
        ResetTriggers();
        animator.SetTrigger("Idle");
    }

    private void ResetTriggers()
    {
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Dig");
    }
    
    private void SetDestination(Vector3 destination)
    {
        follower.destination = destination;
    }

    public void DigHitAnimEvent(EventBusCharacterDigHit obj)
    {
        if (currentTargetTile != null)
            currentTargetTile.Damage(GetDamage());
    }

    private int GetDamage()
    {
        return 1;
    }
}
