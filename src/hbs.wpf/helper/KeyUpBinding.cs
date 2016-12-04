// KeyUpBinding.cs
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

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace picibird.hbs.wpf.helper
{
    public class KeyUpBinding : InputBinding
    {
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(Key), typeof(KeyUpBinding),
                new PropertyMetadata(Key.A, KeyChanged));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(KeyUpBinding), new PropertyMetadata(null));

        public Key Key
        {
            get { return (Key) GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }


        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private static void KeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var keybinding = d as KeyUpBinding;

            Keyboard.AddKeyUpHandler(Application.Current.MainWindow, (s, ku) =>
            {
                if (keybinding.Command != null && ku.Key == keybinding.Key && ku.IsUp)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke((Action) (() => { keybinding.Command.Execute(null); }),
                        DispatcherPriority.Input);
                }
            });
        }
    }
}