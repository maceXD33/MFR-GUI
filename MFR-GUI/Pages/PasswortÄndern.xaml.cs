using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Security.Cryptography;
using System.IO;
using static MFR_GUI.Pages.Globals;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für PasswortÄndern.xaml
    /// </summary>
    public partial class PasswortÄndern : Page
    {
        public PasswortÄndern()
        {
            InitializeComponent();

            alt_PasswordHidden.Focus();

            if (!File.Exists(Globals.projectDirectory + "/Image/passwort.txt"))
            {
                alt_PasswordUnmask.Visibility = Visibility.Hidden;
                alt_PasswordHidden.Visibility = Visibility.Hidden;
                alt_passwort.Visibility = Visibility.Hidden;
                alt_ShowPassword.Visibility = Visibility.Hidden;
                neu_Password_Label.Content = "Password:";
            }

        }

        private void alt_ShowPassword_PreviewMouseDown(object sender, MouseButtonEventArgs e) => alt_ShowPasswordFunction();
        private void alt_ShowPassword_PreviewMouseUp(object sender, MouseButtonEventArgs e) => alt_HidePasswordFunction();
        private void alt_ShowPassword_MouseLeave(object sender, MouseEventArgs e) => alt_HidePasswordFunction();

        private void alt_ShowPasswordFunction()
        {          
            alt_PasswordUnmask.Visibility = Visibility.Visible;
            alt_PasswordHidden.Visibility = Visibility.Hidden;
            alt_PasswordUnmask.Text = alt_PasswordHidden.Password;
        }

        private void alt_HidePasswordFunction()
        {           
            alt_PasswordUnmask.Visibility = Visibility.Hidden;
            alt_PasswordHidden.Visibility = Visibility.Visible;
        }

        private void neu_ShowPassword_PreviewMouseDown(object sender, MouseButtonEventArgs e) => neu_ShowPasswordFunction();
        private void neu_ShowPassword_PreviewMouseUp(object sender, MouseButtonEventArgs e) => neu_HidePasswordFunction();
        private void neu_ShowPassword_MouseLeave(object sender, MouseEventArgs e) => neu_HidePasswordFunction();

        private void neu_ShowPasswordFunction()
        {
            neu_PasswordUnmask.Visibility = Visibility.Visible;
            neu_PasswordHidden.Visibility = Visibility.Hidden;
            neu_PasswordUnmask.Text = neu_PasswordHidden.Password;
        }

        private void neu_HidePasswordFunction()
        {
            neu_PasswordUnmask.Visibility = Visibility.Hidden;
            neu_PasswordHidden.Visibility = Visibility.Visible;
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                btn_speichern_Click(sender, e);
            }
        }
            private void btn_speichern_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Globals.projectDirectory + "/Image/passwort.txt"))
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
                var pbkdf2 = new Rfc2898DeriveBytes(alt_PasswordHidden.Password, salt, 100000);
                byte[] hash = pbkdf2.GetBytes(20);
                // Compare the results 
                for (int i = 0; i < 20; i++)
                {
                    //Incorrect Password
                    if (hashBytes[i + 16] != hash[i])
                    {
                        password = false;
                        l_Fehler.Foreground = Brushes.Red;
                        l_Fehler.Content = "Altes Passwort ist falsch";
                    }
                }
                //Correct Password
                if (password)
                {
                    if (neu_PasswordHidden.Password != "")
                    {
                        passwort_ändern();
                        l_Fehler.Foreground = Brushes.Green;
                        l_Fehler.Content = "Passwort gespeichert!";
                        password_checked = true;
                    }
                    else
                    {
                        l_Fehler.Foreground = Brushes.Red;
                        l_Fehler.Content = "Passwort eingeben!";
                    }

                }
            }
            else
            {
                passwort_ändern();
                l_Fehler.Foreground = Brushes.Green;
                l_Fehler.Content = "Passwort gespeichert!";
                password_checked = true;

            }
        }

        private void btn_zurück_Click(object sender, RoutedEventArgs e)
        {
            Startseite s = new Startseite();
            this.NavigationService.Navigate(s);
        }
            
        public void passwort_ändern()
        {
            //Create the salt value with a cryptographic PRNG
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            //Create the Rfc2898DeriveBytes and get the hash value
            var pbkdf2 = new Rfc2898DeriveBytes(neu_PasswordHidden.Password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            //Combine the salt and password bytes for later use
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            //Turn the combined salt+hash into a string for storage
            string savedPasswordHash = Convert.ToBase64String(hashBytes);

            //Save passwort in File
            string trainingFacesDirectory = Globals.projectDirectory + "/Image/";
            File.WriteAllText(trainingFacesDirectory + "passwort.txt", savedPasswordHash);
        }
    }
}
