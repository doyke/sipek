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


using System.Collections.Generic;
using MenuDesigner;


namespace Telephony
{

  public class CConfigData
  {
    private int _sipPort = 5060;
    private string _sipProxy = "0.0.0.0";
    private bool _sipRegister = false;
    private int _sipRegPeriod = 3600;
    private string _sipUsername = "";
    private string _sipPassword = "";

    public int SipPort 
    {
      get { return _sipPort; }
      set { _sipPort = value; }
    }
    public string SipProxy 
    {
      get { return _sipProxy; }
      set { _sipProxy = value; }
    }
    public bool SipRegister 
    {
      get { return _sipRegister; }
      set { _sipRegister = value; }
    }
    public int SipRegPeriod
    {
      get { return _sipRegPeriod; }
      set { _sipRegPeriod = value; }
    }
    public string SipUsername
    {
      get { return _sipUsername; }
      set { _sipUsername = value; }
    }
    public string SipPassword 
    {
      get { return _sipPassword; }
      set { _sipPassword = value; }
    }
  
  }


  /// <summary>
  /// 
  /// </summary>
  public class CCallManager
  {
    #region Variables

    private static CCallManager _instance = null;

    private Dictionary<int, CStateMachine> _calls;  //!< Call table

    private int _currentSession = -1;

    private CConfigData _config;

    #endregion

    #region Properties

    public string SipProxy
    {
      get { return _config.SipProxy; }
    }

    #endregion Properties


    #region Constructor

    public static CCallManager getInstance()
    { 
      if (_instance == null)
      {
        _instance = new CCallManager();
      }
      return _instance;
    }

    private CCallManager()
    {
      _calls = new Dictionary<int, CStateMachine>();
      _config = new CConfigData();

    }
    #endregion Constructor


    #region Methods


    public void initialize()
    {
      // Create menu pages...
      new CConnectingPage();
      new CRingingPage();
      new CReleasedPage();
      new CActivePage();
      new CDialPage();
      new CIncomingPage();
      new CPreDialPage();


      // init SIP
      CSipProxy.initialize();

      // Initialize call table
      _calls = new Dictionary<int, CStateMachine>();
    }

    public void shutdown()
    {
      //CSipProxy proxy = new CSipProxy(0);
      //proxy.shutdown();
      CSipProxy.shutdown();
    }

    public void updateConfig(string proxy, int port)
    {
      _config.SipProxy = proxy;
      _config.SipPort = port;
    }

    public void updateGui()
    {
      // get current session
      if (_currentSession < 0)
      {
        CComponentController.getInstance().showPage((int)2);
        return;
      }

      CStateMachine call = getCall(_currentSession);
      switch (call.getStateId())
      {
        case CAbstractState.EStateId.IDLE:
          CComponentController.getInstance().showPage((int)2);
          break;
        case CAbstractState.EStateId.CONNECTING:
          CComponentController.getInstance().showPage((int)ECallPages.P_CONNECTING);
          break;
        case CAbstractState.EStateId.ALERTING:
           CComponentController.getInstance().showPage((int)ECallPages.P_RINGING);
          break;
        case CAbstractState.EStateId.ACTIVE:
          CComponentController.getInstance().showPage((int)ECallPages.P_ACTIVE);
          break;
        case CAbstractState.EStateId.RELEASED:
          CComponentController.getInstance().showPage((int)ECallPages.P_RELEASED);
          break;
        case CAbstractState.EStateId.INCOMING:
          CComponentController.getInstance().showPage((int)ECallPages.P_INCOMING);
          break;
      }
    }

    public CStateMachine getCurrentCall()
    {
      if (_currentSession < 0) return null;
      return _calls[_currentSession];
    }

    public CStateMachine getCall(int session)
    {
      if (_calls.Count == 0) return null;
      return _calls[session];
    }


    public void createSession(string number)
    {
      CStateMachine call = createCall();
      int newsession = call.getState().makeCall(number);
      if (newsession != -1)
      {
        call.Session = newsession;
        _calls.Add(newsession, call);
        _currentSession = newsession;
      }
      updateGui();
    }
    
    public CStateMachine createSession(int sessionId, string number)
    {
      CStateMachine call = createCall();
      call.Session = sessionId;
      _calls.Add(sessionId, call);
      _currentSession = sessionId;
      updateGui();
      return call;
    }

    public void destroySession()
    {
      CStateMachine call = getCall(_currentSession);
      call.getState().endCall();
    }

    public void destroySession(int session)
    {
      _calls.Remove(_currentSession);
      if (_calls.Count == 0) _currentSession = -1;
    }

    public void onUserRelease()
    {
    }

    public void onUserAnswer()
    {
      CStateMachine call = getCall(_currentSession);
      call.getState().acceptCall();
    }

    /////////////////////////////////////////////////////////////////////

    private CStateMachine createCall()
    {
      CStateMachine call = new CStateMachine(new CSipProxy(0));
      return call;
    }

    public void destroy(int session)
    {
      _calls.Remove(session);
    }

    #endregion Methods

  }

} // namespace Telephony
