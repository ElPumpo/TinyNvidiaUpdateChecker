# TinyNvidiaUpdateChecker
Tiny application which checks for NVIDIA GeForce GPU drivers, written in C-sharp (C#) for Windows

Instead of using GeForce Experience you can use this tiny application which will be highly customizable, secure and easy to use.
It searches for NVIDIA GPU drivers exactly like GeForce Experience does but in a lightweight (and open source) application. 

When you install GeForce Experience a lot of extra services are being installed without you knowing, and we have no clue what the executables does in the background when the application isn't even used.
Not only that but GeForce Experience is really slow and I don't want to maunally go to their website to check for updates.

# Download
Downloads are available [here](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases).
Don't forget to download `HtmlAgilityPack.dll` which is a dependency.

Don't forget to setup HTML Agility Pack when cloning the project. Download the libary using GuNet. Open up your Package Manager Console and type in `Install-Package HtmlAgilityPack`. This will setup the required files for the libary in order to work.

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

# Todo
This application isn't perfect, it has multiple flaws and other stuff. Here is the to-do list:
- [ ] Port some stuff to a GUI
- [ ] Improve error handling
- [ ] Better local GPU driver version detection (Either `NvAPI` or through reading current driver mounted)
- [x] Add progress bar for driver download

# Legal

### License
TinyNvidiaUpdateChecker - Check for NVIDIA GPU drivers, GeForce Experience replacer

Copyright (C) 2016 Hawaii_Beach

This program Is free software: you can redistribute it And/Or modify it under the terms Of the GNU General Public License As published by the Free Software Foundation, either version 3 Of the License, Or (at your option) any later version.

This program Is distributed In the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty Of MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License For more details.

You should have received a copy Of the GNU General Public License along with this program. If Not, see <http://www.gnu.org/licenses/>.
