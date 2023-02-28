using System;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für PersonenAnzeigen.xaml
    /// </summary>
    public partial class PersonenAnzeigen : Page
    {/*
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

        */
        public PersonenAnzeigen()
        {
            InitializeComponent();

            txt_NameSuchen.Focus();

            Uri u = new Uri(Globals.projectDirectory + "/TrainingFaces");
            explorer.Source = u;
        }

        /*
        private void Window_Loaded(object sender, RoutedEventArgs e) 
        {
            int x = 600;
            int y = 500;

            var xpos = (int)App.Current.MainWindow.Left + x;
            var ypos = (int)App.Current.MainWindow.Top + y;

            int current_x = System.Windows.Forms.Cursor.Position.X;
            int current_y = System.Windows.Forms.Cursor.Position.Y;

            SetCursorPos(xpos, ypos);

            mouse_event(MOUSEEVENTF_RIGHTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, xpos, ypos, 0, 0);

            SetCursorPos(xpos + 10, ypos + 10);

            mouse_event(MOUSEEVENTF_RIGHTDOWN, xpos + 10, ypos + 10, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, xpos + 10, ypos + 10, 0, 0);

            SetCursorPos(xpos + 350, ypos + 40);

            mouse_event(MOUSEEVENTF_RIGHTDOWN, xpos + 350, ypos + 40, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, xpos + 350, ypos + 40, 0, 0);

            SetCursorPos(current_x, current_y);
        }  
        */

        private void btn_Zurueck2_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Menu());
        }

        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (Directory.Exists(Globals.projectDirectory + "/TrainingFaces/" + txt_NameSuchen.Text))
                {
                    Uri u = new Uri(Globals.projectDirectory + "/TrainingFaces/" + txt_NameSuchen.Text);
                    explorer.Source = u;
                }
                else
                {
                    l_Fehler.Content = "Person nicht gefunden!";
                }
            }
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if(explorer.CanGoBack)
            {
                explorer.GoBack();
            }
        }

        private void btn_Forward_Click(object sender, RoutedEventArgs e)
        {
            if(explorer.CanGoForward)
            {
                explorer.GoForward();
            }
        }
    }
}