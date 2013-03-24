Version number semantics:
*************************

The current version number is kept in version.txt, of the form

major.minor.build.revision

As is common with .net projects, it works as follows:

major: increments as very significant changes to the client take place.  Due to the importance of backwards compatibility with Errordite server, it is unlikely this will be incremented very often.

minor: increments with significant changes to the client.  All changes to the API interface should result in an increment in major or minor.  Resets when major is incremented.

build: increments continually (except when revision is incremented).  Never resets.  All packages with any sort of change should result in an increase to build.

revision: very minor changes.  Probably only emergency bugfixes to a build.  Not clear when this would be used in reality.  Resets when any other counter is incremented.


Creating a package:
**********************

Creating a package is done with one of the build-increment-*.cmd files.  This will change the version number and create a release build in the builds directory.

The AssemblyVersion is set to major.minor.  This means any build / revision with the same version number can be dropped into an application directory.  This implies that any changes to any interface need to result in a major or minor increment.

The AssemblyFileVersion is set to major.minor.build.revision - this gives traceability without effecting compatibility.


Developer builds:
*****************

Developer builds via VS have an AssemblyVersion of 0.0.0.0 and an AssemblyFileVersion of "dev".  They are not ilmerged and hence cannot function as a direct swap for a packaged build.


Debug builds:
*************

Running build-debug-preserve-version.cmd creates a debug build with the same version number.  It gets put in the builds directory in a debug folder.  Keeping the version number allows for a simple swap to aid debugging within the host application.