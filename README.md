# TypeStar

Simple C# bot for TypeRacer using Selenium WebDriver

## Warnings and Disclaimers

* This is actually not allowed on the TypeRacer site, so if you have moral qualms about this whole thing, then you can stop reading here.
* You will probably get banned for using this, since the simple bot does not do a great job at mimicking human behavior (see the "Shortcomings and Future Work" section at the bottom), so I wouldn't recommend using it on your main account.  I'm sure the admins at TypeRacer are smart people, so they will eventually detect and ban you, especially if you set the bot to type at high WPM.

## Quick Start Guide

1. When the program loads, it should open up a terminal window that will control a web browser (currently Google Chrome) that will open up shortly after.  Make sure that you don't close these windows or the bot will probably crash.
2. After this browser loads TypeRacer, you can remain a guest or log into your account.
3. In the program, in the "Play TypeRacer" section, click the "Start" button.
4. Now you can just join a race, and when it starts the bot should automatically start playing.
5. When you wish to stop, you can click the "Stop Now" or "Last Race" buttons under the "Play TypeRacer" section.  You can also just exit the program.

## Configuration Options

### Timing Settings

These are probably fine as they are, so you don't really have to change the default values, but if you want to, here's what they do.

* **Sleep Interval**: When the bot waits for a period of time, it performs a check every so often to see if the user terminated the bot.  This determines the interval at which the bot checks for those flags.  Lowering this number should make clicking "Stop" and "Last Run" feel more responsive potentially at the cost of performance, so I think the default value of 100ms is pretty reasonable.
* **Loop Interval**: This determines the interval at which the bot checks if a race has started.  A lower value will help the bot start each race faster, but will require more CPU time.  The default is to check every 100ms, which is already quite fast and doesn't require examining the webpage too frequently.
* **Post-Race Delay**: This is the amount of time that the bot will wait after each race before attempting to join a new race.  It doesn't have too many practical uses, but you could use it to have to bot either join immediately to get as many races in as possible, or perhaps to have a long delay in between so that it doesn't play too many races if left unattended for a while.

### WPM Settings

These are the most likely the settings you'll be interested in, since they set the parameters of the typing bot.  These are mostly used to calculate the Raw WPM at the end.  The bot plays TypeRacer in a very simple way, which makes the calculation very simple.  It inputs the text one character at a time, and if it makes a mistake, it will always delete the erroneous character on it's next keystroke.  I believe you can actually change the values in these boxes during the race so that the bot will type faster or slower in real-time, but you have to tab out of each box when you're done editing (pressing Enter won't work).

* **Target WPM**: This is the WPM that you would like the bot to reach.  It is the WPM after all errors have been corrected.  TypeRacer will also automatically reject WPM scores that are way too high.  Note that TypeRacer will require you to verify your speed by completing a captcha typing test, for which you get several tries to succeed.  You don't need perfect accuracy, and TypeRacer will allow WPM 25% higher than your test result to be accepted.  For example, if you scored 160 WPM on the typing test, TypeRacer will accept all scores up to 200 WPM.  Don't set this number way too high such that you won't be able to pass the test.  If you care about saving your score, the bot is only as good as 125% of your peak typing speed.
* **Target Accuracy**: This is the probability that the bot will type the correct character.  You should set this value between 0 and 1, inclusive.
* **Letters per Word**: This is the average value number of characters in each word.  Since the bot actually uses CPM (characters per minute) internally to determine how fast it should type, this parameter helps convert the CPM to WPM.  The default value is just what TypeRacer says they use to calculate your WPM, so it's not great to change this value.
* **Raw WPM**: You can't actually set this yourself, but this uses the parameters you supplied above to produce an estimate of the raw WPM, which is the WPM including errors.

### Play TypeRacer

You can use these buttons to control the bot.

* **Start**: Starts the bot.  It will automatically start typing once it finds itself in a race, and it will automatically join new races after it's done, but it won't join a race if you're on the TypeRacer homepage.  It can only play an entire race though, such that if you had already typed a few words, it won't be able to do it because it'll try to start at the beginning.
* **Stop Now**: This stops the bot right where it is.  Note that if you stopped it in the middle of a race, you won't be able to finish the race if you try to start the bot again, so you can either finish the race manually or just refresh the page to quit the race.
* **Last Run**: This will stop the bot unless it is in the middle of a race.  This does mean that if it's in a countdown before a race, it will stop and the bot won't actually participate in the upcoming race.

## Shortcomings and Future Work

Most of this will probably never get done, since this is just a simple demonstration of using Selenium WebDriver to play a game like TypeRacer, but if later down the line and I'm really feeling it, I might implement some of these things below.

* Currently, the bot calculates a target average CPM, and then before typing each character, it will select an amount of time over a uniform distribution slightly above and below the target CPM.  As a result, it looks like it's typing around the same speed the entire race.  This is not terribly realistic because the actual time for each character typed probably varies greatly.  One possible enhancement would be for the bot to select a wait time off of a user-specified distribution--perhaps a more realistic one would be high probability for low times, but low non-zero probabilities for much longer times.  Additionally, certain letters can be more difficult to type after other letters, such as if the same finger had to be used for consecutive letters, which is then dependent on the keyboard layout.  However, all of this would make calculating the Raw WPM much harder.
* Currently, the bot will always correct an error on the next keystroke.  It also has the same wait time for all keystrokes, including Backspace to correct errors.  This is not realistic because most people will probably make multiple errors before realizing that they made a mistake, and then go back and correct all of those errors.  A potential enhancement to address this issue would be for varying the number of characters before the bot "realizes" that it made an earlier error and is forced to correct it.  Bonus points if it can use the Ctrl-Backspace or Ctrl-A Backspace to delete multiple characters at once.
* Currently, the bot has the same error rate on every character, regardless of what it is.  This is not realistic because humans probably make more mistakes typing certain characters and words than others--for example, I would imagine that people are more likely to make a mistake on numbers or infrequently used punctuation symbols than common English words.  One possible enhancement would be a definition of error rates for different characters.
* Currently, the bot makes errors by typing a random lowercase letter.  This is not realistic because errors are more likely to be made by keys near the correct one rather than by a random key.  Additionally, this means that it very rarely will actually type the correct character, although it will still delete the character.  One possible enhancement could be to select the error key based on other keys near the correct key.  Bonus points for taking different keyboard layouts into account.
* Currently, the bot only has variance for timings at each character.  This means the bot's scores are very similar across races.  This is not realistic because humans probably have much higher variance between races, without consistently getting similar numbers for both WPM and accuracy from race to race.  One possible way to address this issue could be to have these settings be chosen off of another distribution at the start of each race.

At the end of the day, it's probably more fun to actually get better at typing than to use a program to artificially boost your scores on TypeRacer, right?
