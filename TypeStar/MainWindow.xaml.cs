using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;

namespace TypeStar
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        /*
         * TODO LIST:
         * - make multiple mistakes in a row (requires updated calculation in UpdateRawWPM)
         * - variable mistake length (requires updated calculation in UpdateRawWPM)
         * - WPF bindings and validation instead of event-based manual updates/validation
         */
        private static readonly string CHROME_PATH = @"exe\";

        private double TARGET_WPM;
        private double TARGET_ACC;
        private double CHARS_PER_WORD;

        private double RAW_WPM;
        private int TIME_LO;
        private int TIME_HI;

        private int SLEEP_INTERVAL;
        private int LOOP_INTERVAL;
        private int POST_RACE_DELAY;

        private IWebDriver _driver;

        private bool _stopNow;
        private bool _lastRun;

        private static readonly Random RNG = new Random();

        public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
            InitDriverAsync();
		}

		private async void InitDriverAsync()
		{
			await Task.Run(() =>
			{
				_driver = new ChromeDriver(CHROME_PATH);
			});

			this.btnStart.IsEnabled = true;

            await Task.Run(() =>
            {
                _driver.Navigate().GoToUrl("http://www.typeracer.com");
            });
        }

		private void Window_Closed(object sender, EventArgs e)
		{
			if (_driver != null)
			{
				_driver.Quit();
			}
		}

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			RunAsync();
		}

		private async void RunAsync()
		{
			// Disable all the controls on the form while it's running
			this.btnStart.IsEnabled = false;
			this.btnStopNow.IsEnabled = true;
            this.btnLastRun.IsEnabled = true;

            _stopNow = false;
            _lastRun = false;
			await Task.Run(() =>
			{
				while (!_lastRun)
				{
					// Click the "Race Again" link if it is present
					while (!_lastRun)
					{
						try
						{
							IWebElement againLink = GetRaceAgainLink();
							if (againLink != null && againLink.Displayed)
							{
								againLink.Click();
							}
							break;
						}
						catch { }
					}

					// Get the class of the textbox, which will determine if the game has started or not
					string inputClass;
					do
					{
						try
						{
							IWebElement input = GetInputBox();
							inputClass = input.GetAttribute("class");
						}
						catch
						{
							inputClass = null;
						}
					} while (inputClass == null && !_stopNow);

					// If the race has started, then do the race
					if (inputClass != null && !inputClass.Contains("txtInput-unfocused"))
					{
						DoRace();
						ResponsiveSleepLastRun(POST_RACE_DELAY);
					}

					// Wait a little bit to not tax the CPU so hard with this loop
					ResponsiveSleepLastRun(LOOP_INTERVAL);
				}
			});

			// Enable all the controls when it's done running
			this.btnStart.IsEnabled = true;
            this.btnStopNow.IsEnabled = false;
            this.btnLastRun.IsEnabled = false;
        }

		private void btnStopNow_Click(object sender, RoutedEventArgs e)
		{
			_stopNow = true;
            _lastRun = true;

            this.btnStopNow.IsEnabled = false;
            this.btnLastRun.IsEnabled = false;
        }

        private void btnLastRun_Click(object sender, RoutedEventArgs e)
        {
            _lastRun = true;

            this.btnLastRun.IsEnabled = false;
        }

        private IWebElement GetInputBox()
		{
			return _driver.FindElement(By.XPath("//table[@class = 'gameView']/tbody/tr[2]//table/tbody/tr[2]//input"));
		}

		private IWebElement GetRaceAgainLink()
		{
			return _driver.FindElement(By.XPath("//table[@class = 'gameView']/tbody/tr[3]//table[@class = 'navControls']//a[@class = 'raceAgainLink']"));
		}

		private string GetRaceText()
		{
			return _driver.FindElement(By.XPath("//table[@class = 'gameView']/tbody/tr[2]//table//tr/td/div/div/div")).Text;
		}

		private void SleepRandom(Random rng, int lo, int hi)
		{
			ResponsiveSleepStopNow(rng.Next(lo, hi + 1));
		}

		private void DoActionDelay(Action act, int time)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			act.Invoke();

            int diff = (int)(time - sw.ElapsedMilliseconds);
            ResponsiveSleepStopNow(diff);
		}

		private void DoRace()
		{
			IWebElement inputElt = GetInputBox();
			string txt = GetRaceText();
			foreach (char c in txt)
			{
				if (_stopNow)
				{
					break;
				}

				while (RNG.NextDouble() > TARGET_ACC && !_stopNow)
				{
					DoActionDelay(
						() => inputElt.SendKeys(char.ToString((char)RNG.Next('a', 'z' + 1))),
						RNG.Next(TIME_LO, TIME_HI + 1)
					);
					DoActionDelay(
						() => inputElt.SendKeys(Keys.Backspace),
						RNG.Next(TIME_LO, TIME_HI + 1)
					);
				}
				DoActionDelay(
					() => inputElt.SendKeys(char.ToString(c)),
					RNG.Next(TIME_LO, TIME_HI + 1)
				);
			}
		}

		private void ResponsiveSleepLastRun(int ms)
		{
            if (_lastRun || ms <= 0)
            {
                return;
            }

            if (ms <= SLEEP_INTERVAL)
            {
                Thread.Sleep(ms);
            }
            else
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (!_lastRun && sw.ElapsedMilliseconds < ms)
                {
                    int time = (int)Math.Max(SLEEP_INTERVAL, ms - sw.ElapsedMilliseconds);
                    if (time > 0)
                    {
                        Thread.Sleep(time);
                    }
                }
            }
		}

        private void ResponsiveSleepStopNow(int ms)
        {
            if (_stopNow || ms <= 0)
            {
                return;
            }

            if (ms <= SLEEP_INTERVAL)
            {
                Thread.Sleep(ms);
            }
            else
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (!_stopNow && sw.ElapsedMilliseconds < ms)
                {
                    int time = (int)Math.Max(SLEEP_INTERVAL, ms - sw.ElapsedMilliseconds);
                    if (time > 0)
                    {
                        Thread.Sleep(time);
                    }
                }
            }
        }

        private void UpdateTargetWPM(object sender, RoutedEventArgs e)
        {
            ValidateDoubleTextBox(this.txtTargetWPM, ref TARGET_WPM, 0);
            UpdateTargetWPM();
        }

        private void UpdateTargetAccuracy(object sender, RoutedEventArgs e)
        {
            ValidateDoubleTextBox(this.txtTargetAccuracy, ref TARGET_ACC, 0, 1);
            UpdateTargetWPM();
        }

        private void UpdateCharsPerWord(object sender, RoutedEventArgs e)
        {
            ValidateDoubleTextBox(this.txtCharsPerWord, ref CHARS_PER_WORD, 0);
            UpdateTargetWPM();
        }

        private void UpdateSleepInterval(object sender, RoutedEventArgs e)
        {
            ValidateIntegerTextBox(this.txtSleepInterval, ref SLEEP_INTERVAL);
        }

        private void UpdateLoopInterval(object sender, RoutedEventArgs e)
        {
            ValidateIntegerTextBox(this.txtLoopInterval, ref LOOP_INTERVAL);
        }

        private void UpdatePostRaceDelay(object sender, RoutedEventArgs e)
        {
            ValidateIntegerTextBox(this.txtPostRaceDelay, ref POST_RACE_DELAY);
        }

        private void InvalidTextBox(TextBox tb)
        {
            tb.Background = Brushes.LightPink;
        }

        private void ValidTextBox(TextBox tb)
        {
            tb.Background = Brushes.White;
        }

        private bool ValidateDoubleTextBox(TextBox tb, ref double val, double lo = double.MinValue, double hi = double.MaxValue)
        {
            string txt = tb.Text;
            double res;
            if (double.TryParse(txt, out res) && res >= lo && res <= hi)
            {
                val = res;
                ValidTextBox(tb);
                return true;
            }
            else
            {
                InvalidTextBox(tb);
                return false;
            }
        }

        private bool ValidateIntegerTextBox(TextBox tb, ref int val, int lo = int.MinValue, int hi = int.MaxValue)
        {
            string txt = tb.Text;
            int res;
            if (int.TryParse(txt, out res) && res >= lo && res <= hi)
            {
                val = res;
                ValidTextBox(tb);
                return true;
            }
            else
            {
                InvalidTextBox(tb);
                return false;
            }
        }

        private void UpdateTargetWPM()
        {
            RAW_WPM = TARGET_WPM * (3 - (2 * TARGET_ACC));
            this.txtRawWPM.Text = RAW_WPM.ToString("N3");

            double cpm = (1 + CHARS_PER_WORD) * TARGET_WPM;
            double delay = 60000 / cpm;
            TIME_LO = Math.Max(1, (int)(delay * 0.25));
            TIME_HI = Math.Max(1, (int)(delay * 1.75));
        }
    }
}
