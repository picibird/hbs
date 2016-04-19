// SpineViewModel.cs
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
using picibits.core.helper;

namespace picibird.hbs.viewmodels.book
{
    public class SpineViewModel : BookPlaneViewModel
    {
        private void LoadQRCodeURL()
        {
            //Intent openUrlIntent = new Intent(Intent.ACTION_OPEN, Uri);
            //openUrlIntent.AddExtra("title", Title);
            //openUrlIntent.AddExtra("subtitle", Subtitle);
            //Pici.Intent.Send(openUrlIntent);

            //IProcesses processes = Pici.Services.Create<IProcesses>();
            //processes.StartProcess(QRCodeURL);
        }

        #region QrCodeUrl

        private string mQrCodeUrl;

        public string QrCodeUrl
        {
            get { return mQrCodeUrl; }
            set
            {
                if (mQrCodeUrl != value)
                {
                    var old = mQrCodeUrl;
                    mQrCodeUrl = value;
                    RaisePropertyChanged("QrCodeUrl", old, value);
                }
            }
        }

        #endregion QrCodeUrl

        #region QRCodeClickCommand

        private DelegateCommand mQRCodeClickCommand;

        public DelegateCommand QRCodeClickCommand
        {
            get
            {
                if (mQRCodeClickCommand == null)
                    mQRCodeClickCommand = new DelegateCommand(param => LoadQRCodeURL());
                return mQRCodeClickCommand;
            }
        }

        #endregion QRCodeClickCommand
    }

    public class QrCodeClickCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
        }
    }
}