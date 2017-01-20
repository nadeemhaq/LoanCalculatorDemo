using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LoanCalcMobile.Annotations;
using Xamarin.Forms;

namespace LoanCalcMobile
{
    public class CalculateCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var vm = parameter as LoanViewModel;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://loancalculatordemo.azurewebsites.net/");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync($"/api/loancalc?principal={vm.Principal}&numberofpayments={vm.NumberOfPayments}&interestrate={vm.InterestRate}").Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var paymentAmount = response.Content.ReadAsStringAsync().Result;                
                vm.PaymentAmount = $"Payment Amount: {double.Parse(paymentAmount):C}";

            }
        }

        public event EventHandler CanExecuteChanged;
    }

    public class LoanViewModel : INotifyPropertyChanged
    {
        public double Principal { get; set; }
        public int NumberOfPayments { get; set; }
        public double InterestRate { get; set; }

        private string _paymentAmount;
        public string PaymentAmount
        {
            get { return _paymentAmount; }
            set
            {
                _paymentAmount = value;
                OnPropertyChanged("PaymentAmount");
            }
        }

        public ICommand CalculateCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var model = new LoanViewModel {Principal = 20000, NumberOfPayments = 360, InterestRate = 0.04d, CalculateCommand = new CalculateCommand() };

            BindingContext = model;
        }
    }
}
