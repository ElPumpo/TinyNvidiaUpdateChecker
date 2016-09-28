# Change Log
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) 
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
### Added
- Added the argument "--force-dl" for a force download of gpu drivers even if the drivers are up-to-date.
- Option to view release PDF.

### Changed
- Some code has been moved around.
- The default download folder is now the users Downloads folder instead of the temp folder.
- A lot of minor code improvements.

### Fixed
- Some `Process.Start()` will no longer risk breaking the application.

## [1.3.0] - 2016-08-09
### Added
- Instead of just launching web browser where user has to load the NVIDA website in order to download the driver, the application now does it for you. Using SaveFileDialog to choose GPU driver save file.
- Basic documentation started.

### Changed
- Changes around config system, for example the key "Desktop GPU" has been renamed to "GPU Type".
- Code 10% more bullet proof.
- Updated HTMLAgilityPack to v1.4.9.5, and therefore upped .NET framework target to 4.5.
- All released builds are now set to the "Release" build type in VS for better performance.

### Fixed
- Repairing a config key does no longer skip the request.

## [1.2.0] - 2016-07-10
### Added
- Missing author in assembly information.
- Support for mobile NVIDIA GPUs.
- New argument - "--eraseConfig", erases configuration file.

### Changed
- Minor cleanup and improvements.
- Complete re-write of config system, had a great time doing so and had to be done - old system was wanky. Moved to '.xml' format powered by the .NET framework.
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
- "--debug" command line argument used for debugging purposes.
- "--?" command line argument as help menu.

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
- Fixed using "-quiet" command line argument where application would not end.

## [0.1.0.0] - 2016-05-14 [YANKED]
Initial public release.
