# Issue creation/alteration guide

## For testers/users
Currently issues have 2 templates:
* Bug report
* Feature requests

Please fill the template as precisely as possible to ensure that it will be taken into development.

## For testers
If there is a label "for testing" on an issue, the issue should be fixed in the latest version for testing. See "<> Code" and "Releases" for latest pre-release.
## Issue lifecycle
1. Issue is created;
2. Issue is reviewed by project manager if it will be taken into development, which means issue is assigned a milestone and a developer;
3. When issue is taken into development, it should be put a label "in development".
4. When issue is fixed by developer in pre-release version, it should be assigned a stage "in testing".
5. If tester cannot recreate an issue/confirms that feature request is working -> "fixed" state.
6. Issue is closed when its published/pushed to master branch.

### Issue label categories
**Platform**: OSX, Android, Windows, iOS, Embedded

**Improvements** feature request, optimization

**Problem** bug

**Feedback** question, draft

**Stage** in development, in testing, fixed

**Other** wontfix, duplicate
