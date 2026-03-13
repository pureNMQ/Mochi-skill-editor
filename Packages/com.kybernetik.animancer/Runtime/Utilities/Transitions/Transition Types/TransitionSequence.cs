// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if ! UNITY_EDITOR
#pragma warning disable CS0618 // Type or member is obsolete (for Animancer Events in Animancer Lite).
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// <inheritdoc/>
    /// <summary>A group of transitions which play one after the other.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/TransitionSequence
    /// 
    [Serializable]
    public class TransitionSequence : Transition<SequenceState>,
        IAnimationClipCollection,
        ICopyable<TransitionSequence>
    {
        /************************************************************************************************************************/

        [DrawAfterEvents]
        [SerializeReference]
        [Tooltip("The transitions to play in this sequence.")]
        private ITransition[] _Transitions = Array.Empty<ITransition>();

        /// <summary>[<see cref="SerializeField"/>] The transitions to play in this sequence.</summary>
        public ref ITransition[] Transitions
            => ref _Transitions;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override SequenceState CreateState()
        {
            var state = new SequenceState();
            state.Set(_Transitions);
            return state;
        }

        /************************************************************************************************************************/

        /// <summary>Is everything in this sequence valid?</summary>
        public override bool IsValid
        {
            get
            {
                for (int i = 0; i < _Transitions.Length; i++)
                    if (!_Transitions[i].IsValid())
                        return false;

                return true;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sequences don't loop.</summary>
        /// <remarks>
        /// If the last state in the sequence is set to loop it will do so,
        /// but the rest of the sequence won't replay automatically.
        /// </remarks>
        public override bool IsLooping
            => false;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override float MaximumLength
        {
            get
            {
                var value = 0f;

                for (int i = 0; i < _Transitions.Length; i++)
                {
                    var transition = _Transitions[i];
                    if (transition.IsValid())
                        value += transition.MaximumLength;
                }

                return value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Adds the <see cref="ClipTransition.Clip"/> of everything in this sequence to the collection.</summary>
        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            for (int i = 0; i < _Transitions.Length; i++)
                clips.GatherFromSource(_Transitions[i]);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override Transition<SequenceState> Clone(CloneContext context)
            => new TransitionSequence();

        /// <inheritdoc/>
        public sealed override void CopyFrom(Transition<SequenceState> copyFrom, CloneContext context)
            => this.CopyFromBase(copyFrom, context);

        /// <inheritdoc/>
        public virtual void CopyFrom(TransitionSequence copyFrom, CloneContext context)
        {
            base.CopyFrom(copyFrom, context);

            AnimancerUtilities.CopyExactArray(copyFrom._Transitions, ref _Transitions);
        }

        /************************************************************************************************************************/
    }
}

