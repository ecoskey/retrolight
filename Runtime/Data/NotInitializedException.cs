using System;

namespace Retrolight.Data {
    public class NotInitializedException : InvalidOperationException {
        public NotInitializedException(string message) : base(message) { }
        
        public static NotInitializedException DefaultMsg(string className, string initMethod = "Init") => 
            new NotInitializedException(@$"
                {className} is used before it has been initialized.
                Make sure {className}.{initMethod}() has been called.
            ");
    }
}