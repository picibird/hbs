// FilterSwipeBehaviour.cs
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
using picibits.app.animation;
using picibits.core.behaviour;
using picibits.core.export.instances;
using picibits.core.math;
using picibits.core.mvvm;
using picibits.core.pointer;
using Interpolation = picibits.app.animation.interpolation.Interpolation;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs.viewmodels.filter.behaviour
{
    public class FilterSwipeBehaviour : Behaviour<ViewModel>
    {
        private double cummulatedX;
        private double cummulatedY;

        private readonly List<double> DeltaX = new List<double>();

        private bool isDown;
        private Point lastPoint;

        public bool IsEnabled { get; private set; }

        private FilterContainerViewModel FVCM
        {
            get { return Attached as FilterContainerViewModel; }
        }

        public override void OnAttached(ViewModel attached)
        {
            var fvcm = attached as FilterContainerViewModel;
            fvcm.Chooser.PropertyChanged += OnChooserPropertyChanged;
            //fvcm.PropertyChanged += OnAttachedPropertyChanged;
        }

        public override void OnDettached(ViewModel dettached)
        {
            var fvcm = dettached as FilterContainerViewModel;
            fvcm.Chooser.PropertyChanged -= OnChooserPropertyChanged;
            //fvcm.PropertyChanged -= OnAttachedPropertyChanged;
        }

        private void OnAttachedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public void OnChooserPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (FVCM.Chooser.VisualState != FilterChooserStates.CLOSED)
            {
                if (!IsEnabled)
                {
                    IsEnabled = true;
                    FVCM.Pointing.Event += OnPointingEvent;
                }
            }
            else
            {
                if (IsEnabled)
                {
                    IsEnabled = false;
                    FVCM.Pointing.Event -= OnPointingEvent;
                }
            }
        }

        private void OnPointingEvent(object sender, PointerEventArgs e)
        {
            if (!e.IsPrimary)
                return;

            if (e.Type == PointerEventType.DOWN)
            {
                cummulatedX = 0;
                cummulatedY = 0;
                var captured = e.Pointer.Capture(Attached.View);
                lastPoint = e.Pointer.GetPosition(Attached.View);
                isDown = true;
                DeltaX.Clear();
                DeltaX.Add(0);
            }
            if (e.Type == PointerEventType.MOVE && isDown)
            {
                var position = e.Pointer.GetPosition(Attached.View);
                var deltaX = position.X - lastPoint.X;
                var deltaY = position.Y - lastPoint.Y;
                DeltaX.Insert(0, deltaX);
                cummulatedX += deltaX;
                cummulatedY += deltaY;
                lastPoint = position;
                if (Math.Abs(cummulatedX) > Config.Pointer.TapMoveThreshold * 1.5 &&
                    Math.Abs(cummulatedY) < Config.Pointer.TapMoveThreshold * 1.5)
                {
                    OnSwipeXDelta(deltaX);
                    OnSwipeCummulatedXDelta(cummulatedX);
                }
            }
            if (e.Type == PointerEventType.UP && isDown)
            {
                if (Math.Abs(cummulatedX) > Config.Pointer.TapMoveThreshold * 1.5)
                {
                    var lastDeltaX = DeltaX.FirstOrDefault(dX => dX != 0);
                    var lastDeltaDirection = Math.Sign(lastDeltaX);
                    var cummulatedDirection = Math.Sign(cummulatedX);
                    var swipeOut = lastDeltaDirection == cummulatedDirection;
                    if (swipeOut && Math.Abs(cummulatedY) < Config.Pointer.TapMoveThreshold * 1.5)
                    {
                        var m = GetTransform(cummulatedDirection);
                        var ani = FinishSwipeWithAnimation(m, 0);
                        ani.Complete += (a, p) => { FVCM.VisualState = FilterContainerVisualStates.DISCARDED; };
                    }
                    else
                        ResetWithAnimation();
                }
                isDown = false;
                var captureReleased = e.Pointer.ReleaseCapture(Attached.View);
            }
            if (e.Type != PointerEventType.DOWN && e.Type != PointerEventType.UP && e.Type != PointerEventType.MOVE)
            {
                if (isDown)
                {
                    ResetWithAnimation();
                }
            }
        }

        private void ResetWithAnimation()
        {
            isDown = false;
            var m = GetTransform(0);
            FinishSwipeWithAnimation(m, 1, false);
        }

        public void FinishSwipeWithAnimation()
        {
            var m = GetTransform(1);
            FinishSwipeWithAnimation(m, 0);
        }

        private EaseObject FinishSwipeWithAnimation(IMatrix m, double opacity, bool discardOnFinish = true)
        {
            var vm = GetTransformed();
            var startOpacity = vm.Opacity;
            var ease = AnimationTransitions.CircEaseOut;
            var easeObject = ArtefactAnimator.AddEase(vm, new[] { "TransformMatrix" }, new object[] { m }, 0.5, ease);
            easeObject.Update +=
                (s, progress) =>
                {
                    vm.Opacity = (double)Interpolation.Double.Interpolate(startOpacity, opacity, progress);
                };
            if (discardOnFinish)
                easeObject.Complete += (a, p) => { FVCM.VisualState = FilterContainerVisualStates.DISCARDED; };
            return easeObject;
        }

        private void OnSwipeXDelta(double deltaX)
        {
            OnSwipeXDeltaRelavtive(deltaX / Attached.ActualSize.Width);
        }

        private void OnSwipeXDeltaRelavtive(double deltaXRel)
        {
        }

        private void OnSwipeCummulatedXDelta(double cummulatedDeltaX)
        {
            OnSwipeCummulatedXDeltaRelative(cummulatedDeltaX / Attached.ActualSize.Width);
        }

        private void OnSwipeCummulatedXDeltaRelative(double cummulatedDeltaX)
        {
            var transform = GetTransform(cummulatedDeltaX);
            var transformed = GetTransformed();
            // TODO: in some situations the transformed object can be null, why i don't know
            transformed.TransformMatrix = transform;
            transformed.Opacity = GetOpacity(cummulatedDeltaX);
        }

        private double GetOpacity(double cummulatedDeltaX)
        {
            return 1 - Math.Abs(cummulatedDeltaX);
        }

        private IMatrix GetTransform(double cummulatedDeltaX)
        {
            var f = MxM.Identity;
            var translateX = Math.Sign(cummulatedDeltaX) * FVCM.ActualSize.Width;
            var t = MxM.Create(translateX, 0);
            var p = Math.Abs(cummulatedDeltaX);
            p = MathX.Clamp(p, 0, 1);
            return MxM.Progress(ref f, ref t, p);
        }

        private ViewModel GetTransformed()
        {
            if (FVCM.Chooser.VisualState == FilterChooserStates.OPENED)
                return FVCM.Chooser;
            return FVCM.Filter;
        }
    }
}