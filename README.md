# Using WindowsEx.Notification

This module is indended to mimic Windows 10 toast notifications without dependency
on actual Windows 10 feature.

WPF was chosen instead of WinRT so for actual notification window could be displayed
a basic application message loop must be running.

Existing message loop can be used in both WPF and WinForms applications, however
it's not necessary, the module can create its own applicaiton object.

It's however important than only one such module can be run within one application.

When not using in a console app - ALWAYS USE ASYNC METHODS. They are the most
lightweight and stable.

Notifications could be displayed ONLY FROM STA THREADS! Main GUI application thread
is usually a STA thread. Calling from other threads will cause an exception.