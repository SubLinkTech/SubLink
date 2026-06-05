using System;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data;

internal sealed class JoystickErrorEventArgs : EventArgs {
    public Exception Exception { get; set; } = new();

    public JoystickErrorEventArgs() { }

    public JoystickErrorEventArgs(Exception exception) {
        Exception = exception;
    }
}

public class JoystickEventArgs<T> : EventArgs where T : new() {
    public T Data { get; set; } = new T();
}
