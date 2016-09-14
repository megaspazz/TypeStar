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

namespace TypeStar
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string CHROME_PATH = @"exe\";
		private const double CHARS_PER_WORD = 5;

		private const int SLEEP_INTERVAL = 100;
		private const int LOOP_INTERVAL = 100;
		private const int POST_RACE_DELAY = 2500;

		private const double TARGET_WPM = 280;
		private const double TARGET_ERR = 0.05;
		
		private IWebDriver _driver;

		private bool _stop;

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
				_driver.Navigate().GoToUrl("http://www.typeracer.com");
			});

			this.btnStart.IsEnabled = true;
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
			this.btnStop.IsEnabled = true;

			_stop = false;
			await Task.Run(() =>
			{
				while (!_stop)
				{
					// Click the "Race Again" link if it is present
					while (true)
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
					} while (inputClass == null);

					// If the race has started, then do the race
					if (!inputClass.Contains("txtInput-unfocused"))
					{
						DoRace(TARGET_WPM, TARGET_ERR);
						ResponsiveSleep(POST_RACE_DELAY);
					}

					// Wait a little bit to not tax the CPU so hard with this loop
					Thread.Sleep(LOOP_INTERVAL);
				}
			});

			// Enable all the controls when it's done running
			this.btnStart.IsEnabled = true;
			this.btnStop.IsEnabled = false;
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			_stop = true;
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
			Thread.Sleep(rng.Next(lo, hi + 1));
		}

		private void DoActionDelay(Action act, int time)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			act.Invoke();
			int diff = (int)(time - sw.ElapsedMilliseconds);
			if (diff > 0)
			{
				Thread.Sleep(diff);
			}
		}

		private void DoRace(double wpm, double err)
		{
			Random rng = new Random();
			double cpm = CHARS_PER_WORD * wpm;
			double delay = 60000 / cpm;
			int timeLo = Math.Max(1, (int)(delay * 0.25));
			int timeHi = Math.Max(1, (int)(delay * 1.75));

			IWebElement inputElt = GetInputBox();
			string txt = GetRaceText();
			foreach (char c in txt)
			{
				if (_stop)
				{
					break;
				}

				while (rng.NextDouble() < err && !_stop)
				{
					DoActionDelay(
						() => inputElt.SendKeys(char.ToString((char)rng.Next('a', 'z' + 1))),
						rng.Next(timeLo, timeHi + 1)
					);
					DoActionDelay(
						() => inputElt.SendKeys(Keys.Backspace),
						rng.Next(timeLo, timeHi + 1)
					);
				}
				DoActionDelay(
					() => inputElt.SendKeys(char.ToString(c)),
					rng.Next(timeLo, timeHi + 1)
				);
			}
		}

		private void ResponsiveSleep(int ms)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			while (!_stop && sw.ElapsedMilliseconds < ms)
			{
				int time = (int)Math.Max(SLEEP_INTERVAL, ms - sw.ElapsedMilliseconds);
				if (time > 0)
				{
					Thread.Sleep(time);
				}
			}
		}
	}
}
