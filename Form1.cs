// 🔒USB SECURITY KEY
// Created by Justin Linwood Ross
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UsbSecurityKey
{
    public partial class Form1 : Form
    {
        // P/Invoke (Platform Invocation Services) allows us to call unmanaged code from managed code.
        // Here, we're importing the LockWorkStation function from the user32.dll library,
        // which is a standard Windows library for user interface tasks.
        [DllImport("user32.dll")]
        private static extern void LockWorkStation();

        // The icon that will live in the system tray (next to the clock).
        private readonly NotifyIcon _notifyIcon;
        // The core logic handler for checking the USB key's presence and validity.
        private readonly UsbAuthenticator _authenticator;
        // A boolean flag to keep track of the current authentication state.
        private bool _isAuthenticated;
        // A timer to periodically check for the key, acting as a fallback mechanism.
        private readonly Timer _checkTimer;

        public Form1()
        {
            InitializeComponent(); // This is required for Windows Forms designer support.

            // Configure the form to run as a background application.
            // We don't want a visible window, so we minimize it and hide it from the taskbar.
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Visible = false;

            // Initialize the component that handles the authentication logic.
            _authenticator = new UsbAuthenticator();
            // Perform an initial check as soon as the application starts.
            _isAuthenticated = _authenticator.Authenticate();

            // Create the system tray icon and its context menu.
            _notifyIcon = new NotifyIcon
            {
                // Use a default system icon. This will be updated based on auth status.
                Icon = System.Drawing.SystemIcons.Shield,
                Visible = true, // Make the icon visible in the tray.
                Text = "USB Security Key" // Tooltip text when hovering over the icon.
            };

            // Create the right-click menu for the tray icon.
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", null, OnExitClicked); // Add an "Exit" option.
            _notifyIcon.ContextMenuStrip = contextMenu;

            // Set up a timer as a secondary check mechanism.
            // This ensures that even if the Windows device change message fails, the system
            // will still lock within a few seconds of the key being removed.
            _checkTimer = new Timer
            {
                Interval = 5000 // The interval is in milliseconds, so this is 5 seconds.
            };
            _checkTimer.Tick += CheckTimer_Tick; // Assign the event handler for when the timer ticks.
            _checkTimer.Start(); // Start the timer.
        }

        /// <summary>
        /// This is the core logic method that checks the authentication status and locks the PC if necessary.
        /// </summary>
        private void CheckAuthentication()
        {
            // Store the previous authentication state before checking again.
            bool wasAuthenticated = _isAuthenticated;
            // Run the authentication check.
            _isAuthenticated = _authenticator.Authenticate();

            // Update the tray icon to reflect the new status.
            UpdateIcon();

            // This is the most critical part: if the user *was* authenticated, but now they *are not*,
            // it means the key was just removed. We must lock the workstation immediately.
            if (wasAuthenticated && !_isAuthenticated)
            {
                LockWorkStation();
            }
        }

        /// <summary>
        /// Updates the system tray icon and tooltip text based on the authentication status.
        /// </summary>
        private void UpdateIcon()
        {
            // If authenticated, show a blue "Information" icon. Otherwise, show a yellow "Warning" shield.
            _notifyIcon.Icon = _isAuthenticated
               ? System.Drawing.SystemIcons.Information
               : System.Drawing.SystemIcons.Warning;

            // Update the tooltip to be more descriptive.
            _notifyIcon.Text = _isAuthenticated
               ? "USB Security Key: Authenticated"
               : "USB Security Key: Key Not Found";
        }

        /// <summary>
        /// Event handler for the timer's Tick event.
        /// </summary>
        private void CheckTimer_Tick(object sender, EventArgs e)
        {
            // Every time the timer ticks (e.g., every 5 seconds), run the authentication check.
            CheckAuthentication();
        }

        /// <summary>
        /// Event handler for the "Exit" button in the tray icon's context menu.
        /// </summary>
        private void OnExitClicked(object sender, EventArgs e)
        {
            // Hide the icon from the tray to provide immediate feedback to the user.
            _notifyIcon.Visible = false;
            // Properly shut down the application.
            Application.Exit();
        }

        // --- Windows Message Handling for Device Changes ---
        // These constants are standard Windows message identifiers.
        private const int WM_DEVICECHANGE = 0x0219;         // Message sent when a hardware device's status changes.
        private const int DBT_DEVICEARRIVAL = 0x8000;      // A device has been inserted.
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // A device has been removed.

        /// <summary>
        /// Overrides the default window procedure (WndProc) to listen for system-level messages.
        /// This is more efficient than the timer because it reacts instantly to hardware changes.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            // Pass the message to the base class first.
            base.WndProc(ref m);

            // Check if the message is a device change notification.
            if (m.Msg == WM_DEVICECHANGE)
            {
                // Check if the specific type of change is a device being added or removed.
                if (m.WParam.ToInt32() == DBT_DEVICEARRIVAL || m.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE)
                {
                    // If a USB device was plugged in or unplugged, run our check immediately
                    // instead of waiting for the next timer tick.
                    CheckAuthentication();
                }
            }
        }

        /// <summary>
        /// Overrides the OnLoad event to ensure the form is hidden right away.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Although we set Visible = false in the constructor, this is an extra guarantee
            // that the form window will never flash on the screen.
            Hide();
        }
    }
}