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


namespace Telephony
{
  public enum EUserStatus : int
  { 
  	AVAILABLE, 
    BUSY, 
    OTP, 
    IDLE, 
    AWAY, 
    BRB, 
    OFFLINE, 
    OPT_MAX
  }

  public interface CCallProxyInterface
  {
    int makeCall(string dialedNo, int accountId);

    bool endCall(int sessionId);

    bool alerted(int sessionId);

    bool acceptCall(int sessionId);

    bool holdCall(int sessionId);

    bool retrieveCall(int sessionId);

    bool xferCall(int sessionId, string number);

    bool xferCallSession(int sessionId, int session);

    bool threePtyCall(int sessionId, int session);

    //bool serviceRequest(EServiceCodes code, int session);
    bool serviceRequest(int sessionId, int code, string dest);

    bool dialDtmf(int sessionId, int mode, string digits);
  }

  interface CTelephonyCallback
  {
    #region Methods

    void incomingCall(string callingNo);

    void onAlerting();

    void onConnect();

    void onReleased();

    void onHoldConfirm();

    #endregion Methods
  }

  public interface CCommonProxyInterface
  {
    int initialize(); 
    int shutdown();

    int registerAccounts();
    int registerAccounts(bool renew);

    int addBuddy(string ident);

    int delBuddy(int buddyId);

    int sendMessage(string dest, string message);

    int setStatus(int accId, EUserStatus presence_state);
  }

  public interface CMediaProxyInterface
  {
    //int initialize();
    //int shutdown();

    int playTone(ETones toneId);

    int stopTone();
  }

  public interface ICallLogInterface
  {
    void addCall(ECallType type, string number, System.DateTime time, System.TimeSpan duration);

    void save();
  }

} // namespace Sipek