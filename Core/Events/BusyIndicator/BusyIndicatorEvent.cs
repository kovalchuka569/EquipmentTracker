namespace Core.Events;

public class BusyIndicatorIsBusyEvent : PubSubEvent<bool>{}
public class BusyIndicatorMessageEvent : PubSubEvent<string>{}
public class BusyIndicatorVisibilityIndicatorEvent : PubSubEvent<string>{}
public class BusyIndicatorEvent : PubSubEvent<bool>{}
    