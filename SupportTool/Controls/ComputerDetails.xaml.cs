using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
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

namespace SupportTool.Controls
{
    /// <summary>
    /// Interaction logic for ComputerDetails.xaml
    /// </summary>
    public partial class ComputerDetails : UserControl, IViewFor<ComputerDetailsViewModel>
    {
        public ComputerDetails()
        {
            InitializeComponent();



            this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.CNTextBlock.Text);
            this.OneWayBind(ViewModel, vm => vm.Computer.Company, v => v.CompanyTextBlock.Text);
        }

        public ComputerObject Computer
        {
            get { return (ComputerObject)GetValue(ComputerProperty); }
            set { SetValue(ComputerProperty, value); }
        }

        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(ComputerDetails), new PropertyMetadata(null, (d,e)=> (d as ComputerDetails).ViewModel.Computer = e.NewValue as ComputerObject));

        public ComputerDetailsViewModel ViewModel
        {
            get { return (ComputerDetailsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerDetailsViewModel), typeof(ComputerDetails), new PropertyMetadata(new ComputerDetailsViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ComputerDetailsViewModel; }
        }
    }
}
