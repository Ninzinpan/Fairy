/// <summary>
/// イベントキューで渡すデータの基本構造（今回は未使用）。
/// Basic structure for data passed in the event queue (not used in this simple version).
/// </summary>
public class EventData
{
    public string EventId;
    // public object Payload; // 将来的にイベント固有のデータを渡す場合

    public EventData(string eventId)
    {
        EventId = eventId;
    }
}
