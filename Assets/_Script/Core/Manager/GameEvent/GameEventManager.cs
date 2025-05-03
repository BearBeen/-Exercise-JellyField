using System;

public static class GameEventManager<EventBinder, EventKeyType, EventDataType>
        where EventBinder: IGameEventBinder<EventKeyType, EventDataType>
        where EventKeyType : unmanaged, Enum
        where EventDataType : struct
{
    static private Action<EventDataType>[] _callBacks = new Action<EventDataType>[Enum.GetValues(typeof(EventKeyType)).Length];

    public static void AddEventListener(EventKeyType eventKey, Action<EventDataType> callBack)
    {
        _callBacks[eventKey.ToInt()] += callBack;
    }

    public static void RemoveEventListener(EventKeyType eventKey, Action<EventDataType> callBack)
    {
        _callBacks[eventKey.ToInt()] -= callBack;
    }

    public static void Invoke(EventKeyType eventKey, EventDataType eventData)
    {
        _callBacks[eventKey.ToInt()]?.Invoke(eventData);
    }
}

/// <summary>
/// Look ugly but good for concrete-type binding and can lead to fewer mistakes.
/// For an event, an enum will bind with a struct; that struct may get big.
/// You can use an explicit struct layout to create a C union-like struct, but be careful and don't use that method with an object instance.
/// </summary>
/// <typeparam name="EventKeyType"></typeparam>
/// <typeparam name="EventDataType"></typeparam>
public interface IGameEventBinder<EventKeyType, EventDataType>
        where EventKeyType : unmanaged, Enum
        where EventDataType : struct
{ }

public static class GameEventManager
{
    public struct EmptyEventData { }
}