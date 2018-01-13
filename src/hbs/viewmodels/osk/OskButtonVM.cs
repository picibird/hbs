using System;
using picibits.app.mvvm;
using picibits.core;
using picibits.core.helper;

namespace picibird.hbs.viewmodels.osk
{
    public class OskButtonVM : ButtonViewModel
    {
        public event EventHandler Clicked;

        private IOsk mOsk;

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
                        mOsk.toggle();
                    });

                return mClickCommand;
            }
        }

        #endregion ClickCommand


        public OskButtonVM()
        {
            mOsk = Pici.Services.Get<IOsk>();
        }
    }
}
