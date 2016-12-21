using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Miriot.Utils
{
    public class UltrasonicDistanceSensor
    {
        #region Fields
        private readonly GpioPin _gpioPinTrig;
        private readonly GpioPin _gpioPinEcho;
        private bool _init;
        private readonly Stopwatch sw;
        #endregion

        public UltrasonicDistanceSensor(int trigGpioPin, int echoGpioPin)
        {
            var gpio = GpioController.GetDefault();
            sw = new Stopwatch();

            if (gpio == null) return;

            _gpioPinTrig = gpio.OpenPin(trigGpioPin);
            _gpioPinEcho = gpio.OpenPin(echoGpioPin);
            _gpioPinTrig.SetDriveMode(GpioPinDriveMode.Output);
            _gpioPinEcho.SetDriveMode(GpioPinDriveMode.Input);
            _gpioPinTrig.Write(GpioPinValue.Low);
        }

        public async Task InitAsync()
        {
            if (!_init && _gpioPinTrig != null)
            {
                //first time ensure the pin is low and wait two seconds
                _gpioPinTrig.Write(GpioPinValue.Low);
                await Task.Delay(2000);
                _init = true;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public async Task<double> GetDistanceAsync()
        {
            // turn on the pulse
            _gpioPinTrig.Write(GpioPinValue.High);
            await Task.Delay(10);
            _gpioPinTrig.Write(GpioPinValue.Low);

            sw.Reset();
            sw.Start();

            while (_gpioPinEcho.Read() == GpioPinValue.Low)
            {
                if (sw.Elapsed.TotalSeconds > 10)
                    break;
            }

            while (_gpioPinEcho.Read() == GpioPinValue.High) {  /*Loop unitl value has been read*/ }

            sw.Stop();

            var elapsed = sw.Elapsed.TotalSeconds;
            var distance = elapsed * 34000;

            distance /= 2;
            return distance;
        }
    }
}