using System;
using System.Collections.Generic;

public static class EventBus
{
    private static Dictionary<Type, Delegate> _eventTable = new();

    public static void Subscribe<T>(Action<T> callback)
    {
        if (_eventTable.TryGetValue(typeof(T), out var del))
            _eventTable[typeof(T)] = Delegate.Combine(del, callback);
        else
            _eventTable[typeof(T)] = callback;
    }

    public static void Unsubscribe<T>(Action<T> callback)
    {
        if (_eventTable.TryGetValue(typeof(T), out var del))
        {
            del = Delegate.Remove(del, callback);
            if (del == null) _eventTable.Remove(typeof(T));
            else _eventTable[typeof(T)] = del;
        }
    }

    public static void Publish<T>(T evt)
    {
        if (_eventTable.TryGetValue(typeof(T), out var del))
            (del as Action<T>)?.Invoke(evt);
    }
}

public class EventBusCharacterDigHit
{
    
}

public class EventBusTileDestroyed
{
    public Tile tile;
}

public class EventBusItemPickedUp
{
    public Item item;
}

public class EventBusBarkTriggered
{
    public BarkDataSo barkData;
}

public class EventBusFreezeUpdate
{
    public float maxFreeze;
    public float currentFreeze;
}

public class EventBusFireplaceBurn
{
    public Fireplace fireplace;
}

public class EventBusItemSold
{
    public Item item;
}

public class EventBusEndGame
{
    
}

public class EventBusFootstep
{
    
}