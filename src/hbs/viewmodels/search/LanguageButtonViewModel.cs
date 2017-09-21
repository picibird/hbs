using System;
using picibits.app.mvvm;
using picibits.core.helper;
using picibits.core;
using System.Globalization;
using System.Collections.Generic;
using picibits.core.mvvm;
using picibits.app.animation;

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
            IsEnabled = false;
            Opacity = 0.0d;
            if (Pici.Resources.CultureInfo.Name == "en-US")
                AvailableLanguages.Reverse();
            Clicked += OnClicked;
            Pici.Resources.CultureChanged += OnCultureChanged;
            OnCultureChanged(null, null);
        }

        public override void RaisePropertyChanged(string name, object oldValue = null, object newValue = null)
        {
            base.RaisePropertyChanged(name, oldValue, newValue);
            if(name == nameof(ViewModel.IsEnabled))
            {
                ArtefactAnimator.AddEase(this, new[] { "Opacity" }, new object[] { IsEnabled ? 1 : 0 }, 1);
            }
        }

        private void OnClicked(object sender, EventArgs e)
        {
            var buf = AvailableLanguages[0];
            AvailableLanguages.RemoveAt(0);
            AvailableLanguages.Add(buf);
            Pici.Resources.CultureInfo = AvailableLanguages[0];
            Events.OnIdleOnce(() => HBS.ViewModel.OnSearch());
        }


        private void OnCultureChanged(System.Globalization.CultureInfo sender, System.Globalization.CultureInfo param)
        {
            var languageTitle = Pici.Resources.CultureInfo.Name.Substring(0, 2);
            while (!AvailableLanguages[0].Name.StartsWith(languageTitle))
            {
                var buf = AvailableLanguages[0];
                AvailableLanguages.RemoveAt(0);
                AvailableLanguages.Add(buf);
            }
            languageTitle = Pici.Resources.Find(languageTitle);
            Text = languageTitle;

        }
    }
}