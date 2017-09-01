// AvailabilityItemsVM.cs
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
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using picibird.hbs.availabity;
using picibird.hbs.ldu;
using picibits.app.animation;
using picibits.core.controls;
using picibits.core.mvvm;
using picibits.core;

namespace picibird.hbs.viewmodels.availability
{
    public class AvailabilityItemsVM : ItemsViewModel
    {
        private EaseObject opacityAnimation;


        public AvailabilityItemsVM()
        {
            Style = new ViewStyle("AvailabilityItemsViewStyle");
            Opacity = 0;
            Items.CollectionChanged += OnItemsCollectionChanged;
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Has = Items.Count > 0;
        }

        private void ToggleItemsFilledOpacityAnimation(bool visible)
        {
            if (opacityAnimation != null)
                opacityAnimation.Stop();
            if (visible)
                Opacity = 0;
            else
                Opacity = 1;
            opacityAnimation = ArtefactAnimator.AddEase(this, "Opacity", visible ? 1 : 0, 0.5);
        }

        protected override void OnModelChanged(Model oldModel, Model newModel)
        {
            base.OnModelChanged(oldModel, newModel);
            if (oldModel != null)
            {
                UpdateTitle(null);
                ToggleItemsFilledOpacityAnimation(false);
            }
            if (newModel != null && newModel is Hit)
            {
                var hit = newModel as Hit;
                UpdateTitle(hit);
                UpdateAvailability(hit);
            }
        }

        private void UpdateTitle(Hit hit)
        {
            var title = Pici.Resources.Find("availability");
            if (hit != null)
            {
                var swb = hit.GetLocationWithSource(Sources.SWB);
                if (swb != null)
                {
                    title += string.Format(" ({0} of {1})", swb.countAvailable, swb.countExisting);
                }
            }
            Title = title;
        }

        public void UpdateAvailability(Hit hit)
        {
            if (hit != null)
            {
                Items.RemoveAll();
                try
                {
                    var service = HBS.Injections.GetInstance<IAvailabilityService>();
                    if (service != null)
                    {
                        service.Check(hit).ContinueWith(task =>
                        {
                            if (task.Status == TaskStatus.RanToCompletion)
                            {
                                var availabilities = task.Result;
                                if (availabilities.Count() > 0)
                                {
                                    foreach (var av in availabilities)
                                    {
                                        var availVM = new AvailabilityVM(hit.CoverColorScheme, av);
                                        Items.Add(availVM);
                                    }
                                    ToggleItemsFilledOpacityAnimation(true);
                                }
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }

        #region Has

        private bool mHas;

        public bool Has
        {
            get { return mHas; }
            set
            {
                if (mHas != value)
                {
                    var old = mHas;
                    mHas = value;
                    RaisePropertyChanged("Has", old, value);
                }
            }
        }

        #endregion Has

        #region Title

        private string mTitle;

        public string Title
        {
            get { return mTitle; }
            set
            {
                if (mTitle != value)
                {
                    var old = mTitle;
                    mTitle = value;
                    RaisePropertyChanged("Title", old, value);
                }
            }
        }

        #endregion Title
    }
}