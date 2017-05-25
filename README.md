# SF-deactivate-statesave-issue

## Problem
A save of actor state from `OnDeactivateAsync()` fails silently.

## Background
In our commercial project, we typically save state after each actor method invocation, which we understand to be best-practice.  However, a small set of method invocations occur very frequently and we strongly prefer to not save state within these methods (we accept the possibility of data loss).  It is _usually_ the case that another state-saving method invocation occurs prior to actor deactivation, but we must engineer for the possibility that such an invocation may not occur.

## Steps to Reproduce
* Run the project
* Watch "Diagnostics Events" view, see that the save of value "2112" on actor de-activation doesn't work (fails silently).

## Notes
* environment: VS 2015 enterprise, SF SDK 2.5, SF RT 5.5
* this repo is based on standard service-fabric reliable-actor-project creation from VS 2015 enterprise.  The second commit contains the code which demonstrates the issue.
