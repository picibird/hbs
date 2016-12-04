// BackCoverViewModel.cs
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

using picibird.hbs.viewmodels.availability;
using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.book
{
    public class BackCoverViewModel : ViewModel
    {
        protected override void OnModelChanged(Model oldModel, Model newModel)
        {
            base.OnModelChanged(oldModel, newModel);
            Availabilities.Model = newModel;
        }

        #region Availabilities

        private AvailabilityItemsVM mAvailabilities;

        public AvailabilityItemsVM Availabilities
        {
            get
            {
                if (mAvailabilities == null)
                    mAvailabilities = new AvailabilityItemsVM();
                return mAvailabilities;
            }
        }

        #endregion Availabilities
    }
}