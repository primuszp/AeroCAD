using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Primusz.AeroCAD.View.Input
{
    public sealed class KeyboardShortcutService
    {
        private readonly List<KeyboardShortcutBinding> bindings = new List<KeyboardShortcutBinding>();

        public void Register(
            Key key,
            Func<bool> handler,
            ModifierKeys modifiers = ModifierKeys.None,
            bool allowWhenTextInputFocused = false,
            Func<KeyboardShortcutContext, bool> canHandle = null)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            bindings.Add(new KeyboardShortcutBinding(key, modifiers, allowWhenTextInputFocused, handler, canHandle));
        }

        public bool TryHandle(KeyEventArgs e, bool isTextInputFocused)
        {
            if (e == null)
                return false;

            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var modifiers = Keyboard.Modifiers;
            var context = new KeyboardShortcutContext(e, isTextInputFocused, modifiers);

            foreach (var binding in bindings)
            {
                if (!binding.Matches(key, modifiers))
                    continue;

                if (isTextInputFocused && !binding.AllowWhenTextInputFocused)
                    return false;

                if (binding.CanHandle != null && !binding.CanHandle(context))
                    return false;

                return binding.Handler();
            }

            return false;
        }

        public sealed class KeyboardShortcutContext
        {
            public KeyboardShortcutContext(KeyEventArgs keyEventArgs, bool isTextInputFocused, ModifierKeys modifiers)
            {
                KeyEventArgs = keyEventArgs;
                IsTextInputFocused = isTextInputFocused;
                Modifiers = modifiers;
            }

            public KeyEventArgs KeyEventArgs { get; }

            public bool IsTextInputFocused { get; }

            public ModifierKeys Modifiers { get; }
        }

        private sealed class KeyboardShortcutBinding
        {
            public KeyboardShortcutBinding(
                Key key,
                ModifierKeys modifiers,
                bool allowWhenTextInputFocused,
                Func<bool> handler,
                Func<KeyboardShortcutContext, bool> canHandle)
            {
                Key = key;
                Modifiers = modifiers;
                AllowWhenTextInputFocused = allowWhenTextInputFocused;
                Handler = handler;
                CanHandle = canHandle;
            }

            public Key Key { get; }

            public ModifierKeys Modifiers { get; }

            public bool AllowWhenTextInputFocused { get; }

            public Func<bool> Handler { get; }

            public Func<KeyboardShortcutContext, bool> CanHandle { get; }

            public bool Matches(Key key, ModifierKeys modifiers)
            {
                return key == Key && modifiers == Modifiers;
            }
        }
    }
}


