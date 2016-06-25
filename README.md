# TinyNvidiaUpdateChecker
The application currently only supports desktop GeForce GPUs, if you want your GPU covered, start a issue above.

#About
Tiny application which checks for NVIDIA GPU drivers, developed in C# for Windows.

Instead of using GeForce Experience you can use this tiny application which will be highly customizable, secure and easy to use.
It searches for NVIDIA GPU drivers exactly like GeForce Experience does but in a lightweight (and open source) application. 

When you install GeForce Experience a lot of extra services are being installed without you knowing, and we have no clue what the executables does in the background when the application isn't even used.
Not only that but GeForce Experience is really slow and I don't want to maunally go to their website to check for updates.

#Download
Downloads are available [here](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases).
Don't forget to download `HtmlAgilityPack.dll` which is a dependency.

If you're forking this project don't worry about the dependencies as they are handled by NuGet.

#Requirements
+ Windows 10, 8.1, 8 or 7
+ .NET Framework 4
+ Stable internet connection

#Command arguments
| Argument   | Explanation |
| ---------- |:-----------:|
| --quiet | run quiet |
| --debug | turn debugging on |
| --? | view help |

#Configuration file
You may customize the application using the config file located at `%localappdata%\Hawaii_Beach\TinyNvidiaUpdateChecker` and modify `config.ini`

| Option | Restrictions | Effect |
| ------ | ------------ |:------:|
| Check for Updates | 0 or 1 | enables or disables searches for client updates |

#License
TinyNvidiaUpdateChecker - Check for NVIDIA GPU drivers, GeForce Experience replacer

Copyright (C) 2016 Hawaii_Beach

This program Is free software: you can redistribute it And/Or modify
it under the terms Of the GNU General Public License As published by
the Free Software Foundation, either version 3 Of the License, Or
(at your option) any later version.

This program Is distributed In the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty Of
MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License For more details.

You should have received a copy Of the GNU General Public License
along with this program.  If Not, see <http://www.gnu.org/licenses/>.
