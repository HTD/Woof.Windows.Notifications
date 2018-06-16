using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace Woof.WindowsEx.Notifications {

    /// <summary>
    /// Custom WPF styled modern notification window.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public sealed partial class NotificationWindow : Window, IDisposable {

        public NotificationWindow() {
            InitializeComponent();
            Loaded += async (s, e) => {
                await AppearAsync();
                await Task.Delay((int)(1000 * AutoDismiss));
                if (!_IsDismissing) await DisappearAsync();
            };
            Dismiss.Click += async (s, e) => {
                if (!_IsDismissing) await DisappearAsync();
            };
        }

        #region Properties

        /// <summary>
        /// Gets or sets one of notification contexts.
        /// </summary>
        public Notification.Contexts Context {
            get => (Notification.Contexts)GetValue(ContextProperty);
            set => SetValue(ContextProperty, value);
        }

        /// <summary>
        /// Gets or sets the text displayed in the header.
        /// </summary>
        public string HeaderText {
            get => (string)GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        public string MessageText {
            get => (string)GetValue(MessageTextProperty);
            set => SetValue(MessageTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the logo icon.
        /// </summary>
        public bool ShowLogo {
            get => Logo.Visibility == Visibility.Visible;
            set => Logo.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Gets or sets the visibility of dismiss button.
        /// </summary>
        public bool ShowButton {
            get => Dismiss.Visibility == Visibility.Visible;
            set {
                if (!value && _AutoDismiss < 1) _AutoDismiss = 2;
                Dismiss.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gets or sets auto dismiss time in seconds.
        /// </summary>
        public double AutoDismiss {
            get => _AutoDismiss; set {
                if (value < 1) ShowButton = true;
                _AutoDismiss = value;
            }
        }

        /// <summary>
        /// Gets or sets horizontal and vertical space between the screen edge and the notification window, default 10px.
        /// </summary>
        public Point Space { get; set; } = new Point(10, 10);

        #region Properties registration

        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register(
            "Context",
            typeof(Notification.Contexts),
            typeof(NotificationWindow)
        );

        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
            "HeaderText",
            typeof(string),
            typeof(NotificationWindow)
        );

        public static readonly DependencyProperty MessageTextProperty = DependencyProperty.Register(
            "MessageText",
            typeof(string),
            typeof(NotificationWindow)
        );

        double _AutoDismiss;
        bool _IsDismissing;

        #endregion

        #endregion

        /// <summary>
        /// Provides a reset event to wait until the window is dismissed.
        /// </summary>
        internal ManualResetEventSlim ResetEvent = new ManualResetEventSlim(false);

        Point PositionRightBottom {
            get {
                var vd = new System.Drawing.Rectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
                foreach (var screen in Screen.AllScreens) vd = System.Drawing.Rectangle.Union(vd, screen.WorkingArea);
                return new Point(vd.Right - Width - Space.X, vd.Bottom - Height - Space.Y);
            }
        }

        Point PositionOffscreen {
            get {
                var p = PositionRightBottom;
                //return new Point(p.X, p.Y + Height + (Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height));
                return new Point(p.X + Width, p.Y);
            }
        }

        ManualResetEventSlim Slide(Point here, Point target, double time) {
            var duration = new Duration(TimeSpan.FromMilliseconds(time));
            var xAnimation = new DoubleAnimation(here.X, target.X, duration);
            var yAnimation = new DoubleAnimation(here.Y, target.Y, duration);
            var done = new ManualResetEventSlim(false);
            xAnimation.Completed += (s, e) => done.Set();
            BeginAnimation(LeftProperty, xAnimation);
            BeginAnimation(TopProperty, yAnimation);
            return done;
        }

        async Task AppearAsync() {
            var done = Slide(PositionOffscreen, PositionRightBottom, 160);
            await Task.Run(() => { done.Wait(); });
            _IsDismissing = false;
        }

        async Task DisappearAsync() {
            _IsDismissing = true;
            var done = Slide(PositionRightBottom, PositionOffscreen, 320);
            await Task.Run(() => { done.Wait(); });
            Close();
            ResetEvent.Set();
        }

        public void Dispose() => ((IDisposable)ResetEvent).Dispose();

    }

}