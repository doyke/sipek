using System;
using System.Collections.Generic;
using System.Text;
using Telephony;

using NUnit.Framework;

namespace UnitTest
{
  public class MockSipProxy : CTelephonyInterface
  {
    public int makeCall(string dialedNo, int accountId) { return 1; }

    public bool endCall() { return true; }

    public bool alerted() { return true; }

    public bool acceptCall() { return true; }

    public bool holdCall() { return true; }

    public bool retrieveCall() { return true; }

    public bool xferCall(string number) { return true; }

    public bool xferCallSession(int session) { return true; }

    public bool threePtyCall(int session) { return true; }

    //bool serviceRequest(EServiceCodes code, int session);
    public bool serviceRequest(int code, string dest) { return true; }

    public bool dialDtmf(int mode, string digits) { return true; }
  }

  public class MockCommonProxy : CCommonProxyInterface
  {
    #region CCommonProxyInterface Members
    public int initialize()
    {
      return 1;
    }
    public int shutdown()
    {
      return 1;
    }

    public int registerAccounts()
    {
      return 1;
    }

    public int playTone(ETones toneId)
    {
      return 1;
    }

    public int stopTone()
    {
      return 1;
    }

    public int addBuddy(string ident)
    {
      return 1;
    }

    public int delBuddy(int buddyId)
    {
      return 1;
    }

    public int sendMessage(string dest, string message)
    {
      return 1;
    }

    #endregion
  }



  [TestFixture]
  public class TestTelephony
  {

    [Test]
    public void testStateMachineCreate()
    {
      CStateMachine sm = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());

      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero ,sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.getStateId());

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.getStateId());
      Assert.AreEqual("INCOMING", sm.getStateName());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.getStateId());
      Assert.AreEqual("ALERTING", sm.getStateName());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.getStateId());
      Assert.AreEqual("CONNECTING", sm.getStateName());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.getStateId());
      Assert.AreEqual("RELEASED", sm.getStateName());

      sm.destroy();
  
    }

    [Test]
    public void testStateMachineCreateSequence()
    {
      CStateMachine sm = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());

      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.getStateId());

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.getStateId());
      Assert.AreEqual("INCOMING", sm.getStateName());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.getStateId());
      Assert.AreEqual("ALERTING", sm.getStateName());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.getStateId());
      Assert.AreEqual("CONNECTING", sm.getStateName());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.getStateId());
      Assert.AreEqual("RELEASED", sm.getStateName());

      sm.destroy();

      // Second
      sm = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());
      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.getStateId());

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.getStateId());
      Assert.AreEqual("INCOMING", sm.getStateName());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.getStateId());
      Assert.AreEqual("ALERTING", sm.getStateName());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.getStateId());
      Assert.AreEqual("CONNECTING", sm.getStateName());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.getStateId());
      Assert.AreEqual("RELEASED", sm.getStateName());
      sm.destroy();

      // third

      sm = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());
      Assert.AreEqual(-1, sm.Session);
      Assert.AreEqual(TimeSpan.Zero, sm.Duration);
      Assert.AreEqual(EStateId.IDLE, sm.getStateId());

      // changing state
      sm.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm.getStateId());
      Assert.AreEqual("INCOMING", sm.getStateName());
      sm.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm.getStateId());
      Assert.AreEqual("ALERTING", sm.getStateName());
      sm.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm.getStateId());
      Assert.AreEqual("CONNECTING", sm.getStateName());
      sm.changeState(EStateId.RELEASED);
      Assert.AreEqual(EStateId.RELEASED, sm.getStateId());
      Assert.AreEqual("RELEASED", sm.getStateName());
      sm.destroy();
    }

    [Test]
    public void testMultipleStateMachines()
    {
      CStateMachine sm1 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());
      CStateMachine sm2 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());
      CStateMachine sm3 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());

      Assert.AreEqual(-1, sm1.Session);
      Assert.AreEqual(TimeSpan.Zero, sm1.Duration);
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());

      Assert.AreEqual(-1, sm2.Session);
      Assert.AreEqual(TimeSpan.Zero, sm2.Duration);
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());

      Assert.AreEqual(-1, sm3.Session);
      Assert.AreEqual(TimeSpan.Zero, sm3.Duration);
      Assert.AreEqual(EStateId.IDLE, sm3.getStateId());

      // changing state
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      sm2.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm2.getStateId());
      sm3.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm3.getStateId());

      sm1.destroy();
      sm2.destroy();
      sm3.destroy();

      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm3.getStateId());
    }

    [Test]
    public void testMultipleStateMachinesSequence()
    {
      CStateMachine sm1 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());

      Assert.AreEqual(-1, sm1.Session);
      Assert.AreEqual(TimeSpan.Zero, sm1.Duration);
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());

      // changing state
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      sm1.destroy();

      CStateMachine sm2 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());
      Assert.AreEqual(-1, sm2.Session);
      Assert.AreEqual(TimeSpan.Zero, sm2.Duration);
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
      
      sm2.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm2.getStateId());

      sm2.destroy();

      CStateMachine sm3 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());
      Assert.AreEqual(-1, sm3.Session);
      Assert.AreEqual(TimeSpan.Zero, sm3.Duration);
      Assert.AreEqual(EStateId.IDLE, sm3.getStateId());

      sm3.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm3.getStateId());

      sm3.destroy();

      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm3.getStateId());


    }

    [Test]
    public void testIncomingCall()
    {
      CStateMachine sm1 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      sm1.changeState(EStateId.INCOMING);
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);

      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.destroy();
    }

    [Test]
    public void testOutgoingCall()
    {
      CStateMachine sm1 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      sm1.changeState(EStateId.CONNECTING);
      Assert.AreEqual(EStateId.CONNECTING, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ALERTING);
      Assert.AreEqual(EStateId.ALERTING, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.changeState(EStateId.ACTIVE);
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual("ACTIVE", sm1.getStateName());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual(true, sm1.Counting);
      Assert.AreNotSame(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.destroy();
    }

    [Test]
    public void testStateMachineEventHandlingOutgoing()
    {
      CStateMachine sm1 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());
      sm1.getState().makeCall("1234", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm1.getStateId());
      Assert.AreEqual(false, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNo);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.getState().onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm1.getStateId());
      Assert.AreEqual(false, sm1.Counting);
      Assert.AreEqual(sm1.RuntimeDuration, TimeSpan.Zero);

      sm1.getState().onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.getState().onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());
    }

    [Test]
    public void testStateMachineEventHandlingIncoming()
    {
      CStateMachine sm1 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());
      
      sm1.getState().incomingCall("1234");
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNo);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.getState().acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.getState().onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);
      //Assert.AreNotEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());
    }


    [Test]
    public void testCallFeaturesCallHold()
    {
      CStateMachine sm1 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());

      sm1.getState().incomingCall("1234");
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNo);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.getState().acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);

      sm1.getState().holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId()); // still ACTIVE (waiting confirmation)
      sm1.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.getStateId());
      // check twice hold
      sm1.getState().holdCall();
      Assert.AreEqual(EStateId.HOLDING, sm1.getStateId());

      sm1.getState().retrieveCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());

      sm1.getState().holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId()); // still ACTIVE (waiting confirmation)
      sm1.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.getStateId());

      sm1.destroy();
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());

    }

    [Test]
    public void testCallFeaturesCallHoldMultiple()
    {
      CStateMachine sm1 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());

      sm1.getState().incomingCall("1234");
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNo);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      sm1.getState().acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);

      sm1.getState().holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId()); // still ACTIVE (waiting confirmation)
      sm1.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm1.getStateId());

      // next call
      CStateMachine sm2 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());

      sm2.getState().makeCall("4444", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm2.getStateId());
      Assert.AreEqual(false, sm2.Incoming);
      Assert.AreEqual("4444", sm2.CallingNo);

      sm2.getState().onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm2.getStateId());
      Assert.AreEqual(false, sm2.Counting);

      sm2.getState().onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm2.getStateId());
      Assert.AreEqual(true, sm2.Counting);

      sm2.getState().holdCall();
      Assert.AreEqual(EStateId.ACTIVE, sm2.getStateId()); // still ACTIVE (waiting confirmation)
      sm2.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm2.getStateId());

      // release first
      sm1.getState().onReleased();
      Assert.AreEqual(EStateId.RELEASED, sm1.getStateId());
      sm2.getState().onHoldConfirm();
      Assert.AreEqual(EStateId.HOLDING, sm2.getStateId());

      sm2.getState().endCall();
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
      sm2.getState().onReleased();
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
    }

    [Test]
    public void testCallFeaturesCallWaiting()
    {
      // out call
      CStateMachine sm2 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());

      sm2.getState().makeCall("4444", 0);
      Assert.AreEqual(EStateId.CONNECTING, sm2.getStateId());
      Assert.AreEqual(false, sm2.Incoming);
      Assert.AreEqual("4444", sm2.CallingNo);

      sm2.getState().onAlerting();
      Assert.AreEqual(EStateId.ALERTING, sm2.getStateId());
      Assert.AreEqual(false, sm2.Counting);

      sm2.getState().onConnect();
      Assert.AreEqual(EStateId.ACTIVE, sm2.getStateId());
      Assert.AreEqual(true, sm2.Counting);

      // inc call
      CStateMachine sm1 = new CStateMachine(null, new MockSipProxy(), new MockCommonProxy());

      sm1.getState().incomingCall("1234");
      Assert.AreEqual(EStateId.INCOMING, sm1.getStateId());
      Assert.AreEqual(true, sm1.Incoming);
      Assert.AreEqual("1234", sm1.CallingNo);
      Assert.AreEqual(sm1.RuntimeDuration.ToString(), TimeSpan.Zero.ToString());

      // check what happens here? 
      sm1.getState().acceptCall();
      Assert.AreEqual(EStateId.ACTIVE, sm1.getStateId());
      Assert.AreEqual(true, sm1.Counting);
      // this should be done automatically by call manager
      // Here we do not test call manager
      //Assert.AreEqual(EStateId.HOLDING, sm2.getStateId()); 

      sm1.getState().endCall();
      sm2.getState().endCall();
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());
      sm1.getState().onReleased();
      sm2.getState().onReleased();
      Assert.AreEqual(EStateId.IDLE, sm1.getStateId());
      Assert.AreEqual(EStateId.IDLE, sm2.getStateId());

    }

    [Test]
    public void testCallFeaturesCallTransfer()
    {
      Assert.Ignore();
    }

    [Test]
    public void testCallFeaturesConference()
    {
      Assert.Ignore();
    }

    [Test]
    public void testCallFeaturesAutoAnswer()
    {
      Assert.Ignore();
    }

    [Test]
    public void testCallFeaturesCallForwarding()
    {
      Assert.Ignore();
    }


  }


}
