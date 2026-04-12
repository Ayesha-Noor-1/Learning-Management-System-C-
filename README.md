# Learning Management System (C# Console)

A **.NET console** application for a small institute scenario (**KICSIT Management System**): public information, then separate **student**, **teacher**, and **admin** portals. Data is stored in **text files** under your user **Documents** folder so builds in Visual Studio do not overwrite your saved data.

## Requirements

- [.NET SDK 10](https://dotnet.microsoft.com/download) (or the version matching `TargetFramework` in `LMS-C#/LMS-C#.csproj`)
- **Visual Studio 2022** (recommended) or any editor with `dotnet` CLI
- Windows (paths and console behavior are tested for typical Windows terminals)

## How to run

### Visual Studio

1. Open `LMS-C#.slnx` (or open the folder containing this repository).
2. Set **LMS-C#** as the startup project if prompted.
3. Press **F5** (debug) or **Ctrl+F5** (run without debugging).

### Command line

```powershell
cd LMS-C#
dotnet run
```

## Features

| Area | Capabilities |
|------|----------------|
| **Guest / main menu** | Faculty list, courses, admission information, sign-in |
| **Student** | Attendance, notices, timetable, quiz, quiz results |
| **Teacher** | Timetable, quiz authoring, quiz results, mark and view attendance |
| **Admin** | Add/remove students, notices, add teachers, edit timetable, return to main menu |

## Data and configuration

- On first run, the app creates **`%USERPROFILE%\Documents\KICSITData\`** (if needed) and **copies seed `.txt` files** from the build output **only when a file is missing** there. After that, your data in `KICSITData` is left in place.
- Seed copies live under the `LMS-C#` project and are copied to the output directory at build time (`CopyToOutputDirectory`).

## UI library

The console UI uses **[Spectre.Console](https://spectreconsole.net/)** for panels, selection menus, prompts, tables, and basic styling (not the default plain black-and-white flow).

## Project layout

```text
LMS-C#/                 ← C# project and seed data files
  Program.cs            Entry point
  AppPaths.cs           Paths under Documents\KICSITData
  GeneralView.cs        Landing menu
  AuthManager.cs        Sign-in
  Student.cs            Student portal
  Teacher.cs            Teacher portal
  Admin.cs              Admin portal
  UI.cs                 Spectre-based presentation helpers
  *.txt                 Seed data (logins, timetable, etc.)
README.md               This file
LMS-C#.slnx             Solution
```

## Troubleshooting

### Build error: cannot copy `LMS-C#.exe` — file in use

The application is still running and locking the output **`.exe`**. Stop debugging (**Shift+F5**), close the console window, then build again. If needed, end the **LMS-C#** process in Task Manager.

### Sign-in fails after adding users in Admin

Credentials are read from the same **`KICSITData`** files the admin writes to. If you edit files by hand, keep the format: **one line username, next line password**, repeating for each account.

## Repository

Upstream project: [Learning-Management-System-C-](https://github.com/Ayesha-Noor-1/Learning-Management-System-C-)

## License

No license file is included in this repository. Add one if you intend to distribute or reuse the code beyond coursework.
