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
using picibird.shelfhub;
using picibits.core.collection;

namespace picibird.hbs.viewmodels.availability
{
    public class RessourcesVM : ViewModel
    {
        private EaseObject opacityAnimation;


        public RessourcesVM()
        {
            Title = "Ressources";
            Style = new ViewStyle("RessourcesViewStyle");
            Opacity = 0;
            Has = false;
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
                ToggleItemsFilledOpacityAnimation(false);
            }
            if (newModel != null && newModel is Hit)
            {
                var hit = newModel as Hit;
                UpdateAvailability(hit);
            }
        }


        public async void UpdateAvailability(Hit hit)
        {
            if (hit != null)
            {
                hit.RessourceLinks.RemoveAll();
                try
                {
                    var shelfhub = ShelfhubSearch.createShelfhubClient();
                    ShelfhubItem shelfhubItem = hit.shelfhubItem as ShelfhubItem;
                    var ressourcesParams = new RessourcesParams()
                    {
                        Item = shelfhubItem,
                        Shelfhub = ShelfhubSearch.PROFILE_ACTIVE,
                        Locale = Pici.Resources.CultureInfo.Name
                    };
                    RessourcesResponse resp = await shelfhub.RessourcesAsync(ressourcesParams);
                    Has = resp.Links.Count > 0;
                    if (Has)
                    {
                        foreach (var link in resp.Links)
                        {
                            var l = new picibird.hbs.ldu.Link("ressource", link.Url, link.Title, "", hit, link.Note);
                            hit.RessourceLinks.Add(l);
                        }
                        ToggleItemsFilledOpacityAnimation(true);
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