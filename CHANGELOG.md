# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) 
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
### Added
- Added hiding un-supported GPUs in the SelectGPU form which is enabled by default.
- Option to create a minimal installer of the drivers, so we don't install telemetry and other things we don't need. Requires WinRAR for now.
- No longer downloads driver if it exists.

### Changed
- Improved the driver description fetcher and the download message box.
- Now following the [CA1709](https://msdn.microsoft.com/en-us/library/ms182240.aspx) standard by Microsoft, and "migrated" to VS2017.

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
