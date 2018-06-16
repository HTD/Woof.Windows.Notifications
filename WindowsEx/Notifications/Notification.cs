using System;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Woof.WindowsEx.Notifications {

    /// <summary>
    /// Windows 10 toast notifications.
    /// </summary>
    public sealed class Notification : IDisposable {

        /// <summary>
        /// Available context of the notification.
        /// </summary>
        public enum Contexts { Error, Message, Success, Warning }

        /// <summary>
        /// Common header to show with notifications.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets notification window logo. A path relative to project root. The resource should be compiled as resource.
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// Gets or sets the time in seconds used to automatically dismiss the notification. Default 5 seconds.
        /// </summary>
        public double AutoDismiss { get; set; } = 5;

        /// <summary>
        /// Gets or sets a flag if OK button should be displayed with notifications. Default true.
        /// </summary>
        public bool ShowButton { get; set; } = true;

        /// <summary>
        /// Displays "toast" notification.
        /// </summary>
        /// <param name="header">Application title to display in first line.</param>
        /// <param name="logo">Optional logo icon to display. Provide path relative to project root, the file should be linked as resource.</param>
        public Notification(string header, string logo = null) {
            Header = header;
            Logo = logo;
        }

        /// <summary>
        /// Displays notification in synchronous application context, the main thread is unblocked when the window is dismissed.
        /// <para>MUST BE CALLED FROM STA THREAD!</para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public void Notify(Contexts context, string message) {
            var app = new Application() { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            app.Startup += async (s, e) => {
                await NotifyAsync(context, message);
                app.Shutdown();
            };
            app.Run();
        }

        /// <summary>
        /// Displays notification in asynchronous context, the task completes as the window is dismissed.
        /// <para>MUST BE CALLED FROM STA THREAD!</para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>

        public async Task NotifyAsync(Contexts context, string message) {
            StartNotify(context, message);
            await Task.Run(() => ResetEvent.Wait());
            ResetEvent = null;
        }

#pragma warning disable CS4014

        /// <summary>
        /// Displays notification in asynchronous context, the task is not awaided.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public void NotifyAndForget(Contexts context, string message) => NotifyAsync(context, message);

#pragma warning restore CS4014

        #region Static methods

        public static void Configure(string header, string logo = null) => _Instance = new Notification(header, logo);

        public static void Show(Contexts context, string message) => _Instance?.Notify(context, message);

        public static async Task ShowAsync(Contexts context, string message) => await _Instance?.NotifyAsync(context, message);

        public static void ShowAndForget(Contexts context, string message) => _Instance?.NotifyAndForget(context, message);

        #endregion

        #region Non-public

        /// <summary>
        /// Displays notification.
        /// </summary>
        /// <param name="context">Notification context, as message, warning or error.</param>
        /// <param name="message">Message to display.</param>
        private void StartNotify(Contexts context, string message) {
            var window = new NotificationWindow { HeaderText = Header, MessageText = message, AutoDismiss = AutoDismiss, ShowButton = ShowButton };
            if (Logo != null) {
                var logo = new BitmapImage();
                var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                logo.BeginInit();
                logo.UriSource = new Uri($"/{assemblyName};component/{Logo}", UriKind.Relative);
                logo.EndInit();
                window.Logo.Source = logo;
                window.ShowLogo = true;
            }
            else {
                window.ShowLogo = false;
            }
            window.Context = context;
            ResetEvent = window.ResetEvent;
            window.Show();
            SystemSounds.Asterisk.Play();
        }

        public void Dispose() => ResetEvent?.Dispose();

        ManualResetEventSlim ResetEvent;
        static Notification _Instance;

        #endregion

    }

}