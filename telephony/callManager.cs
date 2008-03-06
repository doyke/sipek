/* 
 * Copyright (C) 2007 Sasa Coh <sasacoh@gmail.com>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 
 */

/*! \mainpage Sipek Phone Project
 *
 * \section intro_sec Introduction
 *
 * SIPek is a small open source project that is intended to share common VoIP software design concepts 
 * and practices. It'd also like to become a simple and easy-to-use SIP phone with many useful features.
 * 
 * SIPek's telephony engine is based on common library used in Sipek project. The telephony part is powered 
 * by great SIP stack engine PJSIP (http://www.pjsip.org). The connection between pjsip code (C) 
 * and .Net GUI (C#) is handled by simple wrapper which is also suitable for mobile devices. Sipek use C# Audio library from http://www.codeproject.com/KB/graphics/AudioLib.aspx. 
 * The SIPek's simple software design enables efficient development, easy upgrading and 
 * user menus customizations.
 * 
 * Visit SIPek's home page at http://sipekphone.googlepages.com/ 
 *
 * \section install_sec Installation
 *
 * \subsection step1 Step 1: 
 *  
 * etc...
 */


/*! \namespace CallControl
    \brief Module CallControl is a general Call Automaton engine.

    A more detailed namespace description.
*/

using System.Collections;
using System.Collections.Generic;
using System;
using Common;

namespace CallControl
{

  //////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// CCallManager
  /// Main telephony class
  /// </summary>
  public class CCallManager
  {
    #region Variables

    private static CCallManager _instance = null;

    private Dictionary<int, CStateMachine> _calls;  //!< Call table

    private AbstractFactory _factory = new NullFactory();

    PendingAction _pendingAction;
 
    #endregion


    #region Properties

    public AbstractFactory Factory
    {
      get { return _factory; }
      set { _factory = value; }
    }

    public IConfiguratorInterface Config
    {
      get { return Factory.getConfigurator(); }
    }

    public CStateMachine this[int index]
    {
      get
      {
        if (!_calls.ContainsKey(index)) return null;
        return _calls[index];
      }
    }

    public Dictionary<int, CStateMachine> CallList
    {
      get { return _calls; }
    }

    public int Count
    {
      get { return _calls.Count; }
    }

    public bool Is3Pty
    {
      get 
      {
        return (getNoCallsInState(EStateId.ACTIVE) == 2) ? true : false;
      }
    }

    ///////////////////////////////////////////////////////////////////////////
    private bool _initialized = false;
    public bool isInitialized
    {
      get { return _initialized; }
    }

    #endregion Properties


    #region Constructor

    /// <summary>
    /// CCallManager Singleton
    /// </summary>
    /// <returns></returns>
    public static CCallManager getInstance()
    { 
      if (_instance == null)
      {
        _instance = new CCallManager();
      }
      return _instance;
    }

    #endregion Constructor

    #region Events

    public delegate void DCallStateRefresh();  // define callback type 
    public event DCallStateRefresh CallStateRefresh;

    enum EPendingActions : int
    {
      EUserAnswer,
      ECreateSession,
      EUserHold
    };

    /// <summary>
    /// 
    /// </summary>
    class PendingAction
    {
      delegate void DPendingAnswer(int sessionId); // for onUserAnswer
      delegate void DPendingCreateSession(string number, int accountId); // for CreateOutboudCall

      private EPendingActions _actionType;
      private int _sessionId;
      private int _accountId;
      private string _number;


      public PendingAction(EPendingActions action, int sessionId)
      {
        _actionType = action;
        _sessionId = sessionId;
      }
      public PendingAction(EPendingActions action, string number, int accId)
      {
        _actionType = action;
        _sessionId = -1;
        _number = number;
        _accountId = accId;
      }

      public void Activate()
      {
        switch (_actionType)
        {
          case EPendingActions.EUserAnswer:
            CCallManager.getInstance().onUserAnswer(_sessionId);
            break;
          case EPendingActions.ECreateSession:
            CCallManager.getInstance().createOutboundCall(_number, _accountId);
        	  break;
          case EPendingActions.EUserHold:
            CCallManager.getInstance().onUserHoldRetrieve(_sessionId);
            break;
        }
      }

    }

    /////////////////////////////////////////////////////////////////////////
    // Callback handlers
    /// <summary>
    /// Inform GUI to be refreshed 
    /// </summary>
    public void updateGui()
    {
      if (null != CallStateRefresh) CallStateRefresh();
    }

    #endregion Events

    #region Public methods

    ///////////////////////////////////////////////////////////////////
    /// Common routines

    public int initialize()
    {
      int status = 0;
      ///
      if (!isInitialized)
      {
        // register to signaling proxy interface
        Factory.getCommonProxy().CallStateChanged += OnCallStateChanged;
        Factory.getCommonProxy().CallIncoming += OnIncomingCall;
        Factory.getCommonProxy().CallNotification += OnCallNotification;

        // Initialize call table
        _calls = new Dictionary<int, CStateMachine>(); 
        
        status = Factory.getCommonProxy().initialize();
        if (status != 0) return status;

        Factory.getCommonProxy().registerAccounts(false);
      }
      else
      {       
        // reregister 
        Factory.getCommonProxy().registerAccounts(true); 
      }
      _initialized = true;
      return status;
    }

    /// <summary>
    /// Shutdown telephony
    /// </summary>
    public void Shutdown()
    {
      this.CallList.Clear();
      Factory.getCommonProxy().shutdown();
    }

    /////////////////////////////////////////////////////////////////////////////////////////
    // Call handling routines

    /// <summary>
    /// Handler for outgoing calls (accountId is not known).
    /// </summary>
    /// <param name="number">Number to call</param>
    public CStateMachine createOutboundCall(string number)
    {
      int accId = Config.DefaultAccountIndex;
      return this.createOutboundCall(number, accId);
    }

    /// <summary>
    /// Handler for outgoing calls (sessionId is not known yet).
    /// </summary>
    /// <param name="number">Number to call</param>
    /// <param name="accountId">Specified account Id </param>
    public CStateMachine createOutboundCall(string number, int accountId)
    {
      // check if current call automatons allow session creation.
      if (this.getNoCallsInStates((int)(EStateId.CONNECTING | EStateId.ALERTING)) > 0)
      {
        // new call not allowed!
        return null;
      }
      // if at least 1 connected try to put it on hold
      if (this.getNoCallsInState(EStateId.ACTIVE) == 0)
      {
        // create state machine
        // TODO check max calls!!!!
        CStateMachine call = new CStateMachine(this);

        // make call request (stack provides new sessionId)
        int newsession = call.getState().makeCall(number, accountId);
        if (newsession == -1)
        {
          return null;
        }
        // update call table
        // TODO catch argument exception (same key)!!!!
        call.Session = newsession;
        _calls.Add(newsession, call);
        return call;
      }
      else // we have at least one ACTIVE call
      {
        // put connected call on hold
        // TODO pending action
        _pendingAction = new PendingAction(EPendingActions.ECreateSession, number, accountId);
        CStateMachine call = getCallInState(EStateId.ACTIVE); 
        call.getState().holdCall(call.Session);
      }
      return null;
    }
    
    /// <summary>
    /// Handler for incoming calls (sessionId is known).
    /// Check for forwardings or DND
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="number"></param>
    /// <returns>call instance</returns>
    public CStateMachine createSession(int sessionId, string number)
    {
      CStateMachine call = new CStateMachine(this);

      if (null == call) return null; 

      // save session parameters
      call.Session = sessionId;
      _calls.Add(sessionId, call);
      
      // notify GUI
      updateGui();

      return call;
    }

    /// <summary>
    /// Destroy call 
    /// </summary>
    /// <param name="session">session identification</param>
    public void destroySession(int session)
    {
      _calls.Remove(session);

      updateGui();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public CStateMachine getCall(int session)
    {
      if ((_calls.Count == 0) || (!_calls.ContainsKey(session))) return null;
      return _calls[session];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="stateId"></param>
    /// <returns></returns>
    public CStateMachine getCallInState(EStateId stateId)
    {
      if (_calls.Count == 0)  return null;
      foreach (KeyValuePair<int, CStateMachine> call in _calls)
      {
        if (call.Value.getStateId() == stateId) return call.Value;
      }
      return null;
    }

    public int getNoCallsInState(EStateId stateId)
    {
      int cnt = 0;
      foreach (KeyValuePair<int, CStateMachine> kvp in _calls)
      {
        if (stateId == kvp.Value.getStateId())
        {
          cnt++;
        }
      }
      return cnt;
    }

    private int getNoCallsInStates(int states)
    {
      int cnt = 0;
      foreach (KeyValuePair<int, CStateMachine> kvp in _calls)
      {
        if ((states & (int)kvp.Value.getStateId()) == (int)kvp.Value.getStateId())
        {
          cnt++;
        }
      }
      return cnt;
    }

    /// <summary>
    /// Collect statemachines being in a given state
    /// </summary>
    /// <param name="stateId">state machine state</param>
    /// <returns>List of state machines</returns>
    public ICollection<CStateMachine> enumCallsInState(EStateId stateId)
    {
      List<CStateMachine> list = new List<CStateMachine>();

      foreach (KeyValuePair<int, CStateMachine> kvp in _calls)
      {
        if (stateId == kvp.Value.getStateId())
        {
          list.Add(kvp.Value);
        }
      }
      return list;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // User handling routines

    /// <summary>
    /// User triggers a call release for a given session
    /// </summary>
    /// <param name="session">session identification</param>
    public void onUserRelease(int session)
    {
      this[session].getState().endCall(session);
    }

    /// <summary>
    /// User accepts call for a given session
    /// In case of multi call put current active call to Hold
    /// </summary>
    /// <param name="session">session identification</param>
    public void onUserAnswer(int session)
    {
      List<CStateMachine> list = (List<CStateMachine>)this.enumCallsInState(EStateId.ACTIVE);
      // should not be more than 1 call active
      if (list.Count > 0)
      {
        // put it on hold
        CStateMachine sm = list[0];
        if (null != sm) sm.getState().holdCall(sm.Session);

        // set ANSWER event pending for HoldConfirm
        // TODO
        _pendingAction = new PendingAction(EPendingActions.EUserAnswer, session);
        return;
      }
      this[session].getState().acceptCall(session);
    }

    /// <summary>
    /// User put call on hold or retrieve 
    /// </summary>
    /// <param name="session">session identification</param>
    public void onUserHoldRetrieve(int session)
    {
      // check Hold or Retrieve
      CAbstractState state = this[session].getState();
      if (state.StateId == EStateId.ACTIVE)
      {
        this.getCall(session).getState().holdCall(session);
      }
      else if (state.StateId == EStateId.HOLDING)
      {
        // execute retrieve
        // check if any ACTIVE calls
        if (this.getNoCallsInState(EStateId.ACTIVE) > 0)
        {
          // get 1st and put it on hold
          CStateMachine sm = ((List<CStateMachine>)enumCallsInState(EStateId.ACTIVE))[0];
          if (null != sm) sm.getState().holdCall(sm.Session);

          // set Retrieve event pending for HoldConfirm
          _pendingAction = new PendingAction(EPendingActions.EUserHold, session);
          return;
        }

        this[session].getState().retrieveCall(session);
      }
      else
      {
        // illegal
      }
    }

    /// <summary>
    /// User starts a call transfer
    /// </summary>
    /// <param name="session">session identification</param>
    /// <param name="number">number to transfer</param>
    public void onUserTransfer(int session, string number)
    {
      this[session].getState().xferCall(session, number);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="digits"></param>
    /// <param name="mode"></param>
    public void onUserDialDigit(int session, string digits, int mode)
    {
      this[session].getState().dialDtmf(session, digits, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    public void onUserConference(int session)
    {
      // check preconditions: 1 call active, other held
      // 1st if current call is held -> search if any active -> execute retrieve
      if ((getNoCallsInState(EStateId.ACTIVE) == 1)&&(getNoCallsInState(EStateId.HOLDING) >= 1))
      {
        CStateMachine call = getCallInState(EStateId.HOLDING);
        call.getState().retrieveCall(call.Session);
        // set conference flag
        return;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void activatePendingAction()
    {
      if (null != _pendingAction) _pendingAction.Activate();
      _pendingAction = null;
    }
    
    #endregion  // public methods

    #region Private Methods
    ////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 
    /// </summary>
    /// <param name="callId"></param>
    /// <param name="callState"></param>
    private void OnCallStateChanged(int callId, int callState, string info)
    {
      //    PJSIP_INV_STATE_NULL,	    /**< Before INVITE is sent or received  */
      //    PJSIP_INV_STATE_CALLING,	    /**< After INVITE is sent		    */
      //    PJSIP_INV_STATE_INCOMING,	    /**< After INVITE is received.	    */
      //    PJSIP_INV_STATE_EARLY,	    /**< After response with To tag.	    */
      //    PJSIP_INV_STATE_CONNECTING,	    /**< After 2xx is sent/received.	    */
      //    PJSIP_INV_STATE_CONFIRMED,	    /**< After ACK is sent/received.	    */
      //    PJSIP_INV_STATE_DISCONNECTED,   /**< Session is terminated.		    */
      //if (callState == 2) return 0;

      CStateMachine sm = getCall(callId);
      if (sm == null) return;

      switch (callState)
      {
        case 1:
          //sm.getState().onCalling();
          break;
        case 2:
          //sm.getState().incomingCall("4444");
          break;
        case 3:
          sm.getState().onAlerting();
          break;
        case 4:
          sm.getState().onConnect();
          break;
        case 6:
          sm.getState().onReleased();
          break;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callId"></param>
    /// <param name="number"></param>
    /// <param name="info"></param>
    private void OnIncomingCall(int callId, string number, string info)
    {
      CStateMachine sm = createSession(callId, number);
      sm.getState().incomingCall(number, info);
    }

    private void OnCallNotification(int callId, ECallNotification notFlag, string text)
    {
      if (notFlag == ECallNotification.CN_HOLDCONFIRM)
      {
        CStateMachine sm = this.getCall(callId);
        if (sm != null) sm.getState().onHoldConfirm();
      }
    }

    #endregion Methods

  }

} // namespace Sipek
