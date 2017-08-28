using System;
using picibits.app.mvvm;
using picibits.core.helper;
using picibits.core;
using System.Globalization;
using System.Collections.Generic;

namespace picibird.hbs.viewmodels.search
{
    public class LanguageButtonViewModel : ButtonViewModel
    {

        public static List<CultureInfo> AvailableLanguages = new List<CultureInfo>();


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

        public LanguageButtonViewModel()
        {
            AvailableLanguages.Add(new CultureInfo("de-DE"));
            AvailableLanguages.Add(new CultureInfo("en-US"));
            if (Pici.Resources.CultureInfo.Name == "en-US")
                AvailableLanguages.Reverse();
            Clicked += OnClicked;
            Pici.Resources.CultureChanged += OnCultureChanged;
            OnCultureChanged(null, null);
        }

        private void OnClicked(object sender, EventArgs e)
        {
            var buf = AvailableLanguages[0];
            AvailableLanguages.RemoveAt(0);
            AvailableLanguages.Add(buf);
            Pici.Resources.CultureInfo = AvailableLanguages[0];
        }


        private void OnCultureChanged(System.Globalization.CultureInfo sender, System.Globalization.CultureInfo param)
        {
            var languageTitle = Pici.Resources.CultureInfo.Name;
            languageTitle = Pici.Resources.Find(languageTitle);
            Text = languageTitle;
        }
    }
}