// OpenedBook3DViewModel.cs
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

using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.book3D
{
    public class OpenedBook3DViewModel : Book3DViewModel
    {
        public override void CreatBookSides()
        {
            base.CreatBookSides();
            Spine.Style = new ViewStyle("SpineStyle");
            BackCover.Style = new ViewStyle("BackCoverStyle");
        }

        protected override void OnOpenedProgressChanged(double progress)
        {
            base.OnOpenedProgressChanged(progress);
            var openProgress = 1 - progress;
            Spine.ContentOpacity = openProgress;
        }
    }
}