using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static MFR_GUI.Pages.Globals;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für Passwort_BildHinzufuegen.xaml
    /// </summary>
    public partial class Passwort_BildHinzufuegen : Page
    {
        public Passwort_BildHinzufuegen()
        {
            InitializeComponent();
        }
        private void ShowPassword_PreviewMouseDown(object sender, MouseButtonEventArgs e) => ShowPasswordFunction();
        private void ShowPassword_PreviewMouseUp(object sender, MouseButtonEventArgs e) => HidePasswordFunction();
        private void ShowPassword_MouseLeave(object sender, MouseEventArgs e) => HidePasswordFunction();

        private void ShowPasswordFunction()
        {
            PasswordUnmask.Visibility = Visibility.Visible;
            PasswordHidden.Visibility = Visibility.Hidden;
            PasswordUnmask.Text = PasswordHidden.Password;
        }

        private void HidePasswordFunction()
        {
            PasswordUnmask.Visibility = Visibility.Hidden;
            PasswordHidden.Visibility = Visibility.Visible;
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                btn_anmelden_Click(sender, e);
            }
        }

        private void btn_anmelden_Click(object sender, RoutedEventArgs e)
        {
            //Bool for Correct Password
            bool password = true;
            //Get Hashcode from File
            string trainingFacesDirectory = Globals.projectDirectory + "/Image/";
            string savedPasswordHash = File.ReadAllText(trainingFacesDirectory + "passwort.txt");
            // Extract the bytes 
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            // Get the salt 
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            // Compute the hash on the password the user entered 
            var pbkdf2 = new Rfc2898DeriveBytes(PasswordHidden.Password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            // Compare the results 
            for (int i = 0; i < 20; i++)
            {
                //Incorrect Password
                if (hashBytes[i + 16] != hash[i])
                {
                    password = false;
                    l_Fehler.Content = "Das Passwort ist falsch!";
                }
            }
            //Correct Password
            if (password)
            {
                BildHinzufuegen bh = new BildHinzufuegen();
                NavigationService.Navigate(bh);

                password_checked = true;
            }
        }

        private void btn_zurück_Click(object sender, RoutedEventArgs e)
        {
            Menu m = new Menu();
            this.NavigationService.Navigate(m);
        }
    }
}
