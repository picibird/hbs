// SearchTextBox.cs
// Date Created: 20.01.2016
// 
// Copyright (c) 2016, picibird GmbH 
// All rights reserved.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System.Windows.Controls;

namespace picibird.hbs.wpf.controls
{
    public class SearchTextBox : TextBox
    {
        //protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        //{
        //    base.OnGotKeyboardFocus(e);
        //    SelectAll();
        //}

        //protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        //{
        //    base.OnLostKeyboardFocus(e);
        //    SelectNothing();
        //}

        //private void SelectNothing()
        //{
        //    Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        SelectionLength = 0;
        //    }));
        //}

        //private void SelectAll()
        //{
        //    Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        SelectionStart = 0;
        //        SelectionLength = Text.Length;
        //    }));
        //}
    }
}