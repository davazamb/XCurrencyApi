using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using XCurrencyApi.Classes;

namespace XCurrencyApi.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Attributes

        private ExchangeRates exchangeRates;

        private decimal amount;

        private double sourceRate;

        private double targetRate;

        private bool isRunning;

        private bool isEnabled;

        private string message;

        #endregion


        #region Properties

        public ObservableCollection<Rate> Rates { get; set; }

        public decimal Amount
        {
            set
            {
                if (amount != value)
                {
                    amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Amount"));
                }
            }
            get
            {
                return amount;
            }
        }

        public double SourceRate
        {
            set
            {
                if (sourceRate != value)
                {
                    sourceRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourceRate"));
                }
            }
            get
            {
                return sourceRate;
            }
        }

        public double TargetRate
        {
            set
            {
                if (targetRate != value)
                {
                    targetRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetRate"));
                }
            }
            get
            {
                return targetRate;
            }
        }

        public bool IsRunning
        {
            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get
            {
                return isRunning;
            }
        }

        public bool IsEnabled
        {
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
            get
            {
                return isEnabled;
            }
        }
        public string Message
        {
            set
            {
                if (message != value)
                {
                    message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
                }
            }
            get
            {
                return message;
            }
        }

        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        #region Constructors

        public MainViewModel()
        {
            Rates = new ObservableCollection<Rate>();
            Message = "Enter an amount, select a source currency, select a target currency and press convert button.";
            IsEnabled = false;
            GetRates();
        }
        #endregion

        #region Methods

        private void LoadRates()
        {
            Rates.Clear();
            var type = typeof(Rates);
            var properties = type.GetRuntimeFields();

            foreach (var property in properties)
            {
                var code = property.Name.Substring(1, 3);
                Rates.Add(new Rate
                {
                    Code = code,
                    TaxRate = (double)property.GetValue(exchangeRates.Rates),
                });
            }
        }

        private async void GetRates()
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://openexchangerates.org");
                var url = "/api/latest.json?app_id=c5ad6fcb983f45448075f48c7cfc32f0";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Error", response.StatusCode.ToString(), "Aceptar");
                    IsRunning = false;
                    IsEnabled = false;
                    return;
                }

                var result = await response.Content.ReadAsStringAsync();
                exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(result);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "Aceptar");
                IsRunning = false;
                IsEnabled = false;
                return;
            }

            LoadRates();
            IsRunning = false;
            IsEnabled = true;
        }

        #endregion

        #region Commands

        public ICommand ConvertCommand { get { return new GalaSoft.MvvmLight.Command.RelayCommand(ConvertMoney); } }

        private async void ConvertMoney()
        {
            if (Amount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter an amount", "Acept");
                return;
            }

            if (SourceRate == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must select a source rate", "Acept");
                return;
            }

            if (TargetRate == -1)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must select a target rate", "Acept");
                return;
            }

            decimal amountConverted = amount / (decimal)sourceRate * (decimal)targetRate;

            Message = string.Format("{0:N2} = {1:N2}", amount, amountConverted);
        }

        #endregion


    }
}
