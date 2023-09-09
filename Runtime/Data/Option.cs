using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Retrolight.Data {
    public static class Option {
        public static Option<T> Some<T>(T value) => new Option<T>(true, value);

        public static Option<T> None<T>() => new Option<T>(false, default);

        public static void Dispose<T>(this Option<T> self) where T : IDisposable {
            if (self.Enabled) self.Value.Dispose();
        }
    }

    [Serializable]
    public struct Option<T> {
        [SerializeField] private bool enabled;
        public bool Enabled => enabled;
        
        [SerializeField] private T value;
        public T Value => enabled ? value : 
            throw new InvalidOperationException($"Cannot access value of disabled {typeof(Option<T>)}");
        
        //public T UnsafeValue => Value;
    
        public Option(bool enabled, T value) {
            this.enabled = enabled;
            this.value = value;
        }

        [Pure]
        public Option<U> Map<U>([InstantHandle] Func<T, U> fn) => enabled ? Option.Some(fn(value)) : Option.None<U>();

        public void With([InstantHandle] Action<T> fn) { if (enabled) fn(value); }
        

        [Pure]
        public Option<U> Ap<U>([InstantHandle] Option<Func<T, U>> fn) => 
            enabled && fn.enabled ? Option.Some(fn.value(value)) : Option.None<U>();
    
        [Pure]
        public Option<U> Flatmap<U>([InstantHandle] Func<T, Option<U>> fn) => enabled ? fn(value) : Option.None<U>();
        
        [Pure]
        public T OrElse(T defaultValue) => enabled ? value : defaultValue;

    }
}