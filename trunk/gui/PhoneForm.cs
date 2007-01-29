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

using System;
using System.Windows.Forms;

using MenuDesigner;
using Telephony;

namespace Gui
{
  public partial class PhoneForm : Form
  {

    protected CComponentController control;
    private int _caretPos = -1;
    private int _selection = -1;
    private System.Collections.Generic.List<Button> _buttons;

    // languages
    public static CLanguage _langEN = new CEnLanguage();
    public static CDeLanguage _langDE = new CDeLanguage();

    /// <summary>
    /// 
    /// </summary>
    public PhoneForm()
    {
      InitializeComponent();

      ///////////////////////////////////////////////////////////////
      // sasacoh coding
      ///////////////////////////////////////////////////////////////
      string str = new string(' ', 255);
      richTextBox1.Lines = new string[] { str, str, str, str, str, str, str, str, str, str };
      _buttons = new System.Collections.Generic.List<Button>();

      control = CComponentController.getInstance();

      Renderer renderer = new Renderer(this);
      CTimerFactoryImpl tmrFactory = new CTimerFactoryImpl();

      control.attach(renderer);
      control.setTimerFactory(tmrFactory);
      control.Language = _langEN;

      // Create menu pages...
      new CInitPage();
      new IdlePage();
      new CPreDialPage();
      new CPhonebookPage();
      
      control.initialize();

      // set active page...
      control.setActivePage((int)EPages.P_INIT);

      // initialize telephony...
      CCallManager.getInstance().initialize();
    }

    public void clearScreen()
    {
      _caretPos = -1;
      _selection = -1;
    }

    public void writeText(int xaxis, int yaxis, String text)
    {
      string[] tempArray = richTextBox1.Lines;
      string line = tempArray[yaxis];
      line = line.Remove(xaxis, text.Length);
      tempArray[yaxis] = line.Insert(xaxis, text);
      richTextBox1.Lines = tempArray;
      // use stored value to prevent selection overridance
      if (_caretPos >= 0) richTextBox1.Select(_caretPos, 1);
      if (_selection >= 0) richTextBox1.Select(_selection, 23);
    }

    public void setCursor(int xaxis, int yaxis)
    {
      _caretPos = yaxis * 256 + xaxis;
      richTextBox1.Select(_caretPos, 1); // simulate caret
    }

    public void setSelection(int startX, int startY, int length)
    {
      _selection = startX + startY * 256;
      richTextBox1.Select(_selection, length);
    }

    public void drawButton(int x, int y)
    {
      Button menuButton = new Button();
      menuButton.Location = new System.Drawing.Point(x * 13, 14 + y * 22);
      menuButton.Size = new System.Drawing.Size(41, 23);
      menuButton.Click += new EventHandler(menuButton_Click);
      menuButton.BackColor = System.Drawing.Color.DarkGray;
      _buttons.Add(menuButton);
      this.Controls.Add(menuButton);

      //richTextBox1.SendToBack();
      //menuButton.BringToFront();
    }

    void menuButton_Click(object sender, EventArgs e)
    {
      Button origin = (Button)sender;
      int orgy = origin.Location.Y;
      int y = (orgy - 14) / 22;

      control.getAccessIf().onSoftKey(y);
    }

    public void eraseButton()
    {
      foreach (Button btn in _buttons)
      {
        this.Controls.Remove(btn);
      }
      _buttons.Clear();
    }

    private void digitKey1_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_1);
    }

    private void digitKey2_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_2);
    }

    private void digitKey3_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_3);
    }

    private void digitKey4_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_4);
    }

    private void digitKey5_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_5);
    }

    private void digitKey6_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_6);
    }

    private void digitKey7_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_7);
    }

    private void digitKey8_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_8);
    }

    private void digitKey9_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_9);
    }

    private void digitKey0_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_0);
    }

    private void digitKeyS_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_star);
    }

    private void digitKeyH_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onDigitKey((int)ENumKeyTags.NumKey_hash);
    }

    private void OKbutton_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onOkKey();
    }

    private void onhookButton_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onOnHookKey();
    }

    private void offhookButton_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onOffHookKey();
    }

    private void Cancel_Click(object sender, EventArgs e)
    {
      control.getAccessIf().onEscKey();
    }

    private void PhoneForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      CCallManager.getInstance().shutdown();
    }

  }
}