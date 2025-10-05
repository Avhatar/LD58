using UnityEngine;

public class TriggerPlayBark : MonoBehaviour
{
    [field: SerializeField]
    public Collider triggerCollider { get; private set; }
    [field: SerializeField]
    public BarkDataSo bark { get; private set; }

    private void OnValidate()
    {
        if (!name.Contains("BarkTrigger"))
            name = "BarkTrigger";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Character"))
            return;
        EventBus.Publish(new EventBusBarkTriggered { barkData = bark });
        gameObject.SetActive(false);
    } 
}
