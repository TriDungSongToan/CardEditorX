# Registry Key

## Overview

CardEditor X can integrate with Windows File Explorer by registering supported
file types.

After registration, supported files can be opened directly with CardEditor X
or selected from the **Open with** menu.

### Supported File Types

| File type | Integration                  |
| --------- | ---------------------------- |
| `.ypk`    | Double-click / Open directly |
| `.cdb`    | Double-click / Open directly |
| `.ceds`   | Double-click / Open directly |
| `.lua`    | Double-click / Open directly |
| `.ydk`    | Double-click / Open directly |
| `.zip`    | Open with                    |
| `.db`     | Open with                    |
| `.sqlite` | Open with                    |
| `.xlsx`   | Open with                    |
| `.txt`    | Open with                    |
| `.md`     | Open with                    |
| `.log`    | Open with                    |
| `.yml`    | Open with                    |
| `.conf`   | Open with                    |

> **Note**
>
> Files listed as **Double-click / Open directly** can be associated with
> CardEditor X for direct opening from Windows File Explorer.
>
> Files listed as **Open with** are only added to the Windows **Open with**
> list. CardEditor X does not automatically become their default application.

---

## Register CardEditor X

Registering CardEditor X adds the supported file types to Windows File Explorer.

### Step 1: Create Registry Key File

In the Main Window interface, click:
```text
Help -> Registry -> Register Registry
```
Choose a location to save the Registry file, then click Save.
CardEditor X will create a .reg file at the selected location.

### Step 2: Import the Registry File

Locate the .reg file created in Step 1 and double-click it.

Windows will display a security confirmation. Select Yes to allow Windows to add the Registry information.

After the Registry file has been imported successfully, CardEditor X will be registered with Windows.

Note: You only need to import the generated .reg file once. After successful registration, you can delete this .reg file.

## UnRegister CardEditor X

To remove CardEditor X from Windows file associations, use the Unregister Registry option in:

Help -> Registry -> Unregister Registry

This removes the file-association settings created by CardEditor X.

Unregistering CardEditor X does not uninstall or remove the application.

---

[← Previous: Getting Started](GettingStarted.md)