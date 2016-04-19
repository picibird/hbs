// FilterContainerViewModel.cs
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
using System.Linq;
using picibird.hbs.ldu;
using picibird.hbs.viewmodels.filter.behaviour;

using picibits.app.animation;
using picibits.app.transition;
using picibits.core;
using picibits.core.collection;
using picibits.core.mvvm;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs.viewmodels.filter
{
    public class FilterContainerViewModel : ViewModel
    {
        public delegate void FiltersAppliedHandler(
            FilterContainerViewModel filterContainer, PiciObservableCollection<Filter> filter);

        public FilterContainerViewModel()
        {
            Style = new ViewStyle("FilterContainerViewStyle");
            Chooser = new FilterChooserViewModel();
            Chooser.PropertyChanged += OnChooserPropertyChanged;

            Pointing.IsEnabled = true;

            SwipeBehaviour = new FilterSwipeBehaviour();
            Behaviours.Add(SwipeBehaviour);

            CreateChooserLoadedTransition(Chooser);
        }

        #region Chooser

        public FilterChooserViewModel Chooser { get; }

        #endregion Chooser

        public FilterSwipeBehaviour SwipeBehaviour { get; }

        protected override void OnViewIsLoadedChanged(bool oldViewIsLoaded, bool newViewIsLoaded)
        {
            base.OnViewIsLoadedChanged(oldViewIsLoaded, newViewIsLoaded);
            if (newViewIsLoaded)
            {
                var loadedAni = ChooserLoadedTransition.AnimateProgressTo(1);
            }
        }

        public override void RaisePropertyChanged(string name, object oldValue = null, object newValue = null)
        {
            base.RaisePropertyChanged(name, oldValue, newValue);
            if (name.Equals("VisualState"))
            {
                if (newValue.Equals(FilterContainerVisualStates.DISCARDED))
                {
                    //DISCARD
                }
            }
        }

        private void OnFilterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var args = e as global::picibits.core.mvvm.PropertyChangedEventArgs;
            if (e.PropertyName.Equals("ViewIsLoaded"))
            {
                OnFilterViewIsLoadedChanged(Filter.ViewIsLoaded);
            }
            if (e.PropertyName.Equals("VisualState"))
            {
                OnFilterVisualStateChanged((string) args.Old, (string) args.New);
            }
        }

        private void OnChooserPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var args = e as global::picibits.core.mvvm.PropertyChangedEventArgs;
            if (e.PropertyName.Equals("SelectedFilterCategory"))
            {
                OnSelectedFilterCategoryChanged(Chooser.SelectedFilterCategory);
            }
            if (e.PropertyName.Equals("VisualState"))
            {
                OnChooserVisualStateChanged((string) args.Old, (string) args.New);
            }
        }

        private void OnChooserVisualStateChanged(string oldVS, string newVS)
        {
            if (newVS == FilterChooserStates.OPENED)
            {
                var openTransition = CreateChooserOpenedTransition(Chooser);
                var openAni = openTransition.AnimateProgressTo(1);
                openAni.Complete += OnOpenTransitionComplete;
            }
            if (newVS == FilterChooserStates.CHOSEN)
            {
                var chosenTransition = CreateChooserChosenTransition(Chooser);
                var chosenAni = chosenTransition.AnimateProgressTo(1);
                chosenAni.Complete += OnCloseTransitionComplete;
            }
        }

        private void OnOpenTransitionComplete(EaseObject easeObject, double percent)
        {
        }

        private void OnCloseTransitionComplete(EaseObject easeObject, double percent)
        {
            Chooser.VisualState = FilterChooserStates.DISCARDED;
        }

        private void OnFilterViewIsLoadedChanged(bool viewIsLoaded)
        {
            if (viewIsLoaded)
            {
                var filterLoadedTransition = CreateFilterLoadedTransition(Filter);
                var filterLoadedAni = filterLoadedTransition.AnimateProgressTo(1);
                filterLoadedAni.Complete += OnFilterLoadedTransitionComplete;
            }
        }

        private void OnFilterLoadedTransitionComplete(EaseObject easeObject, double percent)
        {
        }

        private void OnSelectedFilterCategoryChanged(FilterCategoryId categoryName)
        {
            Pici.Log.info(typeof (FilterChooserViewModel),
                string.Format("selected filter category {0}", Chooser.SelectedFilterCategory));
            var category = HBS.Search.FilterList.FirstOrDefault(fc => fc.Id == categoryName);
            if (category != null)
            {
                if (category.Id == FilterCategoryId.date)
                {
                    Filter = new DateFilterVM(category);
                    //Filter = new ListFilterViewModel(category);
                }
                else if (category.Id == FilterCategoryId.available)
                {
                    Filter = new AvailableFilterViewModel(category);
                    FiltersApplied(this, Filter.SelectedFilter);
                }
                else
                {
                    Filter = new ListFilterViewModel(category);
                }
            }
            else
            {
                if (categoryName == FilterCategoryId.digital)
                {
                    Filter = new DigitalFilterVM();
                    FiltersApplied(this, Filter.SelectedFilter);
                }
            }
        }

        public event FiltersAppliedHandler FiltersApplied;

        private void OnFilterVisualStateChanged(string oldVS, string newVS)
        {
            if (oldVS == FilterVisualStates.EDIT && newVS == FilterVisualStates.NORMAL)
            {
                if (FiltersApplied != null)
                {
                    FiltersApplied(this, Filter.SelectedFilter);
                }
            }

            if (newVS == FilterVisualStates.Delete)
                Filter.ApplyButtonViewModel.TapBehaviour.Tap += OnDeleteTap;
            else
                Filter.ApplyButtonViewModel.TapBehaviour.Tap -= OnDeleteTap;
        }

        private void OnDeleteTap(object sender, EventArgs e)
        {
            SwipeBehaviour.FinishSwipeWithAnimation();
        }

        #region Filter

        private FilterViewModel mFilter;

        public FilterViewModel Filter
        {
            get { return mFilter; }
            set
            {
                if (mFilter != value)
                {
                    var old = mFilter;
                    mFilter = value;
                    OnFilterChanged(old, value);
                }
            }
        }

        protected virtual void OnFilterChanged(FilterViewModel oldFilter, FilterViewModel newFilter)
        {
            RaisePropertyChanged("Filter", oldFilter, newFilter);
            if (oldFilter != null)
            {
                oldFilter.PropertyChanged -= OnFilterPropertyChanged;
            }
            if (newFilter != null)
            {
                newFilter.PropertyChanged += OnFilterPropertyChanged;
                OnFilterViewIsLoadedChanged(newFilter.ViewIsLoaded);
            }
        }

        #endregion Filter

        #region  Chooser Transitions

        public double TransitionDuration = 0.5;

        private TransformTransition ChooserLoadedTransition;

        private TransformTransition CreateChooserLoadedTransition(FilterChooserViewModel chooser)
        {
            ChooserLoadedTransition = new TransformTransition(chooser)
            {
                FromX = 1.2,
                FromY = -0.2,
                FromAngle = -50,
                ToX = 0.7,
                ToY = 0.2,
                ToAngle = -30,
                Duration = TransitionDuration
            };
            return ChooserLoadedTransition;
        }

        private TransformTransition ChooserOpenedTransition;

        private TransformTransition CreateChooserOpenedTransition(FilterChooserViewModel chooser)
        {
            ChooserOpenedTransition = new TransformTransition(chooser)
            {
                FromX = 0.7,
                FromY = 0.2,
                FromAngle = -30,
                Duration = TransitionDuration
            };
            return ChooserOpenedTransition;
        }

        private TransformTransition ChooserChosenTransition;

        private TransformTransition CreateChooserChosenTransition(FilterChooserViewModel chooser)
        {
            ChooserChosenTransition = new TransformTransition(chooser)
            {
                ToX = -1,
                Duration = TransitionDuration
            };
            ChooserChosenTransition.ProgressChanged +=
                (target, progress) => { ChooserChosenTransition.Target.Opacity = 1 - progress; };
            return ChooserChosenTransition;
        }

        #endregion Chooser Transitions

        #region Filter Transitions

        private TransformTransition FilterLoadedTransition;

        private TransformTransition CreateFilterLoadedTransition(FilterViewModel filter)
        {
            FilterLoadedTransition = new TransformTransition(filter)
            {
                FromX = 1.2,
                Duration = TransitionDuration
            };
            return FilterLoadedTransition;
        }

        #endregion Filter Transitions
    }

    public static class FilterContainerVisualStates
    {
        public static readonly string NORMAL = "Normal";
        public static readonly string DISCARDED = "Discarded";
        public static readonly string CLOSED = "Closed";
    }
}