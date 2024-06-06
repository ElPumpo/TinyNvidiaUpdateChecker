![TNUC Logo](image.png)

# TinyNvidiaUpdateChecker

TinyNvidiaUpdateChecker (TNUC for short) is a lightweight tool that checks for NVIDIA GPU drivers for Windows. You can configure TNUC [executing it when logging in](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/wiki/Quiet-runs-on-user-login), and have it only pop up if there's a new driver available.

It has a minimal install feature that extracts the GPU drivers from the otherwise bloated bundle which contains GeForce Experience, 3d drivers and other components. Nothing except the required drivers will be installed.

Uses a NVIDIA API to get the latest driver, and there is support for the majority of their GPUs. Thanks to [ZenitH-AT](https://github.com/ZenitH-AT) for their research and metadata repo used by this project.

## Dependencies

- Windows 10 or higher
- [.NET Desktop Runtime 8 x86](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Internet connection
- (optional) [WinRAR](https://www.rarlab.com/) or [7-Zip](http://www.7-zip.org) for minimal install feature

### .NET Desktop Runtime 8

If a prompt comes up to install .NET Runtime 8, then do not worry.

There is two ways you can install it. The easiest and first way is with a privileged command prompt.

`winget install Microsoft.DotNet.DesktopRuntime.8 --architecture x86`

Or

[Navigate to the download page](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) and select the __.NET Desktop Runtime x86__ download

## Installation

1. [Download the latest version](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases)
2. Start TinyNvidiaUpdateChecker.exe
3. (Optional) configure [execute when logging in](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/wiki/Quiet-runs-on-user-login)

If it does not run or closes after a split second then you did not install the required runtime properly, see above.

### Install with [Scoop](https://scoop.sh/#/apps?s=2&d=1&o=true&p=1&q=tinynvidiaupdatechecker)

```
scoop bucket add extras
scoop install tinynvidiaupdatechecker
```

## How to use

Be sure to [check out the wiki](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/wiki) to learn more about how to use TNUC.

## Alternatives

- [EnvyUpdate](https://github.com/fyr77/EnvyUpdate)
- [nvidia-update](https://github.com/ZenitH-AT/nvidia-update)

## Legal

### License

TinyNvidiaUpdateChecker - Check for NVIDIA GPU driver updates!

Copyright (C) 2016-present Hawaii_Beach

This program is free software: you can redistribute it And/Or modify it under the terms Of the GNU General Public License As published by the Free Software Foundation, either version 3 Of the License, Or (at your option) any later version.

This program is distributed In the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty Of MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License For more details.

You should have received a copy Of the GNU General Public License along with this program. If Not, see <http://www.gnu.org/licenses/>.

### Project icon

[Project icon](https://github.com/Maddoc42/Android-Material-Icon-Generator) by [Maddoc42](https://github.com/Maddoc42) is licensed under [CC BY-NC 3.0](https://creativecommons.org/licenses/by-nc/3.0/)

### HTML Agility Pack

HTML Agility Pack by [zzzprojects](https://github.com/zzzprojects/html-agility-pack) is licensed under the [MIT license](https://opensource.org/licenses/MIT)

### ASCII-alike Progress Bar

[ASCII-alike Progress Bar](https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54) by [DanielSWolf](https://github.com/DanielSWolf) is licensed under the [MIT license](https://opensource.org/licenses/MIT)
