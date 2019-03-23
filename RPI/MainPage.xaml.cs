using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using Windows.Media.Audio;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RPI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public HttpClient Client { get; set; }
        public DispatcherTimer Timer { get; set; }

        public bool Online { get; set; }

        public GpioController gpio { get; set; }
        public Dictionary<string, GpioPin> pins { get; set; }

        public GpioPin Button { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            gpio = GpioController.GetDefault();

            pins = new Dictionary<string, GpioPin>();

            pins["18"] = gpio.OpenPin(18);
            pins["23"] = gpio.OpenPin(23);
            pins["24"] = gpio.OpenPin(24);
            pins["25"] = gpio.OpenPin(25);

            Button = gpio.OpenPin(4);

            foreach (GpioPin pin in pins.Values)
            {
                pin.Write(GpioPinValue.High);
                pin.SetDriveMode(GpioPinDriveMode.Output);
            }

            if (Button.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                Button.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                Button.SetDriveMode(GpioPinDriveMode.Input);

            Button.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            Button.ValueChanged += Button_ValueChanged;

            Client = new HttpClient();

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += Timer_Tick;

            Timer.Start();
        }

        private void Button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (e.Edge == GpioPinEdge.FallingEdge)
            {
                Client.GetStringAsync("http://test.farbeyondapp.com/Trigger/4/3").GetAwaiter().GetResult();
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            if (!Online)
            {
                Online = true;

                string response = Client.GetStringAsync("http://test.farbeyondapp.com/Listen/2").GetAwaiter().GetResult();

                GpioPin pin = null;

                if (response != "")
                {

                    if (response == "IO Lamp")
                    {
                        pin = pins["18"];

                        if (pin.Read() == GpioPinValue.High)
                        {
                            pin.Write(GpioPinValue.Low);
                        }
                        else
                        {
                            pin.Write(GpioPinValue.High);
                        }

                        SwitchStates(18);
                    }
                    else if (response == "IO Kettle")
                    {
                        pin = pins["23"];

                        if (pin.Read() == GpioPinValue.High)
                        {
                            pin.Write(GpioPinValue.Low);
                        }
                        else
                        {
                            pin.Write(GpioPinValue.High);
                        }

                        SwitchStates(23);
                    }
                    else
                    {
                        wanna.Stop();
                        beatit.Stop();
                        billie.Stop();
                        baby.Stop();
                        thriller.Stop();

                        if (response == "Wanna Be Startin Somethin")
                        {
                            wanna.Play();
                        }
                        else if (response == "Beat It")
                        {
                            beatit.Play();
                        }
                        else if (response == "Billie Jean")
                        {
                            billie.Play();
                        }
                        else if (response == "Baby Be Mine")
                        {
                            baby.Play();
                        }
                        else if (response == "Thriller")
                        {
                            thriller.Play();
                        }
                    }

                }

                Online = false;
            }

        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string pinNumber = ((Image)sender).Name.Replace("btn", "");

            if (pinNumber.Length == 3)
                pinNumber = pinNumber.Substring(0, 2);

            GpioPin pin = pins[pinNumber];

            if (pin.Read() == GpioPinValue.High)
            {
                pin.Write(GpioPinValue.Low);
            }
            else
            {
                pin.Write(GpioPinValue.High);
            }

            SwitchStates(pin.PinNumber);
        }

        private void SwitchStates(int pin)
        {
            Image img = (Image)FindName("btn" + pin);

            if (img.Visibility == Visibility.Collapsed)
                img.Visibility = Visibility.Visible;
            else
                img.Visibility = Visibility.Collapsed;

            img = (Image)FindName("btn" + pin + "2");

            if (img.Visibility == Visibility.Collapsed)
                img.Visibility = Visibility.Visible;
            else
                img.Visibility = Visibility.Collapsed;
        }
    }
}
