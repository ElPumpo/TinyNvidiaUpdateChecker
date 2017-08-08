![Image preview](image.png)

# TinyNvidiaUpdateChecker
Tiny application which checks for NVIDIA GeForce GPU drivers, written in C-sharp (C#) for Windows

Visit the [wiki](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/wiki) for more information about the application!

Instead of using GeForce Experience you can use this tiny application which is customizable, secure and very easy to use.
It searches for NVIDIA GPU drivers exactly like GeForce Experience does but in a lightweight (and open source) solution. 

When you install GeForce Experience a lot of extra services are being installed without you knowing, and we have no clue what the executables does in the background when the application isn't even used.

NVIDIAs drivers also come with telemetry which this application can remove from the installer, but requries WinRAR in order to extract the self-extracting archive, and will only extract the core GPU drivers. (no Physics, HD Audio, Geforce Experience, 3D drivers, etc).

# Download
Downloads are available [here](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases).
Don't forget to download `HtmlAgilityPack.dll` which is a dependency and must be in the executing directory.

### Forkers
[HTML Agility Pack](https://www.nuget.org/packages/HtmlAgilityPack) will automatically install when attempting to debug the project (make sure you're running the latest version of VS2017), or you may manually install it by doing the following: Open up your Package Manager Console and type in `Install-Package HtmlAgilityPack`.

# Requirements
+ Windows 10, 8.x or 7
+ .NET framework 4.5
+ `HtmlAgilityPack.dll` in same folder as the executable (automatic download from v1.8.0 and up)
+ Stable internet connection
+ [WinRAR](http://www.rarlab.com/) (if using minimal installer method)

# Legal

### License
TinyNvidiaUpdateChecker - Check for NVIDIA GPU drivers, GeForce Experience replacer

Copyright (C) 2016-2017 Hawaii_Beach

This program Is free software: you can redistribute it And/Or modify it under the terms Of the GNU General Public License As published by the Free Software Foundation, either version 3 Of the License, Or (at your option) any later version.

This program Is distributed In the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty Of MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License For more details.

You should have received a copy Of the GNU General Public License along with this program. If Not, see <http://www.gnu.org/licenses/>.

### Project icon
[Project icon](https://github.com/Maddoc42/Android-Material-Icon-Generator) by [Maddoc42](https://github.com/Maddoc42) is licensed under [CC BY-NC 3.0](https://creativecommons.org/licenses/by-nc/3.0/)

### HTML Agility Pack
HTML Agility Pack by [zzzprojects](https://github.com/zzzprojects/html-agility-pack) is licensed under the [MIT License](https://opensource.org/licenses/MIT)

### ASCII-alike Progress Bar
[ASCII-alike Progress Bar](https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54) by [DanielSWolf](https://github.com/DanielSWolf) is licensed under the [MIT License](https://opensource.org/licenses/MIT)
