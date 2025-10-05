using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI : MonoBehaviour
{
    [field: SerializeField] public TMP_Text moneyAmount { get; private set; }
    [field: SerializeField] public TMP_Text rockAmount { get; private set; }
    [field: SerializeField] public TMP_Text coalAmount { get; private set; }
    [field: SerializeField] public TMP_Text weight { get; private set; }
    [field: SerializeField] public GameObject textPanel { get; private set; }
    [field: SerializeField] public TMP_Text barkText { get; private set; }
    [field: SerializeField] public AudioSource uiAudioSource { get; private set; }
    [field: SerializeField] public float barkCharDelay { get; private set; } = 0.1f;
    [field: SerializeField] public RectTransform freezeBar { get; private set; }
    [field: SerializeField] public RectTransform fireplaceStatusTextRect { get; private set; }
    [field: SerializeField] public TMP_Text fireplaceStatusText { get; private set; }
    [field: SerializeField] public TMP_Text timer { get; private set; }
    
    [field: SerializeField] public GameObject endGamePanel { get; private set; }
    [field: SerializeField] public TMP_Text endGameMoney { get; private set; }
    [field: SerializeField] public TMP_Text endGameTimer { get; private set; }
    [field: SerializeField] public TMP_Text endGameCoal { get; private set; }

    [SerializeField] private Vector3   fireplaceWorldOffset = new Vector3(0, 2f, 0f);
    private                  Transform fireplaceTransform;
    private                  Camera    mainCamera;
    
    private                  Coroutine barkCoroutine;
    private                  string    currentBarkText;
    private                  bool      isTextFullyShown;
    
    public static bool InputLocked { get; private set; } = false;
    public static bool EndGameLock { get; private set; } = false;

    private void Subscribe()
    {
        EventBus.Subscribe<EventBusItemPickedUp>(ItemPickUpUpdateUi);
        EventBus.Subscribe<EventBusBarkTriggered>(TriggerBark);
        EventBus.Subscribe<EventBusFreezeUpdate>(FreezeBarUpdate);
        EventBus.Subscribe<EventBusFireplaceBurn>(UpdateFireplaceStatus);
        EventBus.Subscribe<EventBusItemSold>(OnItemSoldUi);
        EventBus.Subscribe<EventBusEndGame>(UiEndGame);
    }

    private void UiEndGame(EventBusEndGame obj)
    {
        EndGameLock = true;
        InputLocked = true;
        endGamePanel.gameObject.SetActive(true);
        endGameMoney.text = "Money: " + Core.instance.MainCharacter.money.ToString();
        endGameTimer.text = "You survived for " + timer.text + " seconds";
        endGameCoal.text = "Coal collected " + coalTotal;
    }

    private void OnItemSoldUi(EventBusItemSold obj)
    {
        uiAudioSource.clip = Core.instance.globalSettings.onItemSold;
        uiAudioSource.Play();
        UpdateUi();
    }

    private void UpdateFireplaceStatus(EventBusFireplaceBurn obj)
    {
        UpdateUi();
        fireplaceStatusText.text = "Coals: " + obj.fireplace.Coals.Count;
    }

    private void FreezeBarUpdate(EventBusFreezeUpdate obj)
    {
        freezeBar.sizeDelta = new Vector2(200 / obj.maxFreeze * obj.currentFreeze, freezeBar.sizeDelta.y);
    }

    private void TriggerBark(EventBusBarkTriggered obj)
    {
        if (barkCoroutine != null)
            StopCoroutine(barkCoroutine);

        currentBarkText = obj.barkData.prefix + obj.barkData.barkText;
        barkCoroutine = StartCoroutine(PlayBarkCoroutine(currentBarkText));
    }

    private IEnumerator PlayBarkCoroutine(string text)
    {
        textPanel.SetActive(true);
        barkText.text = "";
        isTextFullyShown = false;

        foreach (char c in text)
        {
            barkText.text += c;
            if (c != ' ' && uiAudioSource != null && Core.instance.globalSettings.barkCharSounds.Count > 0)
            {
                var clip = Core.instance.globalSettings.barkCharSounds[
                    Random.Range(0, Core.instance.globalSettings.barkCharSounds.Count)];
                uiAudioSource.PlayOneShot(clip);
            }

            yield return new WaitForSeconds(barkCharDelay);
        }

        isTextFullyShown = true;
        barkCoroutine = null;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        InputLocked = textPanel.activeSelf; 

        if (!mouse.leftButton.wasPressedThisFrame)
            return;

        if (textPanel.activeSelf)
        {
            if (barkCoroutine != null)
            {
                StopCoroutine(barkCoroutine);
                barkCoroutine = null;
                barkText.text = currentBarkText;
                isTextFullyShown = true;
            }
            else if (isTextFullyShown)
            {
                textPanel.SetActive(false);
            }
        }
    }

    private void Unsubscribe()
    {
        EventBus.Unsubscribe<EventBusItemPickedUp>(ItemPickUpUpdateUi);
        EventBus.Unsubscribe<EventBusBarkTriggered>(TriggerBark);
        EventBus.Unsubscribe<EventBusFreezeUpdate>(FreezeBarUpdate);
        EventBus.Unsubscribe<EventBusFireplaceBurn>(UpdateFireplaceStatus);
        EventBus.Unsubscribe<EventBusItemSold>(OnItemSoldUi);
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private int coalTotal = 0;
    private void ItemPickUpUpdateUi(EventBusItemPickedUp obj)
    {
        if (obj.item.itemType == Item.ItemType.Coal)
            coalTotal++;
        UpdateUi();
    }

    private void Start()
    {
        UpdateUi();
        Subscribe();
        mainCamera = Core.instance.cameraController.cameraMain;
        fireplaceTransform = Core.instance.fireplace.transform;
    }

    private void UpdateUi()
    {
        int totalWeight = 0;
        int totalCoals = 0;
        int totalRocks = 0;
        Character character = Core.instance.MainCharacter;
        foreach (var item in character.Inventory)
        {
            totalWeight += item.Weight;
            if (item.itemType == Item.ItemType.Coal)
                totalCoals++;
            if (item.itemType == Item.ItemType.Rock)
                totalRocks++;
        }
        
        weight.text = totalWeight + " / " + Core.instance.MainCharacter.strength;
        moneyAmount.text = Core.instance.MainCharacter.money + "";
        rockAmount.text = totalRocks + "";
        coalAmount.text = totalCoals + "";
    }
    
    private void LateUpdate()
    {
        UpdateFireplaceStatusPosition();
        if (EndGameLock)
            return;
        timer.text = Mathf.RoundToInt(Time.time) + "";
    }

    private void UpdateFireplaceStatusPosition()
    {
        if (fireplaceStatusText == null || fireplaceTransform == null || mainCamera == null)
            return;

        Vector3 worldPos = fireplaceTransform.position + fireplaceWorldOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0)
        {
            fireplaceStatusText.gameObject.SetActive(false);
            return;
        }

        fireplaceStatusText.gameObject.SetActive(true);
        fireplaceStatusTextRect.position = screenPos;
    }
}