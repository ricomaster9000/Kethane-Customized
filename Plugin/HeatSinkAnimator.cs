﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kethane
{
    public class HeatSinkAnimator : PartModule
    {
        [KSPField(isPersistant = false)]
        public string HeatAnimation;

        [KSPField(isPersistant = false)]
        public string OpenAnimation;

        [KSPField(isPersistant = false)]
        public float OpenTemperature;

        private AnimationState[] heatAnimationStates;
        private AnimationState[] openAnimationStates;

        public override void OnStart(PartModule.StartState state)
        {
            openAnimationStates = SetUpAnimation(OpenAnimation);
            heatAnimationStates = SetUpAnimation(HeatAnimation);
        }

        private AnimationState[] SetUpAnimation(string animationName)
        {
            var states = new List<AnimationState>();
            foreach (var animation in this.part.FindModelAnimators(animationName))
            {
                var animationState = animation[animationName];
                animationState.speed = 0;
                animationState.enabled = true;
                animationState.wrapMode = WrapMode.ClampForever;
                animation.Blend(animationName);
                states.Add(animationState);
            }
            return states.ToArray();
        }

        public override void OnUpdate()
        {
            var draperPoint = 525;
            var heatFraction = (this.part.temperature - draperPoint) / (this.part.maxTemp - draperPoint);
            
            foreach (var state in heatAnimationStates)
            {
                state.normalizedTime = heatFraction;
            }

            var shouldOpen = this.part.temperature >= OpenTemperature;

            foreach (var state in openAnimationStates)
            {
                state.normalizedTime = Mathf.Clamp01(state.normalizedTime);
            }

            var openState = openAnimationStates.First();
            var isMoving = (openState.normalizedTime > 0) && (openState.normalizedTime < 1);
            if (!isMoving)
            {
                var isOpen = openState.normalizedTime == 1;
                if (isOpen != shouldOpen)
                {
                    foreach (var state in openAnimationStates)
                    {
                        state.speed = shouldOpen ? 1 : -1;
                    }
                }
            }
        }
    }
}
