// SwipeBehaviour.cs
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
using System.Collections.Generic;
using System.Linq;
using picibird.hbs.config;
using picibits.app.transition;
using picibits.core;
using picibits.core.behaviour;
using picibits.core.export.instances;
using picibits.core.mvvm;
using picibits.core.pointer;

namespace picibird.hbs.behaviours
{
    public abstract class SwipeBehaviour : Behaviour<ViewModel>
    {
        public static Direction Direction = Direction.Center;

        private double cummulatedAbsoluteX;
        private double cummulatedAbsoluteY;
        private double cummulatedX;
        private readonly List<double> DeltaX = new List<double>();

        private bool isDown;
        private bool isSwiping;
        private Point lastPoint;

        public override void OnAttached(ViewModel attached)
        {
            Reset();
            attached.Pointing.IsEnabled = true;
            attached.Pointing.Event += OnPointingEvent;
        }

        public override void OnDettached(ViewModel dettached)
        {
            dettached.Pointing.IsEnabled = false;
            dettached.Pointing.Event -= OnPointingEvent;
        }

        protected virtual void OnPointingEvent(object sender, PointerEventArgs e)
        {
            if (!e.IsPrimary)
                return;

            //down
            if (e.Type == PointerEventType.DOWN)
            {
                isDown = true;
                e.Pointer.Capture(Attached.View);
                lastPoint = e.Pointer.GetPosition(Attached.View);
            }
            //up
            if (isDown && e.Type == PointerEventType.UP)
            {
                if (isSwiping)
                {
                    DeltaX.Reverse();
                    var lastDeltaX = DeltaX.FirstOrDefault(dX => Math.Abs(dX) > 1);

                    double lastCumX = 0;
                    for (int i = 0; i < DeltaX.Count; i++)
                    {
                        lastCumX += DeltaX[i];
                        if (Math.Abs(lastCumX) > 30)
                            break;
                    }
                    Direction = lastCumX > 0 ? Direction.Right : Direction.Left;


                    Pici.Log.info(typeof(SwipeBehaviour), "deltaX={0}; cumX={1}", lastDeltaX, cummulatedX);


                    OnSwipeCompleted(lastDeltaX, cummulatedX);
                }
                Reset(e);
            }
            //move
            if (isDown && e.Type == PointerEventType.MOVE)
            {
                var position = e.Pointer.GetPosition(Attached.View);
                var deltaX = position.X - lastPoint.X;
                var deltaY = position.Y - lastPoint.Y;
                DeltaX.Add(deltaX);
                cummulatedAbsoluteX += Math.Abs(deltaX);
                cummulatedAbsoluteY += Math.Abs(deltaY);
                lastPoint = position;
                if (isSwiping)
                {
                    cummulatedX += deltaX;
                    OnSwipeDelta(deltaX, cummulatedX);
                }
                else if (cummulatedAbsoluteX > Config.Pointer.TapMoveThreshold &&
                         cummulatedAbsoluteY < Config.Pointer.TapMoveThreshold)
                {
                    isSwiping = true;
                    cummulatedX += deltaX;
                    OnSwipeStarting(deltaX, cummulatedX);
                }
            }

            //lost
            if (isDown && e.Type == PointerEventType.LEAVE)
            {
                OnSwipeAbort();
                Reset(e);
            }
        }

        public virtual void Reset(PointerEventArgs e = null)
        {
            isDown = false;
            isSwiping = false;
            lastPoint = new Point();
            DeltaX.Clear();
            DeltaX.Add(0);
            cummulatedX = 0;
            cummulatedAbsoluteX = 0;
            cummulatedAbsoluteY = 0;
            if (e != null)
                e.Pointer.ReleaseCapture(Attached.View);
        }


        public abstract void OnSwipeStarting(double deltaX, double cummulatedX);
        public abstract void OnSwipeDelta(double deltaX, double cummulatedX);
        public abstract void OnSwipeCompleted(double deltaX, double cummulatedX);
        public abstract void OnSwipeAbort();
    }
}