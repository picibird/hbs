using System;
using picibits.app.mvvm;
using picibits.core.helper;

namespace picibird.hbs.viewmodels.search
{
    public class SearchButtonViewModel : ButtonViewModel
    {
        public event EventHandler Clicked;

        #region ClickCommand

        private DelegateCommand mClickCommand;

        public DelegateCommand ClickCommand
        {
            get
            {
                if (mClickCommand == null)
                    mClickCommand = new DelegateCommand((param) => Clicked?.Invoke(this, EventArgs.Empty));
                return mClickCommand;
            }
        }

        #endregion ClickCommand

        
    }
}