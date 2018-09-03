using picibits.app.mvvm;
using picibits.core.helper;
using picibits.core.mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace picibird.hbs.viewmodels.buttons
{
    public class BackButtonVM : ButtonViewModel
    {

        public event EventHandler Clicked;
        

        #region ClickCommand

        private DelegateCommand mClickCommand;

        public DelegateCommand ClickCommand
        {
            get
            {
                if (mClickCommand == null)
                    mClickCommand = new DelegateCommand((param) =>
                    {
                        Clicked?.Invoke(this, EventArgs.Empty);
                    });

                return mClickCommand;
            }
        }

        #endregion ClickCommand


        public BackButtonVM()
        {
            Style = new ViewStyle("BackButtonStyle");
            Visibility = false;
        }

    }
}
