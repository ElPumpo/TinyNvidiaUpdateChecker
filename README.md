![Image preview](image.png)

# TinyNvidiaUpdateChecker

TinyNvidiaUpdateChecker (TNUC for short) is a lightweight application that checks for NVIDIA GPU drivers, written in C-sharp (C#) for Windows. When executed it will check for new driver updates. You can customize TNUC making it execute when logging in your computer, and have it only pop up if there's a new driver available.

It supports extracting drivers to only install the required driver files, which is useful since the default driver installer installs a bunch of bloatware on your computer that you don't need.

**Visit the [Wiki](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/wiki) page for detailed information about the application**

## Update

Thanks to everyone making TNUC popular by staring the project here on GitHub and all the people writing articles about it! Many years ago I started this hobby project to automate checking driver updates, I know the codebase isn't proper and I've learned a lot since then. Even today there's nearly a hundred views on this page daily, which is huge for me, so thanks <3

# Download

[Go to the releases page](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases) for all downloads

## Forkers

[HTML Agility Pack](https://www.nuget.org/packages/HtmlAgilityPack) will automatically install when attempting to debug the project (make sure you're running the latest version of VS2017), or you may manually install it by doing the following: Open up your Package Manager Console and type in `Install-Package HtmlAgilityPack`.

# Requirements

+ Windows 10, 8.x or 7
+ .NET framework 4.6.1
+ `HtmlAgilityPack.dll` in same folder as the executable (automatic download from v1.8.0 and up)
+ Stable internet connection
+ (optional) [WinRAR](https://www.rarlab.com/) or [7-Zip](http://www.7-zip.org)

# Legal

## License

TinyNvidiaUpdateChecker - Check for NVIDIA GPU driver updates!

Copyright (C) 2016-2020 Hawaii_Beach

This program Is free software: you can redistribute it And/Or modify it under the terms Of the GNU General Public License As published by the Free Software Foundation, either version 3 Of the License, Or (at your option) any later version.

This program Is distributed In the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty Of MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License For more details.

You should have received a copy Of the GNU General Public License along with this program. If Not, see <http://www.gnu.org/licenses/>.

## Project icon

[Project icon](https://github.com/Maddoc42/Android-Material-Icon-Generator) by [Maddoc42](https://github.com/Maddoc42) is licensed under [CC BY-NC 3.0](https://creativecommons.org/licenses/by-nc/3.0/)

## HTML Agility Pack

HTML Agility Pack by [zzzprojects](https://github.com/zzzprojects/html-agility-pack) is licensed under the [MIT license](https://opensource.org/licenses/MIT)

## ASCII-alike Progress Bar

[ASCII-alike Progress Bar](https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54) by [DanielSWolf](https://github.com/DanielSWolf) is licensed under the [MIT license](https://opensource.org/licenses/MIT)
