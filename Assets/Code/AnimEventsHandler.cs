using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventsHandler : MonoBehaviour
{
    public void DigHitAnimEvent()
    {
        EventBus.Publish(new EventBusCharacterDigHit());
    }

    public void OnFootstepAnimEvent()
    {
        EventBus.Publish(new EventBusFootstep());
    }
}
