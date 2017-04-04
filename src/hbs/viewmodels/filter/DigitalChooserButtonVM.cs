// DigitalChooserButtonVM.cs
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

using System.Linq;
using picibird.hbs.ldu;
using picibird.shelfhub;
using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.filter
{
    public class DigitalChooserButtonVM : ChooserButtonViewModel
    {
        public DigitalChooserButtonVM(ViewStyle style)
            : base("digital", style)
        {
        }

        public override void OnFilterChanged(Facet filterCategory)
        {
            if (filterCategory.Key == "medium" && filterCategory.Values.Count > 0)
            {
                var filters = filterCategory.Values.Where(f =>
                {
                    switch (f.Value)
                    {
                        case "ebook":
                            return true;
                        case "ejournal":
                            return true;
                        case "article":
                            return true;
                        case "bookchapter":
                            return true;
                        case "bookreview":
                            return true;
                    }
                    return false;
                });
                Frequency = 0;
                foreach (var filter in filters)
                {
                    Frequency += filter.Count.Value;
                }
            }
        }
    }
}