using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupportTool.Services.DialogServices
{
    public class DialogService
    {
        public static void ShowMessage(string text, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None) => MessageBox.Show(text, caption, button, icon);

        public static void ShowError(string text, string caption = "An error occured") => ShowMessage(text, caption, icon: MessageBoxImage.Error);

        public static void ShowInfo(string text, string caption = "") => ShowMessage(text, caption, icon: MessageBoxImage.Information);

        public static bool ShowPrompt(string question, string caption = "", bool showCancel = false) => MessageBox.Show(question, caption, !showCancel ? MessageBoxButton.YesNo : MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes;


        public static void ShowPasswordSetMessage(string password) => ShowInfo($"New password is: {password}\nMust be changed at next logon", "Password set");

        public static void ShowPasswordSetErrorMessage() => ShowError($"Could not set password", "Password not set");
    }
}
