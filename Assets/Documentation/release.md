# Release process of Voyager Controller
## Overview of release process
1. Pre-release
2. Testing and Q&A
3. Update manual if any changes are to be made
4. Update release notes
5. Publish to App store
6. Publish to Play store
7. Update desktop software links

## Pre-release
Release candidates are published to the [releases page](https://github.com/digitalsputnik/DS-Voyager-controller/releases). Pre-release must include the following:
1. links to all supported platform builds (iOS, Android, Windows, OSX), currently builds are buing made on [Unity Cloud build](https://developer.cloud.unity3d.com/);
2. short release notes on what has changed from the previous version;
3. branch it was build from (under Target);
4. add link test sheet (on Google Drive) for the current release;
5. checkbox for Pre-release until it is approved by Q&A.

## Testing and Q&A
Testing consists of three phases:
1. Testing of new features - since new features;
2. Filling out test sheet ([link to template](https://docs.google.com/spreadsheets/d/14ZKBtwgfpKyoVK-KiNli-We00_-AXu-2eqRtX73tk0I/edit?usp=sharing)), which consists mostly of current features that should work;
3. Creative testing - trying to discover issues which do not fall under previous two categories.

If there are issues in the pre-release versions, then triage will be applied to the issues to decide on whether:
- issue is a blocker to release, which means that issue must be fixed immediately and a new pre-release is needed in order to make it to release;
- issue is important but it doesn't hinder the user experience as much, which means it will be fixed with the next release;
- issue doesn't come up very often and doesn't interrupt workflow in most use cases.

## Update manual
Voyager manual update has two steps:
- update [manual on Google Drive](https://docs.google.com/document/d/1OiDbgm5k9GuhUGBgvsNy_xqKdvXaRmXyepqSi5CgNOg/edit);
- send the manual to Taavi for designing and uploading.

## Update release notes
Release notes are held on [Digital Sputnik's support page](https://www.digitalsputnik.com/pages/support). Which means to update you need editing rights to the web page. Release notes are to be collected from pre-release notes (new features) and issues (known bugs).

## Publish to App store
Publishing to App Store consists of following steps:
1. Uploading ipa file from cloud build to App store. This can be done with [Transporter app](https://transportergui.com/).
2. Publishing app to TestFlight on [App Store Connect](https://appstoreconnect.apple.com/) (under My Apps -> DS Voyager Controller -> TestFlight).
3. Creating a new release on [App Store Connect](https://appstoreconnect.apple.com/) (under My Apps -> DS Voyager Controller -> blue "+" sign next to iOS App).
4. Copy release notes and if needed, cut it shorter to meet character limit criteria.
5. Before submitting it is recommended to check "Manually release this version" to control the release time with other platforms.

## Publish to play store
Publishing to Play Store consists of following steps:
1. On [Google Play Console](https://play.google.com/apps/publish) upload cloud build .aab file. This can be done from DS Voyager Controller -> Release management -> App releases -> Manage (from Production Track tab) -> Create Release.
2. Check version.
3. Copy release notes and if needed, cut it shorter to meet character limit criteria.
4. Review and publish if other platforms are ready to be published as well (usually depends on App Store release).

## Update desktop software links
The same desktop software links (OSX and Windows) that are on [releases page](https://github.com/digitalsputnik/DS-Voyager-controller/releases) should be updated on [Digital Sputnik's support page](https://www.digitalsputnik.com/pages/support).
