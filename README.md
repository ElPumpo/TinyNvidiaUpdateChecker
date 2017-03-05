# TinyNvidiaUpdateChecker
Tiny application which checks for NVIDIA GeForce GPU drivers, written in C-sharp (C#) for Windows

Instead of using GeForce Experience you can use this tiny application which will be highly customizable, secure and easy to use.
It searches for NVIDIA GPU drivers exactly like GeForce Experience does but in a lightweight (and open source) application. 

When you install GeForce Experience a lot of extra services are being installed without you knowing, and we have no clue what the executables does in the background when the application isn't even used.
Not only that but GeForce Experience is really slow and I don't want to maunally go to their website to check for updates.

# Download
Downloads are available [here](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases).
Don't forget to download `HtmlAgilityPack.dll` which is a dependency, it must be in the same folder as the executable.

For forkers: [HTML Agility Pack](https://www.nuget.org/packages/HtmlAgilityPack) will automatically install when attempting to debug the project (make sure you're running the latest version of VS2015+U3), or you may manually install it by doing the following: Open up your Package Manager Console and type in `Install-Package HtmlAgilityPack`.

# Requirements
+ Windows 10, 8.x or 7
+ .NET framework 4.5
+ `HtmlAgilityPack.dll` in same folder as the executable
+ Stable internet connection

# Command line arguments
| Argument   | Explanation |
| ---------- |:-----------:|
| --quiet | run quiet |
| --erase-config | erase local configuration file |
| --debug | turn debugging on |
| --force-dl | force download of drivers |
| --version | Current application version |
| --help | view help |

# Configuration file
You may customize the application using the config file located at `%localappdata%\Hawaii_Beach\TinyNvidiaUpdateChecker` and modify `app.config`

| Option | Restrictions | Effect |
| ------ | ------------ |:------:|
| Check for Updates | false or true | enables or disables searches for client updates |
| GPU Type | desktop or mobile | self-explanatory, select `desktop` if you're running a desktop system configuiration |
| Show Driver Description | false or true | Enables showing the driver description of drivers, which is in a beta state |
| GPU Name | n/a | sets the gpu to be fetched |

# Legal

### License
TinyNvidiaUpdateChecker - Check for NVIDIA GPU drivers, GeForce Experience replacer

Copyright (C) 2016 Hawaii_Beach

This program Is free software: you can redistribute it And/Or modify it under the terms Of the GNU General Public License As published by the Free Software Foundation, either version 3 Of the License, Or (at your option) any later version.

This program Is distributed In the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty Of MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License For more details.

You should have received a copy Of the GNU General Public License along with this program. If Not, see <http://www.gnu.org/licenses/>.
