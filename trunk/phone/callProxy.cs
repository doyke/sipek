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

using System.Runtime.InteropServices;
using System.Threading;
using System;
using System.Net;
using System.Net.Sockets;


namespace Sipek
{
  /// <summary>
  /// 
  /// </summary>
  public class CCallProxy : CTelephonyInterface
  {
    // call API
    [DllImport("pjsipDll.dll")]
    private static extern int dll_registerAccount(string uri, string reguri, string domain, string username, string password);

    [DllImport("pjsipDll.dll")]
    private static extern int dll_makeCall(int accountId, string uri);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_releaseCall(int callId);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_answerCall(int callId, int code);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_holdCall(int callId);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_retrieveCall(int callId);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_xferCall(int callId, string uri);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_xferCallWithReplaces(int callId, int dstSession);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_serviceReq(int callId, int serviceCode, string destUri);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_dialDtmf(int callId, string digits, int mode);

    // identify line
    private int _line;
    private int _accountId;

    #region Constructor

    public CCallProxy(int line)
    {
      _line = line;
    }

    #endregion Constructor


    #region Methods

    public int makeCall(string dialedNo, int accountId)
    {
      _accountId = accountId;
      string uri = "sip:" + dialedNo + "@" + CAccounts.getInstance()[accountId].Address;
      _line = dll_makeCall(accountId, uri);
      return _line;
    }

    public bool endCall()
    {
      dll_releaseCall(_line);
      return true;
    }

    public bool alerted()
    {
      dll_answerCall(_line, 180);
      return true;
    }

    public bool acceptCall()
    {
      dll_answerCall(_line, 200);
      return true;
    }
    
    public bool holdCall()
    {
      dll_holdCall(_line);
      return true;
    }
    
    public bool retrieveCall()
    {
      dll_retrieveCall(_line);
      return true;
    }
          
    public bool xferCall(string number)
    {
      string uri = "sip:" + number + "@" + CAccounts.getInstance()[_accountId].Address;
      dll_xferCall(_line, uri);
      return true;
    }
    
    public bool xferCallSession(int session)
    {
      dll_xferCallWithReplaces(_line, session);
      return true;
    }

    public bool threePtyCall(int session)
    {
      dll_serviceReq(_line, (int)EServiceCodes.SC_3PTY,"");
      return true;
    }
    
    public bool serviceRequest(int code, string dest)
    {
      string destUri = "<sip:" + dest + "@" + CAccounts.getInstance().DefAccount.Address + ">";
      dll_serviceReq(_line, (int)code, destUri);
      return true;
    }

    public bool dialDtmf(int mode, string digits)
    {
      dll_dialDtmf(_line, digits, mode);
      return true;
    }

    #endregion Methods

  }

  /// <summary>
  /// 
  /// </summary>
  public static class CPjSipProxy
  {

    #region Wrapper functions
    // callback delegates
    delegate int GetConfigData(int cfgId);
    delegate int OnRegStateChanged(int accountId, int regState);
    delegate int OnCallStateChanged(int callId, int stateId);
    delegate int OnCallIncoming(int callId, string number);
    delegate int OnCallHoldConfirm(int callId);


    [DllImport("pjsipDll.dll")]
    private static extern int dll_init(int listenPort);
    [DllImport("pjsipDll.dll")]
    private static extern int dll_main();
    [DllImport("pjsipDll.dll")]
    private static extern int dll_shutdown();
    
    // callbacks
    [DllImport("pjsipDll.dll")]
    private static extern int onCallStateCallback(OnCallStateChanged cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onRegStateCallback(OnRegStateChanged cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onCallIncoming(OnCallIncoming cb);
    [DllImport("pjsipDll.dll")]
    private static extern int getConfigDataCallback(GetConfigData cb);
    [DllImport("pjsipDll.dll")]
    private static extern int onCallHoldConfirmCallback(OnCallHoldConfirm cb);
    
    // call API
    [DllImport("pjsipDll.dll")]
    private static extern int dll_registerAccount(string uri, string reguri, string domain, string username, string password);
    
    #endregion Wrapper functions

    #region Variables

    static OnCallStateChanged csDel = new OnCallStateChanged(onCallStateChanged);
    static OnRegStateChanged rsDel = new OnRegStateChanged(onRegStateChanged);
    static OnCallIncoming ciDel = new OnCallIncoming(onCallIncoming);
    static GetConfigData gdDel = new GetConfigData(getConfigData);
    static OnCallHoldConfirm chDel = new OnCallHoldConfirm(onCallHoldConfirm);

    #endregion Variables

    #region Methods

    public static void initialize()
    {
      // register callbacks (delegates)
      onCallIncoming( ciDel );
      onCallStateCallback( csDel );
      onRegStateCallback( rsDel );
      onCallHoldConfirmCallback(chDel);

      // Initialize pjsip...
      int port = Properties.Settings.Default.cfgSipPort;
      dll_init(port);
      dll_main();
    }

    public static int shutdown()
    {
      return dll_shutdown();
    }

    public static void restart()
    {
      shutdown();

      int port = Properties.Settings.Default.cfgSipPort;

      dll_init(port);
      dll_main();
    }

    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    // Call API
    //
    public static int registerAccounts()
    {
      for (int i = 0; i < CAccounts.getInstance().getSize(); i++)
      {
        CAccount acc = CAccounts.getInstance()[i];
        if (acc.Name.Length > 0)
        {
          string uri = "sip:" + CAccounts.getInstance()[i].Id + "@" + CAccounts.getInstance()[i].Address;
          string reguri = "sip:" + CAccounts.getInstance()[i].Address; // +":" + CCallManager.getInstance().SipProxyPort;
          //dll_registerAccount("sip:1341@interop.pingtel.com", "sip:interop.pingtel.com", "interop.pingtel.com", "1341", "1234");
          string domain = CAccounts.getInstance()[i].Domain;
          string username = CAccounts.getInstance()[i].Username;
          string password = CAccounts.getInstance()[i].Password;
          int accId = dll_registerAccount(uri, reguri, domain, username, password);
          // todo:::check if accId corresponds to account index!!!
        }
      }
      return 1;
    }

    #endregion Methods

    #region Callbacks

    private static int onCallStateChanged(int callId, int callState)
    {
//    PJSIP_INV_STATE_NULL,	    /**< Before INVITE is sent or received  */
//    PJSIP_INV_STATE_CALLING,	    /**< After INVITE is sent		    */
//    PJSIP_INV_STATE_INCOMING,	    /**< After INVITE is received.	    */
//    PJSIP_INV_STATE_EARLY,	    /**< After response with To tag.	    */
//    PJSIP_INV_STATE_CONNECTING,	    /**< After 2xx is sent/received.	    */
//    PJSIP_INV_STATE_CONFIRMED,	    /**< After ACK is sent/received.	    */
//    PJSIP_INV_STATE_DISCONNECTED,   /**< Session is terminated.		    */
      if (callState == 2) return 0;

      CStateMachine sm = CCallManager.getInstance().getCall(callId);
      if (sm == null) return 0;

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
      return 1;
    }

    private static int onCallIncoming(int callId, string uri)
    {
      string number = uri.Replace("<sip:","");

      int atPos = number.IndexOf('@');
      if (atPos >= 0)
      {
        number = number.Remove(atPos);
      }
      else 
      { 
        int semiPos = number.IndexOf(';');
        if (semiPos >= 0)
        {
          number = number.Remove(semiPos);
        }
        else 
        {
          int colPos = number.IndexOf(':');
          if (colPos >= 0)
          {
            number = number.Remove(colPos);
          }
        }

      }

      CStateMachine sm = CCallManager.getInstance().createSession(callId, number);
      sm.getState().incomingCall(number);
      return 1;
    }


    private static int onRegStateChanged(int accId, int regState)
    {
      switch (regState)
      {
        case 200: 
            CAccounts.getInstance()[accId].RegState = ERegistrationState.ERegistered;
          break;
        default:
          CAccounts.getInstance()[accId].RegState = ERegistrationState.ENotRegistered;
          break;
      }
      CCallManager.getInstance().updateGui();
      return 1;
    }


    private static int getConfigData(int cfgId)
    {

      return 1;
    }

    private static int onCallHoldConfirm(int callId)
    {
      CStateMachine sm = CCallManager.getInstance().getCall(callId);
      if (sm != null) sm.getState().onHoldConfirm();
      return 1;
    }

    #endregion Callbacks

  }

} // namespace Sipek
