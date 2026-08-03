using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;

namespace CardEditor.Commands
{
    public class KeySequenceGesture : InputGesture
    {
        private readonly List<KeyGesturePart> _keySequence;
        private readonly TimeSpan _sequenceTimeout;
        private DateTime _lastKeyTime;
        private int _currentStep;

        public KeySequenceGesture(params KeyGesturePart[] sequence) : this(TimeSpan.FromSeconds(2), sequence) { }

        public KeySequenceGesture(TimeSpan timeout, params KeyGesturePart[] sequence)
        {
            if (sequence == null || sequence.Length == 0) throw new ArgumentException("Sequence cannot be empty");

            _keySequence = new List<KeyGesturePart>(sequence);
            _sequenceTimeout = timeout;
            _currentStep = 0;
            _lastKeyTime = DateTime.MinValue;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (!(inputEventArgs is KeyEventArgs keyArgs)) return false;

            // Check if sequence has timed out
            if (DateTime.Now - _lastKeyTime > _sequenceTimeout)
            {
                _currentStep = 0;
            }

            // Get current expected key
            var currentGesture = _keySequence[_currentStep];

            // Check if current key matches
            if (keyArgs.Key == currentGesture.Key && Keyboard.Modifiers == currentGesture.Modifiers)
            {
                _lastKeyTime = DateTime.Now;
                _currentStep++;

                // Check if sequence is complete
                if (_currentStep >= _keySequence.Count)
                {
                    _currentStep = 0;
                    return true;
                }
            }
            else
            {
                // Wrong key pressed, reset sequence
                _currentStep = 0;
            }
            return false;
        }

        public string GetDisplayString()
        {
            return string.Join(", ", _keySequence.Select(k => k.GetDisplayString()));
        }
    }
    public class KeyGesturePart
    {
        public Key Key { get; }
        public ModifierKeys Modifiers { get; }

        public KeyGesturePart(Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public string GetDisplayString()
        {
            var parts = new List<string>();

            if ((Modifiers & ModifierKeys.Control) != 0)
                parts.Add("Ctrl");
            if ((Modifiers & ModifierKeys.Alt) != 0)
                parts.Add("Alt");
            if ((Modifiers & ModifierKeys.Shift) != 0)
                parts.Add("Shift");
            if ((Modifiers & ModifierKeys.Windows) != 0)
                parts.Add("Win");

            parts.Add(Key.ToString());

            return string.Join("+", parts);
        }
    }
    public class KeySequenceBinding : InputBinding
    {
        public KeySequenceBinding() { }

        public KeySequenceBinding(ICommand command, InputGesture gesture)
        {
            Command = command;
            Gesture = gesture;
        }
    }
}
