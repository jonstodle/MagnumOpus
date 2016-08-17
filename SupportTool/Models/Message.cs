using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupportTool.Models
{
    public class Message
    {
        public string Text { get; set; }
        public string Caption { get; set; }
        public MessageBoxButton Button { get; set; }
        public MessageBoxImage Icon { get; set; }



        public static Message Info(string text, string caption = "") => new Message { Text = text, Caption = caption, Button = MessageBoxButton.OK, Icon = MessageBoxImage.Information };

        public static Message Error(string text, string caption = "An error occured") => new Message { Text = text, Caption = caption, Button = MessageBoxButton.OK, Icon = MessageBoxImage.Error };


        public static Message PasswordSet(string password) => Info($"New password is: {password}", "Password set");

        public static Message PasswordSetError() => Error($"Could not set password", "Password not set");
    }
}
