// ShelfSwipeTransition.cs
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
using picibird.hbs.behaviours;
using picibird.hbs.viewmodels;
using picibits.app.animation;
using picibits.app.transition;
using picibits.core.math;

namespace picibird.hbs.transition
{
    public class ShelfSwipeTransition : TransitionBase
    {
        private double StartIndex;

        private Direction SwipeDirection;

        public ShelfSwipeTransition(ShelfViewModel shelf)
        {
            Shelf = shelf;
        }

        public ShelfViewModel Shelf { get; }

        public override void OnTransitionStarting()
        {
            StartIndex = Shelf.SelectedIndex;
        }

        public override void OnTransitionProgress(double progress)
        {
            var nextIndex = StartIndex - progress;
            nextIndex = MathX.Clamp(nextIndex, -1, HBS.Search.Callback.MaxPageIndex + 1);

            SwipeDirection = nextIndex > Shelf.SelectedIndex ? Direction.Right : Direction.Left;
            Shelf.SelectedIndex = nextIndex;
        }

        public override void OnTranistionCompleted()
        {
            var index = Shelf.SelectedIndex;
            if (SwipeBehaviour.Direction == Direction.Left)
                index = Math.Ceiling(index);
            if (SwipeBehaviour.Direction == Direction.Right)
                index = Math.Floor(index);
            index = MathX.Clamp(index, 0, HBS.Search.Callback.MaxPageIndex);
            Shelf.AnimateSelectedIndexTo(index, 0.5, AnimationTransitions.CircEaseOut);
        }
    }
}