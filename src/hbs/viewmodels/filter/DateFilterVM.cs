// DateFilterVM.cs
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

using picibird.hbs.ldu;

namespace picibird.hbs.viewmodels.filter
{
    public class DateFilterVM : ListFilterViewModel
    {
        public DateFilterVM(FilterCategory category)
            : base(category)
        {
            FilterSelectionViewModel.CategoryAdded += OnCategoryAdded;
            OnCategoryAdded(null);
        }

        private void OnCategoryAdded(Filter sender)
        {
            FilterSelectionViewModel.Items.Sort(f =>
            {
                var year = 0;
                var yearString = (f as FilterSelectionItemViewModel).Filter.Id;
                int.TryParse(yearString, out year);
                return -year;
            });
        }
    }
}