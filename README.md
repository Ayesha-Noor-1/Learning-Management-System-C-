# Learning Management System (C# Console)

A **.NET console** application for a small institute scenario (**KICSIT Management System**): public information, then separate **student**, **teacher**, and **admin** portals.

The project uses **hybrid persistence**: part of the data is in **MongoDB**, and part stays in **text files** under your user **Documents** folder (`KICSITData`) so you can demonstrate both a database layer and traditional file I/O in one assignment.

## Requirements

- [.NET SDK 10](https://dotnet.microsoft.com/download) (or the version matching `TargetFramework` in `LMS-C#/LMS-C#.csproj`)
- **MongoDB** (local install or [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)) reachable from your machine
- **Visual Studio 2022** (recommended) or any editor with `dotnet` CLI
- Windows (paths and console behavior are tested for typical Windows terminals)

## How to run

1. Make **MongoDB** reachable: either **[MongoDB on your PC (localhost)](#mongodb-on-your-pc-localhost)** or **[MongoDB Atlas (step-by-step)](#mongodb-atlas-step-by-step)** below.
2. Edit `LMS-C#/appsettings.json` if needed (`MongoDB:ConnectionString`, `MongoDB:DatabaseName`). Defaults are **local** `mongodb://localhost:27017` and database **`lms_database`**. Override with **`MONGODB_CONNECTION_STRING`** / **`MONGODB_DATABASE_NAME`** if you prefer not to commit Atlas URIs.
3. Run the app.

### MongoDB on your PC (localhost)

**Why use this:** No internet account required, no Atlas IP rules, no `bad auth` for cloud users. Good for labs and offline work.

**Why it “didn’t work” before:** `mongodb://localhost:27017` only works if the **MongoDB server process** is installed and **running** on your machine. `ECONNREFUSED` means nothing is listening on that port.

1. Install **[MongoDB Community Server](https://www.mongodb.com/try/download/community)** for Windows.
2. During setup, enable **“Install MongoDB as a Service”** so it starts automatically.
3. Confirm it is running: **Services** (`services.msc`) → **MongoDB Server** → **Running**.
4. In **Compass**, connect with: `mongodb://localhost:27017` (or `mongodb://localhost:27017/lms_database` — see note below).
5. In **`LMS-C#/appsettings.json`**, use (default in repo):

   - `"ConnectionString": "mongodb://localhost:27017"`
   - `"DatabaseName": "lms_database"`

   The C# driver opens the server from the connection string, then selects the database named in **`DatabaseName`** (`LmsDatabase.InitializeAsync()` → `GetDatabase(dbName)`). You do **not** have to put `lms_database` in the URI path unless you want to; the app uses **`DatabaseName`** for all collections.

6. Run the LMS app (`dotnet run` or F5). Collections appear under **`lms_database`** after the first successful start.

**Local vs Atlas:** Same code paths; only the connection string and database name change. Use Atlas when you cannot install MongoDB locally or need a shared cloud database.

### MongoDB Atlas (step-by-step)

Do these in order. If **MongoDB Compass** shows `bad auth : authentication failed`, the username or password in the URI does not match a **database user** in Atlas (not your Atlas website email/password).

#### 1. Log in and open your project

1. Go to [https://cloud.mongodb.com](https://cloud.mongodb.com) and sign in.
2. Open your **Organization** → **Project** (e.g. “Project 0”).
3. Open **Clusters** and select **Cluster0** (or your cluster name).

#### 2. Make sure the cluster is running

- If you see **“paused”**, click **Resume** and wait until the cluster finishes starting (often 1–3 minutes).

#### 3. Create a database user (this is what goes in the connection string)

1. In the left sidebar, under **SECURITY**, click **Database Access**.
2. Click **Add New Database User** (or select an existing user to **Edit** / **Reset password**).
3. Choose **Password** authentication. Pick a **username** (e.g. `AyeshaNoor`) and a **password** (write it down).
4. Under **Database User Privileges**, for coursework you can use **“Built-in Role”** → **`readWriteAnyDatabase`** (or at least read/write on the database you use, e.g. `KICSITLMS`).
5. Click **Add User** (or **Update User**).

Use **this** username and password in Compass and in `appsettings.json`. Your **Atlas account email/password** is only for logging into the website, not for the URI.

#### 4. Allow your computer to connect (network)

1. Left sidebar **SECURITY** → **Network Access**.
2. **Add IP Address** → **Add Current IP Address** (recommended), or for a shared class demo only, **Allow Access from Anywhere** (`0.0.0.0/0`) if your instructor allows it.
3. Wait until the new rule shows as **Active** (can take a minute).

#### 5. Copy the official connection string from Atlas

1. Go back to **Clusters** → your cluster → click **Connect**.
2. Choose **Compass** (to test) or **Drivers** (for the exact string).
3. Pick **C#** / **Compass** as guided, then **copy** the connection string. It looks like:  
   `mongodb+srv://<username>:<password>@cluster0.xxxxx.mongodb.net/...`
4. Replace `<password>` with the **database user** password from step 3. If the password contains characters like `@ # : / ?`, you must **URL-encode** them in the URI (e.g. `@` → `%40`). Easiest fix: use a password with only letters and numbers for testing.

#### 6. Test in MongoDB Compass first

1. Open **MongoDB Compass** → **New connection**.
2. Paste the full **`mongodb+srv://...`** URI (with real user and password, no angle brackets).
3. Optional but useful: add **`/KICSITLMS`** before the `?` if your string has no database path, e.g.  
   `...mongodb.net/KICSITLMS?retryWrites=true&w=majority&appName=Cluster0`  
   so data lands in the same database name the app uses (`DatabaseName` in `appsettings.json`).
4. Click **Connect**. If you still see **`bad auth`**, go back to **Database Access** and **reset the database user password**, update the URI, and try again.

#### 7. Point this C# project at the same URI

1. Open **`LMS-C#/appsettings.json`** in your repo.
2. Set **`MongoDB:ConnectionString`** to the **exact same** string that succeeded in Compass (same user, same encoded password, same host).
3. Set **`MongoDB:DatabaseName`** to **`KICSITLMS`** (or the name you used in the URI path — they should match).
4. Save the file, rebuild the solution, then run.

The app connects in **`Program.cs`** by calling **`LmsDatabase.InitializeAsync()`**, which reads `appsettings.json` (or the **`MONGODB_CONNECTION_STRING`** environment variable if set).

#### 8. Run the LMS app

Use Visual Studio **F5** / **Ctrl+F5**, or from a terminal:

```powershell
cd LMS-C#
dotnet run
```

If MongoDB is unreachable, the console shows a short error and exits before the main menu.

### Visual Studio

1. Open `LMS-C#.slnx` (or open the folder containing this repository).
2. Set **LMS-C#** as the startup project if prompted.
3. Press **F5** (debug) or **Ctrl+F5** (run without debugging).

### Command line

```powershell
cd LMS-C#
dotnet run
```

### Unit tests

From the repo root (or from `LMS-C#` using the path below):

```powershell
cd LMS-C#
dotnet test LMS-C#.Tests\LMS-C#.Tests.csproj
```

The test project lives under `LMS-C#/LMS-C#.Tests/`; the main app project excludes its `.cs` files so they are not compiled into the executable.

## Features

| Area | Capabilities |
|------|----------------|
| **Guest / main menu** | Faculty list, courses, admission information, sign-in |
| **Student** | Attendance, notices, timetable, take quiz (**attempts stored in MongoDB**), **My quiz results** (your attempts only) |
| **Teacher** | Timetable, quiz authoring, **all students’ quiz attempts** (MongoDB table), optional legacy **`quizresult.txt`** block, **Manage quiz & results** (clear MCQ file / clear Mongo attempts), mark and view attendance |
| **Admin** | Add/remove students, notices, add teachers, edit timetable, **Statistics** (counts + Spectre summary), return to main menu |

## Object-oriented design (where to look)

| Concept | How it appears in this project |
|--------|---------------------------------|
| **Abstraction** | `IPortal` (any signed-in area), `ILoginValidator`, `IQuizResultWriter` — callers depend on contracts, not concrete types. |
| **Encapsulation** | `PortalBase` keeps `_username` private and exposes `DisplayUsername`; validators and writers hide storage details. |
| **Inheritance** | `Student`, `Teacher`, and `Admin` inherit `PortalBase` and supply role-specific menus and actions. |
| **Polymorphism** | `AuthManager` opens an `IPortal` after sign-in; `ILoginValidator` is implemented by `MongoCredentialValidator`; `IQuizResultWriter` / `FileQuizResultWriter` remain in the codebase as an optional legacy pattern (teachers can still view **`quizresult.txt`** alongside Mongo attempts). |
| **Template method** | `PortalBase.Run()` defines the menu loop; subclasses override abstract hooks (`DrawRoleHeader`, `BuildMenuPrompt`, `ExecuteSelection`, …). |
| **Virtual methods** | `PortalBase.BeforeMenuIteration` and `WriteWelcomeLine` can be overridden; accent colors use abstract `GetWelcomeAccentOpening` / `Closing`. |
| **Sealed classes** | `Student`, `Teacher`, `Admin`, `MongoCredentialValidator`, and `FileQuizResultWriter` are `sealed` to prevent unintended further subclassing. |
| **Composition** | Portals call **`QuizAttemptRepository`** and other static repositories for persistence; UI concerns stay in **`UI`** helpers. |
| **Method overloading** | `InputValidation.IsMeaningful(string?)` vs `IsMeaningful(string?, int maxLength)`. |

## Hybrid storage (MongoDB + files)

| Stored in **MongoDB** | Stored in **files** (`Documents\KICSITData\`, seeded from the project) |
|------------------------|------------------------------------------------------------------------|
| User accounts and **hashed** passwords (student / teacher / admin) | **Timetable** (`timetableCS-1A.txt`) |
| **Students** roster fields + link to login user | **Attendance** (`attendanceCS-1A.txt`) |
| **Faculty** directory (name, department, email) + link to teacher login | **Quiz questions** (`quizmakeCS1-A.txt`) |
| **Notices** | |
| **Quiz attempts** (per user: marks, duration, timestamp; collection **`quizAttempts`**) | **Legacy** **`quizresult.txt`** (seeded; teachers may view/clear alongside Mongo) |
| **Courses** and **admission** text shown on the main menu | |

**Why hybrid:** MongoDB fits structured entities (users, roles, relations, notices, quiz attempts). Files stay simple for line-based content (timetable editing, quiz question format) without over-scaffolding the assignment.

**First run:** The app creates `KICSITData`, copies missing seed `.txt` files from the build output, connects to MongoDB, and **seeds the database from those files only when the corresponding MongoDB collections are empty**. After that, sign-in and admin roster changes use **MongoDB**; the old `login*.txt` files are no longer the source of truth for authentication (they remain useful as seed templates).

## Data and configuration

- **MongoDB:** `appsettings.json` in the project (copied to output). Connection failures show a short message at startup.
- **Files:** On first run, the app creates **`%USERPROFILE%\Documents\KICSITData\`** (if needed) and **copies seed `.txt` files** from the build output **only when a file is missing** there. After that, file-backed data in `KICSITData` is left in place.
- Seed copies live under the `LMS-C#` project and are copied to the output directory at build time (`CopyToOutputDirectory`).

## UI library

The console UI uses **[Spectre.Console](https://spectreconsole.net/)** for panels, selection menus, prompts, tables, and basic styling (not the default plain black-and-white flow).

## Project layout

```text
LMS-C#/                 ← C# project and seed data files
  LMS-C#.Tests/         xUnit tests (separate csproj; friend assembly LMS.Tests)
  Program.cs            Entry point (initializes files + MongoDB)
  AppPaths.cs           Paths under Documents\KICSITData (file-backed + seed sources)
  Oop/                  Interfaces + abstract base (IPortal, PortalBase, validators, …)
  Data/                 MongoDB access, repositories, initializer
  Models/               BSON documents for MongoDB
  appsettings.json      MongoDB connection (copied to output)
  GeneralView.cs        Landing menu
  AuthManager.cs        Sign-in (MongoDB)
  Student.cs            Student portal
  Teacher.cs            Teacher portal
  Admin.cs              Admin portal
  UI.cs                 Spectre-based presentation helpers
  *.txt                 Seed data (some only seed Mongo; others used at runtime)
README.md               This file
LMS-C#.slnx             Solution
```

## Troubleshooting

### Build error: cannot copy `LMS-C#.exe` — file in use

The application is still running and locking the output **`.exe`**. Stop debugging (**Shift+F5**), close the console window, then build again. If needed, end the **LMS-C#** process in Task Manager.

### Cannot connect to MongoDB

Ensure `mongod` is running locally **or** (for Atlas) your **Network Access** IP is allowed and the **connection string** is correct. Check `appsettings.json` or `MONGODB_CONNECTION_STRING`.

### Compass or app: `bad auth : authentication failed`

- Use the **Database user** from **Database Access**, not your Atlas login email.
- **Reset password** for that user in Atlas, paste the new password into the URI, and URL-encode special characters if needed.
- Confirm **Network Access** includes your current IP (or `0.0.0.0/0` if you chose that).

### Sign-in fails after adding users in Admin

New accounts are stored in **MongoDB**, not in `loginstd.txt` / `logintea.txt`. If you reset the database, the app may re-seed users from those text files on the next empty-database startup.

## Repository

Upstream project: [Learning-Management-System-C-](https://github.com/Ayesha-Noor-1/Learning-Management-System-C-)

## License

No license file is included in this repository. Add one if you intend to distribute or reuse the code beyond coursework.
