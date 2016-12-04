// ShelfSwipeBehaviour.cs
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
using picibird.hbs.transition;
using picibird.hbs.viewmodels;
using picibits.app.animation;

namespace picibird.hbs.behaviours
{
    public class ShelfSwipeBehaviour : SwipeBehaviour
    {
        #region AttachedShelf

        public ShelfViewModel AttachedShelf
        {
            get { return Attached as ShelfViewModel; }
        }

        #endregion AttachedShelf

        public ShelfSwipeTransition SwipeTransition { get; private set; }

        public override void OnSwipeStarting(double deltaX, double cummulatedX)
        {
            SwipeTransition = new ShelfSwipeTransition(AttachedShelf);
            SwipeTransition.OnTransitionStarting();
        }

        public override void OnSwipeDelta(double deltaX, double cummulatedX)
        {
            var progress = cummulatedX/Attached.ActualSize.Width;
            SwipeTransition.Progress = progress;
        }

        public override void OnSwipeCompleted(double deltaX, double cummulatedX)
        {
            SwipeTransition.OnTranistionCompleted();
        }

        public override void OnSwipeAbort()
        {
            AttachedShelf.AnimateSelectedIndexTo(Math.Round(AttachedShelf.SelectedIndex), 0.5,
                AnimationTransitions.CircEaseOut);
        }
    }
}