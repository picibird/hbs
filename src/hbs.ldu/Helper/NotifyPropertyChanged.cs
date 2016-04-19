// NotifyPropertyChanged.cs
// Date Created: 21.01.2016
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace picibird.hbs.ldu.Helper
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            PropertyChangedHelper.SetProperty<T>(ref field, value, PropertyChanged, this, name);
        }
    }

    public class PropertyChangedHelper
    {
        public static void SetProperty<T>(ref T field, T value, PropertyChangedEventHandler PropertyChanged, object caller, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(caller, new PropertyChangedEventArgs(name));
                }
            }
        }
    }
}
