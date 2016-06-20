using System;
using System.Windows;

namespace KiepVisitorRegistration
{
    public partial class KeyboardHookTest : Window
    {
        public KeyboardHookTest()
        {
            InitializeComponent();

            LowLevelKeyboardHook.Instance.KeyboardHookEvent += new LowLevelKeyboardHook.KeyboardHookEventHandler(Instance_KeyboardHookEvent);
        }

        void Instance_KeyboardHookEvent(int keycode)
        {
            tbTest.Text += DateTime.Now.ToLongTimeString() + " - ";
            tbTest.Text += keycode;
            tbTest.Text += "\r\n";
            tbTest.ScrollToEnd();
        }
    }
}
