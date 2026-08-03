using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;

namespace CardEditor.Commands
{
    public class OptimizedKeySequenceGesture : InputGesture
    {
        private readonly List<KeyGesturePart> _keySequence;
        private readonly TimeSpan _sequenceTimeout;
        private DateTime _lastKeyTime;
        private int _currentStep;

        // HashSet để check nhanh xem key có trong sequence không
        private readonly HashSet<Key> _keysInSequence;
        private readonly bool _requiresModifier;

        public OptimizedKeySequenceGesture(params KeyGesturePart[] sequence)
            : this(TimeSpan.FromSeconds(2), sequence) { }

        public OptimizedKeySequenceGesture(TimeSpan timeout, params KeyGesturePart[] sequence)
        {
            if (sequence == null || sequence.Length == 0)
                throw new ArgumentException("Sequence cannot be empty");

            _keySequence = new List<KeyGesturePart>(sequence);
            _sequenceTimeout = timeout;
            _currentStep = 0;
            _lastKeyTime = DateTime.MinValue;

            // Pre-compute optimization data
            _keysInSequence = new HashSet<Key>(sequence.Select(s => s.Key));
            _requiresModifier = sequence.Any(s => s.Modifiers != ModifierKeys.None);
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (!(inputEventArgs is KeyEventArgs keyArgs)) return false;

            // Fast path filters
            if (keyArgs.RoutedEvent != Keyboard.KeyDownEvent) return false;
            if (keyArgs.Handled) return false;

            var key = keyArgs.Key;

            // Skip system/modifier keys
            if (IsModifierKey(key)) return false;

            // SUPER FAST CHECK: Key không có trong sequence → skip ngay
            if (!_keysInSequence.Contains(key))
            {
                // Nếu đang ở giữa sequence, reset về 0
                if (_currentStep > 0)
                {
                    _currentStep = 0;
                }
                return false;
            }

            // Nếu sequence yêu cầu modifier nhưng không có modifier nào được nhấn
            if (_requiresModifier && Keyboard.Modifiers == ModifierKeys.None && _currentStep == 0)
            {
                return false;
            }

            // Check timeout
            if (DateTime.Now - _lastKeyTime > _sequenceTimeout)
            {
                _currentStep = 0;
            }

            // Main matching logic
            var currentGesture = _keySequence[_currentStep];

            if (key == currentGesture.Key &&
                Keyboard.Modifiers == currentGesture.Modifiers)
            {
                _lastKeyTime = DateTime.Now;
                _currentStep++;

                if (_currentStep >= _keySequence.Count)
                {
                    _currentStep = 0;
                    return true;
                }
            }
            else
            {
                _currentStep = 0;
            }

            return false;
        }

        private static bool IsModifierKey(Key key)
        {
            return key == Key.System ||
                   key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LWin || key == Key.RWin;
        }

        public string GetDisplayString()
        {
            return string.Join(", ", _keySequence.Select(k => k.GetDisplayString()));
        }
    }
}
