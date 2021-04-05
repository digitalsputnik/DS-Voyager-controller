# Issue creation/alteration guide

## For testers/users
Currently issues have 2 templates:
* Bug report
* Feature requests

Please fill the template as precisely as possible to ensure that it will be taken into development. If you currently don't have all the information but would like to inform the development team, assign a "draft" label to the issue. Also please remove fields that cannot be filled so the developer reading it would understand the core issue faster.

## For testers
If the issue is in "for testing" column in the [project page](https://github.com/digitalsputnik/DS-Voyager-controller/projects/4), the issue should be fixed in the latest version for testing. See "<> Code" and "Releases" for latest pre-release.

## Issue lifecycle
1. Issue is created;
2. Issue is reviewed by project manager if it will be **taken into development = issue is assigned a milestone**;
3. Issues in the current sprint will be moved to the [project page](https://github.com/digitalsputnik/DS-Voyager-controller/projects/4) by project manager.
4. When issue is taken into development, it should be moved to column "in development".
5. When issue is pushed to repository, it should be moved to column "fix pushed" by developer.
6. When issue is fixed by developer in pre-release version, it should be moved to column "in testing" by developer/publisher.
7. If tester cannot recreate an issue/confirms that feature request is working -> move the issue to "fixed" column.
8. Issue is closed/"done" when its published/pushed to master branch.

### Issue label categories
**Platform**: OSX, Android, Windows, iOS, Embedded

**Improvements** feature request, optimization

**Problem** bug

**Feedback** question, draft

**Other** wontfix, duplicate
