using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MagnumOpus.Dialog
{
    /// <summary>
    /// Interaction logic for DialogControl.xaml
    /// </summary>
    public partial class DialogControl : UserControl, IEnableLogger
    {
        public IObservable<int> Result => _resultSubject;

        private DialogControl()
        {
            InitializeComponent();
        }

        public DialogControl(Grid parent, DialogType? dialogType, string message, params DialogButtonInfo[] buttons)
        {
            InitializeComponent();

            _parent = parent;
            _buttons = new List<DialogButtonInfo>(buttons);

            SetValue(Grid.RowSpanProperty, _parent.RowDefinitions.Count > 0 ? _parent.RowDefinitions.Count : 1);
            SetValue(Grid.ColumnSpanProperty, _parent.ColumnDefinitions.Count > 0 ? _parent.ColumnDefinitions.Count : 1);

            if (dialogType == null) IconImage.Visibility = Visibility.Collapsed;
            else IconImage.Source = new BitmapImage(new Uri($@"\Assets\{dialogType.ToString()}.png", UriKind.Relative));

            MessageTextBlock.Text = message.Trim();

            Button focusButton = null;
            foreach (var buttonInfo in _buttons)
            {
                var button = new Button { Content = buttonInfo.Text, Tag = buttonInfo.Id };
                if(button.IsDefault = buttonInfo.IsDefault) focusButton = button;
                button.Click += HandleButtonClick;
                ButtonStackPanel.Children.Add(button);
            }

            this.Log().Info($"Showing dialog with content: {message}");

            _parent.Children.Add(this);

            Observable.Timer(TimeSpan.FromSeconds(.1), RxApp.MainThreadScheduler).Subscribe(_ => (focusButton ?? ButtonStackPanel.Children[0]).Focus());
        }

        public DialogControl(Grid parent, DialogType? dialogType, string caption, string message, params DialogButtonInfo[] buttons) : this(parent, dialogType, message, buttons)
        {
            if (caption.HasValue())
            {
                CaptionTextBlock.Text = caption.Trim();
                CaptionTextBlock.Visibility = Visibility.Visible;
            }
        }

        public DialogControl(Grid parent, DialogType? dialogType, string caption, string message, TimeSpan timeout, params DialogButtonInfo[] buttons) : this(parent, dialogType, caption, message, buttons)
        {
            _timeoutObservable = Observable.Timer(timeout)
                .TakeUntil(_resultSubject);
            _timeoutObservable
                .Subscribe(_ => Close());
        }

        public DialogControl(Grid parent, MessageInfo messageInfo) : this(parent, (DialogType)messageInfo.Type, messageInfo.Caption, messageInfo.Message, messageInfo.Buttons) { }



        private void HandleButtonClick(object sender, RoutedEventArgs args)
        {
            var tag = (sender as Button).Tag as Guid?;
            if (tag == null) return;

            var dbi = _buttons.First(dialogButtonInfo => dialogButtonInfo.Id == tag);

            if (_resultSubject != null)
            {
                _resultSubject.OnNext(_buttons.IndexOf(dbi));
                _resultSubject.OnCompleted();
                _resultSubject = null;
            }

            if (dbi.CloseDialog) Close();
        }

        private void Close()
        {
            if (_resultSubject != null)
            {
                _resultSubject.OnNext(-1);
                _resultSubject.OnCompleted();
            }
            _parent.Children.Remove(this);
        }



        private ISubject<int> _resultSubject = new Subject<int>();
        private Grid _parent;
        private List<DialogButtonInfo> _buttons;
        private IObservable<long> _timeoutObservable;
    }

    public enum DialogType { Info, Question, Success, Warning, Error }

    public struct DialogButtonInfo
    {
        public DialogButtonInfo(string text, bool closeDialog = true, bool isDefault = false)
        {
            Id = Guid.NewGuid();
            Text = text;
            CloseDialog = closeDialog;
            IsDefault = isDefault;
        }



        public Guid Id { get; private set; }
        public string Text { get; private set; }
        public bool CloseDialog { get; private set; }
        public bool IsDefault { get; private set; }
    }
}
