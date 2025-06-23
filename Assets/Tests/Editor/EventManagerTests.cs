using NUnit.Framework;
using Caddress.Common;
using UnityEngine;
using UnityEngine.TestTools;

public class EventManagerTests {
    private bool _callbackTriggered;
    private object _receivedData;

    [SetUp]
    public void SetUp() {
        // 清理所有事件
        typeof(EventManager)
            .GetProperty("dictFunctionEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .SetValue(null, new System.Collections.Generic.Dictionary<string, System.Action<object>>());

        _callbackTriggered = false;
        _receivedData = null;
    }

    [Test]
    public void StartListening_AddsCallback_AndTriggerEvent_InvokesIt() {
        string testEvent = "TestEvent";

        EventManager.StartListening(testEvent, OnTestEvent);

        EventManager.TriggerEvent(testEvent, "Hello");

        Assert.IsTrue(_callbackTriggered, "Event callback was not triggered.");
        Assert.AreEqual("Hello", _receivedData);
    }

    [Test]
    public void StopListening_RemovesCallback_CallbackNotInvoked() {
        string testEvent = "TestEvent";

        EventManager.StartListening(testEvent, OnTestEvent);
        EventManager.StopListening(testEvent, OnTestEvent);

        EventManager.TriggerEvent(testEvent, "Hello");

        Assert.IsFalse(_callbackTriggered, "Event callback was triggered after removal.");
    }

    [Test]
    public void TriggerEvent_WithNoListener_DoesNotThrow() {
        Assert.DoesNotThrow(() => EventManager.TriggerEvent("NonExistentEvent"));
    }

    [Test]
    public void StartListening_WithNullOrEmptyParams_LogsError() {
        LogAssert.Expect(LogType.Error, "[EventManager] : [StartListening] : check your input params;");
        EventManager.StartListening(null, null);
    }

    [Test]
    public void StopListening_WithNullOrEmptyParams_LogsError() {
        LogAssert.Expect(LogType.Error, "[EventManager] : [StopListening] : check your input params;");
        EventManager.StopListening(null, null);
    }

    [Test]
    public void TriggerEvent_WithEmptyEventName_LogsError() {
        LogAssert.Expect(LogType.Error, "[EventManager] : [TriggerEvent] : check your input params;");
        EventManager.TriggerEvent("");
    }

    private void OnTestEvent(object data) {
        _callbackTriggered = true;
        _receivedData = data;
    }
}
