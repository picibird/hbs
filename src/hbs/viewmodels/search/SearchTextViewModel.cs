// SearchTextViewModel.cs
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
using System.Windows.Input;
using picibird.hbs.viewmodels.osk;
using picibits.core;
using picibits.core.helper;
using picibits.core.mvvm;
using picibits.core.util;

namespace picibird.hbs.viewmodels.search
{
    public class SearchTextViewModel : ViewModel
    {
        #region SearchText

        private string mSearchText;

        public string SearchText
        {
            get { return mSearchText; }
            set
            {
                if (mSearchText != value)
                {
                    var old = mSearchText;
                    mSearchText = value;
                    OnSearchTextChanged(old, value);
                }
            }
        }

        protected virtual void OnSearchTextChanged(string oldSearchText, string newSearchText)
        {
            RaisePropertyChanged("SearchText", oldSearchText, newSearchText);
            if (oldSearchText != null)
            {
                //manage old View here
            }
            if (newSearchText != null)
            {
                if (newSearchText.Length == 0)
                    EmptyText = Resources["input_search"];
                else
                    EmptyText = "";
            }
        }

        #endregion SearchText

        #region EmptyText

        private string mEmptyText;

        public string EmptyText
        {
            get { return mEmptyText; }
            set
            {
                if (mEmptyText != value)
                {
                    var old = mEmptyText;
                    mEmptyText = value;
                    RaisePropertyChanged("EmptyText", old, value);
                }
            }
        }

        #endregion EmptyText

        public EnterSearchCommand EnterSearchCommand { get; private set; }
        public DelegateCommand TouchDownCommand { get; private set; }
        public DelegateCommand MouseDownCommand { get; private set; }
        public DelegateCommand GotFocusCommand { get; private set; }
        public DelegateCommand LostFocusCommand { get; private set; }

        private IOsk mOsk;
        private bool mIsTouch;

        public SearchTextViewModel()
        {
            mOsk = Pici.Services.Get<IOsk>();
            Style = new ViewStyle("SearchBoxTextStyle");
            EnterSearchCommand = new EnterSearchCommand();
            TouchDownCommand = new DelegateCommand((p) =>
            {
                mIsTouch = true;
                mOsk.open();
            });
            MouseDownCommand = new DelegateCommand((p) =>
            {
                mIsTouch = false;
            });
            GotFocusCommand = new DelegateCommand((p) => { EnterSearchCommand.CanExecuteProp = true; });
            LostFocusCommand = new DelegateCommand((p) => { EnterSearchCommand.CanExecuteProp = false; });
            Pici.Resources.CultureChanged += (s, p) => Events.OnIdleOnce(() => OnSearchTextChanged(SearchText, SearchText));
            EnterSearchCommand.OnEnter += OnEnter;

        }

        private void OnEnter(EnterSearchCommand sender)
        {
            mOsk.close();
        }
    }

    public class EnterSearchCommand : ICommand
    {
        public bool CanExecuteProp;

        public event SimpleEventHandler<EnterSearchCommand> OnEnter;

        public bool CanExecute(object parameter)
        {
            return CanExecuteProp;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (CanExecuteProp)
            {
                OnEnter?.Invoke(this);
            }
        }
    }
}