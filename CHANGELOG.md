# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) 
and this project adheres to [Semantic Versioning](http://semver.org/).

## [1.14.1] - 2020-05-15
### Fixed
- Not launching extracted drivers when using minimal installer

## [1.14.0] - 2020-05-15
I am back, sorry for being inactive the past years. I've been very busy with GTA related projects for years until recently where I got dropped out, and now focusing on my own side projects. I hope you all enjoy this new version!
### Added
- Download selection form, you can now select the download server. Hopefully this resolves issues with slow downloads
- Compare HAP versions, making sure downloaded one is the correct one (which also prevents further errors)
- We now run the NVIDIA installer with the `/noreboot` if the application is running quiet to prevent random reboots
- Support for DCH drivers thanks to [Osspial](https://github.com/Osspial)

### Changed
- Updated HAP to v1.11.23
- Download size in bytes are now properly grouped
- Use string interpolation where available
- Minor code improvements
- Improved tooltips on the driver dialog
- Disabled keyboard shortcuts in driver dialog

### Fixed
- Missing icon files for the forms
- Minor typo
- You can no longer navigate to other websites from the release description
- Driver installer no longer hardcoded to run quiet if minimal installer is used
- Replaced `/noeula` argument with `/nosplash` argument when minimal installer isn't used, which caused Gf Experience to install

## [1.13.0] - 2018-06-06
### Added
- QuickEdit is now disabled, no more accidentally stalling the application

### Changed
- Minor message when hash doesn't match
- Updated HAP to v1.8.4

### Fixed
- Minimal installer not extracting new required EULA files [#41](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/issues/41) - thanks [cywq](https://github.com/cywq)

## [1.12.0] - 2018-03-09
### Added
- Implemented checks for the MD5 hash for the HAP dll file. This is to make so that people don't use a wrong version of HAP, since TNUC only previously checkes for the file name. Now we are also checking the hash which means that we are comparing the entire file. This should fix all the dll hell trouble, sorry for the late fix :/

### Changed
- Moved class files
- Updated HAP to v1.7.1

### Fixed
- An issue where the file is still being used by TNUC after the drivers have been downloaded. This was the cause of [#26](https://github.com/ElPumpo/TinyNvidiaUpdateChecker/issues/26), thanks a lot [indivisible](https://github.com/indivisible) for fixing the issue, credits to him!

## [1.11.0] - 2018-02-09
### Added
- `--ignore-missing-gpu` argument, will ignore the fact that no compatible gpu were found

### Changed
- Tool tips in the driver dialog
- Updated legal message

### Fixed
- Possible fix for 7-Zip extraction error
- Fixed update checker by updating to .NET 4.6.1 and HAP v1.6.16, thanks @dragondaud

## [1.10.0] - 2017-10-24
### Added
- Implemented a much better dialog for chosing directory
- Support for alternative locations for 7-Zip (Scoop, x86 install on amd64 systems, MSI installer and last resort)
- Check against vendor ID if the gpu drivers aren't installed
- Missing text

### Fixed
- Driver installer always run quietly, now only skipping eula
- Tiny text on none dpi-displays in the driver dialog

## [1.9.0] - 2017-09-25
### Added
- New fancy dialog when a new driver is available, with a much better GUI solution and better release notes preview
- Support for 7-Zip, and an improved libary handling system
- `--confirm-dl` argument. It will automaticly download and install new drivers! (uses minimal installer)
- `--config-here` argument. It uses the working directory as the path for the config
- Implemented check for driver file size

### Changed
- Updated HtmlAgilityPack to the latest version
- Replaced the `GPU Type` system with now assuming the type depending on the gpu name
- Changed back to the correct folder browser dialog
- A lot of cleanup (yet again) and refactoring
- Code improvements

### Removed
- You no longer have to select your gpu

## [1.8.0] - 2017-06-28
### Added
- Inform user to select a empty folder if using the minimal installer.
- Automatic download of `HtmlAgilityPack.dll` if not found.
- Network connection check.
- GUI progress bar that shows up upon the driver download, if the user has selected the quiet mode.

### Changed
- Went back to using the `SaveFileDialog`.
- Method renames to match [CA1709](https://msdn.microsoft.com/en-us/library/ms182240.aspx).
- A lot of cleanup.

## [1.7.0] - 2017-04-29
### Added
- Added hiding un-supported GPUs in the SelectGPU form which is enabled by default.
- Option to create a minimal installer of the drivers, so we don't install telemetry and other things we don't need. Requires WinRAR for now.
- No longer downloads driver if it exists.

### Changed
- Improved the driver description fetcher and the download message box.
- Now following the [CA1709](https://msdn.microsoft.com/en-us/library/ms182240.aspx) standard by Microsoft, and "migrated" to VS2017.
- Minor changes.

### Removed
- CTRL-C is no longer ignored.

## [1.6.0] - 2017-02-03
### Added
- Added fetching driver description.
- Added a new way to fetch current GPU version, by getting the GPU information provided by `Win32_VideoController`. No longer based on files which is an improvement.

### Fixed
- Fixed a bug where the config wasn't set correctly in the development enviroment.
- Fixed the date fetched from the NVIDIA servers, with an much improved system.

## [1.5.0] - 2016-11-16
Started focusing on cleaning up code that doesn't make sence, more to come.
### Added
- Added a last resort release notes link creator if none is displayed on Nvidias' website.
- A date has been added when a new update is available to show how long ago the latest driver was released with full date, and added the version number of new gpu driver.

### Changed
- Values fetched from the nvidia website now run through the `.Trim()` method for various reasons.
- A lot of cleanup and minor changes, for example all version numbers are now strings and not ints - it was a pain to read them and is now much better.

### Removed
- Removed old, unused code.

### Fixed
- Trying to open the .pdf release notes when no pdf reader is available will no longer crash the application.
- Fixed so that nothing happens before the intro, for example using the `--version` argument - and possible overlaps.

## [1.4.1] - 2016-10-23
### Added
- Progress bar for driver download, credit goes to https://github.com/DanielSWolf.
- Added a command argument, `--version` to get application version.

### Changed
- Changed the target file to get the current driver cause v375.57 removed `nvvsvc.exe`.
- CTRL+C inputs are now ignored.

### Fixed
- Fixed a issue related to using the `--quiet` command argument to run a silent window.

## [1.4.0] - 2016-10-12
Long time no see, or that's what they say at least. I have been busy being a typical student, I am trying to keep up dev but only have so much spare time to spare programming. As you see, the versions have been spaced a lot. I haven't made the same amount of changes in the code either lately as I used to do (when I had a lot of free time), but that doesn't mean that I am abandoning this project.

Might start pushing out beta versions if people are interested in that kind of stuff, thing is that I am really wondering if people are even using this application, so it might be for... nothing. And in that case, wouldn't this entire project be for nothing..? I don't have a clue of the userbase.

Whatever, a new version out lads.
### Added
- Added the argument `--force-dl` for a force download of gpu drivers even if the drivers are up-to-date.
- Option to view release PDF.

### Changed
- Command line argument handler has been improved, and the argument for erasing local config file has been changed to `--erase-config`.
- Some code has been moved around.
- The default download folder is now the users Downloads folder instead of the temp folder.
- A lot of minor code improvements.

### Fixed
- Some `Process.Start()` calls will no longer risk breaking the application.

## [1.3.0] - 2016-08-09
### Added
- Instead of just launching web browser where user has to load the NVIDA website in order to download the driver, the application now does it for you. Using SaveFileDialog to choose GPU driver save file.
- Basic documentation started.

### Changed
- Changes around config system, for example the key `Desktop GPU` has been renamed to `GPU Type`.
- Code 10% more bullet proof.
- Updated HTMLAgilityPack to v1.4.9.5, and therefore upped .NET framework target to 4.5.
- All released builds are now set to the "Release" build type in VS for better performance.

### Fixed
- Repairing a config key does no longer skip the request.

## [1.2.0] - 2016-07-10
### Added
- Missing author in assembly information.
- Support for mobile NVIDIA GPUs.
- New argument - `--eraseConfig`, erases configuration file.

### Changed
- Minor cleanup and improvements.
- Complete re-write of config system, had a great time doing so and had to be done - old system was wanky. Moved to `.xml` format powered by the .NET framework.
- Now following the Semantic Versioning 2.0.0 standard, http://semver.org/spec/v2.0.0.html.
- Improved update system.
- Improved command line argument handler.

### Fixed
- Solved issue where quiet mode wouldn't show any message boxes.

## [1.1.0.0] - 2016-05-31
### Added
- Check for required DLL file.
- Basic documentation.
- Language specific downloads.
- `--debug` command line argument used for debugging purposes.
- `--?` command line argument as help menu.

### Changed
- Major cleanup / improved code.
- Improved command argument system.

## [1.0.0.0] - 2016-05-20
The application is finally in version 1! Everything seems to work fine so I decided to release it as stable.
### Added
- Now using HtmlAgilityPack.
- Get remote version.
- Get remote driver link.

### Changed
- Minor cleanup.
- All GPU driver versions are now integers instead of being both a int and string.

### Fixed
- Icon modified yet again; fixed material design and more.

## [0.2.0.0] - 2016-05-15
### Added
- Added GPLv3 message.
- Added CHANGELOG.
- Added LICENSE.
- Emulate remote version via configuration file.

### Changed
- Renamed MainConsole.cs to mainConsole.cs.
- A lot of cleanup.
- Changes in configuration file; please delete your configuration file after updating.
- New icon as the old one was a registred trademark by NVIDIA Corporation, thanks to https://github.com/Maddoc42/Android-Material-Icon-Generator.

### Fixed
- Windows version detector fixed.
- Fixed using `-quiet` command line argument where application would not end.

## [0.1.0.0] - 2016-05-14 [YANKED]
Initial public release. Was yanked after the release of v0.2.0.0 because of the missing license (which is in the changelog for v0.2.0.0).
