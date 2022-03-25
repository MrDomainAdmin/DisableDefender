# DisableDefender

**Code still works but requires Tamper Protection to be disabled:
Virus and threat detection settings -> Manage Settings -> Tamper Protection off. I have not found a way to do this via registry/GPO.

C# Project designed to disable defender for testing workstations. Could potentially be used during an engagement to disable defender if you are already in a privileged context.

Leverages code borrowed from https://bitbucket.org/MartinEden/local-policy/src/default/ to modify group policy settings to disable and enable windows defender functionality.


**Requires privileged context
```
Usage:
	DisableDefender.exe
		Will disable defender

	DisableDefender.exe --clean
		Will re-enable defender
```