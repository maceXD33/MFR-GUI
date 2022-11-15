using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Security.Cryptography;
using System.IO;

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

        private void btn_speichern_Click(object sender, RoutedEventArgs e)
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
            string trainingFacesDirectory = Globals.projectDirectory + "/TrainingFaces/";
            File.WriteAllText(trainingFacesDirectory + "passwort.txt", savedPasswordHash);
        }

        private void btn_zurück_Click(object sender, RoutedEventArgs e)
        {
            Startseite s = new Startseite();
            this.NavigationService.Navigate(s);
        }
    }
}
