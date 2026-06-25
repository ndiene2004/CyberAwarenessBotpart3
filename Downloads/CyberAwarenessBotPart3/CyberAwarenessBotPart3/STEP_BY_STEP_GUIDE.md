# 🛡️ CyberAwarenessBot – Part 3: Step-by-Step Visual Studio 2022 Guide
### Digitra Solutions | Author: Kutlwano

---

## 📋 WHAT YOU ARE BUILDING

Part 3 adds four advanced features to the existing chatbot GUI:

| # | Feature | Description |
|---|---------|-------------|
| 1 | **Task Assistant** | Add, view, mark done, and delete cybersecurity tasks |
| 2 | **Cybersecurity Quiz** | 12-question interactive quiz with scoring and feedback |
| 3 | **NLP Simulation** | Keyword detection that understands different phrasings |
| 4 | **Activity Log** | Records every action the bot takes |

---

## 🔧 PREREQUISITES

Before starting, make sure you have:
- ✅ Visual Studio 2022 (Community, Professional, or Enterprise)
- ✅ **.NET desktop development** workload installed (includes WPF)
- ✅ Your Part 2 project (or start fresh — this guide covers both)

> **To check/install the workload:**  
> Open Visual Studio Installer → Modify → tick ".NET desktop development" → Modify

---

## STEP 1 – CREATE THE PROJECT

### Option A: You have Part 2 working
1. Open your Part 2 solution in Visual Studio 2022.
2. Skip to **Step 3** — you will *add to* your existing files.

### Option B: Starting fresh
1. Open **Visual Studio 2022**.
2. Click **"Create a new project"**.
3. Search for **WPF Application** → select the C# one → click **Next**.
4. Set:
   - Project name: `CyberAwarenessBot`
   - Location: your desktop or project folder
   - Solution name: `CyberAwarenessBotPart3`
5. Click **Next**.
6. Select **.NET 8.0** (or .NET 6.0 if 8 isn't available) → click **Create**.

Visual Studio will scaffold `MainWindow.xaml` and `MainWindow.xaml.cs` for you.

---

## STEP 2 – UNDERSTAND THE FILE STRUCTURE

After creating the project, Solution Explorer should show:

```
CyberAwarenessBotPart3/          ← Solution folder
│
├── CyberAwarenessBotPart3.sln   ← Solution file (double-click to open)
│
└── CyberAwarenessBot/           ← Project folder
    ├── CyberAwarenessBot.csproj ← Project settings
    ├── App.xaml                 ← Application startup config
    ├── App.xaml.cs              ← App code-behind
    ├── MainWindow.xaml          ← THE GUI (all four tabs)
    └── MainWindow.xaml.cs       ← THE LOGIC (chatbot + quiz + tasks + log)
```

**Important:** Every `.xaml` file has a matching `.xaml.cs` code-behind file.
You will be editing **MainWindow.xaml** and **MainWindow.xaml.cs**.

---

## STEP 3 – REPLACE MainWindow.xaml (THE GUI)

### 3a. Open the XAML file
In Solution Explorer, double-click `MainWindow.xaml`.
Visual Studio shows the **XAML editor** (not the designer – we edit code directly).

### 3b. Select all and replace
1. Press `Ctrl + A` to select all existing code.
2. Press `Delete`.
3. Paste the **entire contents of `MainWindow.xaml`** from the provided file.

### 3c. What the XAML creates (explained simply)

```
Window
└── Grid (3 rows: header, tabs, status bar)
    ├── Border (header banner – dark blue background)
    │
    ├── TabControl (4 tabs)
    │   ├── Tab 1: "💬 Chat"
    │   │   ├── ScrollViewer + TextBox  (chat history – read-only)
    │   │   ├── TextBox + Button        (user input + Send)
    │   │   └── WrapPanel               (quick-action buttons)
    │   │
    │   ├── Tab 2: "📋 Tasks"
    │   │   ├── GroupBox                (add-task form: title, description, date picker)
    │   │   ├── ListView                (task list with columns)
    │   │   └── StackPanel             (Mark Done, Delete, Refresh buttons)
    │   │
    │   ├── Tab 3: "🎮 Quiz"
    │   │   ├── Border                  (score display)
    │   │   ├── Border + TextBlock      (question text)
    │   │   ├── StackPanel + 4 RadioButtons (answer choices A-D)
    │   │   ├── Border + TextBlock      (feedback after answering)
    │   │   └── StackPanel             (Start, Next, Restart buttons)
    │   │
    │   └── Tab 4: "📜 Activity Log"
    │       ├── ListView                (log entries with timestamp)
    │       └── StackPanel             (Clear, Refresh buttons)
    │
    └── Border (status bar at the bottom)
```

### 3d. Key XAML concepts used

| XAML Feature | What it does |
|---|---|
| `x:Name="..."` | Gives a control a name so C# code can reference it |
| `ItemsSource="{Binding ...}"` | Links a ListView to a C# collection |
| `DisplayMemberBinding="{Binding Title}"` | Shows a property from the data class |
| `Tag="A"` | Stores data on a control (used to identify which answer was chosen) |
| `Click="EventName_Click"` | Links a button click to a C# method |

---

## STEP 4 – REPLACE MainWindow.xaml.cs (THE LOGIC)

### 4a. Open the code-behind
In Solution Explorer, click the **arrow** next to `MainWindow.xaml`
to expand it, then double-click `MainWindow.xaml.cs`.

OR: With `MainWindow.xaml` open, press `F7` to jump to the code-behind.

### 4b. Select all and replace
1. Press `Ctrl + A` → `Delete`.
2. Paste the **entire contents of `MainWindow.xaml.cs`**.

### 4c. Understanding the code sections

The code-behind is organised into 7 sections with clear comments:

---

### SECTION 1 – DATA MODELS (the three simple classes at the top)

```csharp
// These classes hold data for each feature.
// They are displayed in ListViews via data binding.

public class CyberTask    // One task (title, description, reminder, done flag)
public class LogEntry     // One log line (timestamp + description)
public class QuizQuestion // One question (text, 4 options, correct answer, explanation)
```

**Why this approach?**
Instead of using global variables scattered everywhere, we create proper
data classes. This makes the code clean, maintainable, and easy to extend.

---

### SECTION 2 – CONSTRUCTOR (runs when the window opens)

```csharp
public MainWindow()
{
    InitializeComponent();         // Connects XAML controls to C# variables
    TaskListView.ItemsSource = _tasks;  // Bind task list to the ListView
    LogListView.ItemsSource  = _log;    // Bind log list to the ListView
    LoadQuizQuestions();           // Populate the question bank
    AppendChat("🤖 Bot", "Hello!..."); // Welcome message
    AddLog("Application started."); // First log entry
}
```

---

### SECTION 3 – NLP SIMULATION (the most important part of Part 3)

```csharp
private void ProcessUserInput()
{
    string input = UserInputBox.Text.ToLower(); // Case-insensitive matching

    // Priority order: quiz → task → knowledge topics → fallback
    if (ContainsAny(input, "quiz", "game", "start quiz"))
        → StartQuiz

    else if (ContainsAny(input, "add task", "remind me", "enable 2fa"))
        → SuggestTask()   // Pre-fills the Tasks tab

    else if (ContainsAny(input, "phishing", "scam email"))
        → GetPhishingInfo()

    else if (ContainsAny(input, "password", "passphrase"))
        → GetPasswordInfo()

    // ... and so on for each topic

    else
        → NlpFallback()  // Tries a second pass with partial matches
}
```

**The `ContainsAny()` helper:**
```csharp
// Checks if the input matches ANY of the given keywords
private static bool ContainsAny(string input, params string[] keywords)
    => keywords.Any(k => input.Contains(k, StringComparison.OrdinalIgnoreCase));
```

This means "Enable 2FA", "enable two-factor", and "set up 2fa" all
trigger the same response — just like real NLP!

**The `NlpFallback()` method:**
When no keyword matches, we do a second sweep with broader terms
("hack", "attack", "safe", "secure", "email") to give a partial response
rather than just saying "I don't understand."

---

### SECTION 4 – TASK MANAGER

```csharp
private void AddTask_Click(...)
{
    // 1. Validate that a title was entered
    // 2. Create a CyberTask object
    // 3. Add it to _tasks (ObservableCollection → ListView auto-updates)
    // 4. Log the action
    // 5. Clear the form
}

private void MarkDone_Click(...)
{
    // 1. Get the selected task from TaskListView.SelectedItem
    // 2. Set IsDone = true
    // 3. Call RefreshTaskList() to force the ListView to re-render
}

private void DeleteTask_Click(...)
{
    // 1. Confirm with the user (MessageBox.Show)
    // 2. Remove from _tasks collection
}
```

**Why `ObservableCollection<T>`?**
Unlike a regular `List<T>`, `ObservableCollection<T>` automatically tells
the ListView to update whenever items are added or removed.
You don't have to manually refresh it for Add/Remove — only for
property changes (like marking done), where we call `RefreshTaskList()`.

---

### SECTION 5 – QUIZ

```csharp
private void LoadQuizQuestions()
{
    _questions = new List<QuizQuestion> { ... }; // 12 questions
    ShuffleQuestions(); // Fisher-Yates shuffle for random order
}

private void ShowNextQuestion()
{
    // Display question text + 4 radio button options
    // Clear previous selection
    // Hide feedback panel
    // Disable Next button until user answers
}

private void Answer_Click(...)
{
    // 1. Read which radio button was clicked (Tag = "A"/"B"/"C"/"D")
    // 2. Compare to CorrectAnswer
    // 3. Update score
    // 4. Show feedback (green for correct, red for wrong)
    // 5. Lock answers (disable radio buttons)
    // 6. Enable Next button
}

private void EndQuiz()
{
    // Show final score + motivational message
    // Calculate percentage and pick appropriate message
}
```

---

### SECTION 6 – ACTIVITY LOG

```csharp
private void AddLog(string description)
{
    _log.Add(new LogEntry {
        Timestamp   = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        Description = description
    });
    // Keep max 100 entries to prevent memory bloat
}
```

Every significant action calls `AddLog()`:
- Task added/deleted/completed
- Quiz started/answered/completed
- User asked about a topic
- User used a quick-action button

---

## STEP 5 – BUILD AND RUN

### 5a. Build the project
Press `Ctrl + Shift + B` (Build Solution).

**Common errors and fixes:**

| Error | Fix |
|-------|-----|
| `The name 'ChatDisplay' does not exist` | Check x:Name in XAML matches variable in .cs |
| `TargetFramework net8.0-windows not found` | Install ".NET 8 SDK" or change to net6.0-windows in .csproj |
| `InitializeComponent() not found` | Make sure `x:Class` in XAML matches namespace in .cs |
| `Partial class must have same base class` | Check `public partial class MainWindow : Window` in .cs |

### 5b. Run the project
Press `F5` (Debug) or `Ctrl + F5` (Run without debugging).

The app should open with the dark navy theme and 4 tabs.

---

## STEP 6 – TEST EACH FEATURE

### Testing the Chat / NLP (Tab 1)
Type each of these and press Enter — the bot should respond appropriately:

| Input | Expected behaviour |
|-------|-------------------|
| `hello` | Greeting response |
| `what is phishing` | Phishing information |
| `help me with passwords` | Password tips |
| `enable 2fa` | Switches to Tasks tab + pre-fills title |
| `start quiz` | Switches to Quiz tab + starts quiz |
| `show activity log` | Switches to Log tab |
| `xyzabcnotacommand` | NLP fallback with suggestions |

### Testing the Task Manager (Tab 2)
1. Type a title (e.g., "Enable 2FA on Gmail") → click ➕ Add Task.
2. Add a description and optional reminder date.
3. Select the task → click ✅ Mark Done → the ✅ icon should appear.
4. Select a task → click 🗑️ Delete → confirm → task disappears.

### Testing the Quiz (Tab 3)
1. Click ▶ Start Quiz — first question appears.
2. Click an answer — feedback appears immediately (green = correct, red = wrong).
3. Click ⏭ Next Question — next question loads.
4. After all 12 questions: final score screen appears.
5. Click 🔄 Restart — quiz resets.

### Testing the Activity Log (Tab 4)
1. Perform several actions (add tasks, answer quiz, chat).
2. Switch to Activity Log tab — entries should appear with timestamps.
3. Click 🗑️ Clear Log → confirm → log clears (one entry added back).

---

## STEP 7 – COMMIT TO GITHUB

The assignment requires **minimum 6 commits with meaningful messages**.
Here's a suggested commit plan:

```bash
# Commit 1: Initial Part 3 setup
git add .
git commit -m "Part 3: Add WPF tab structure with 4 tabs (Chat, Tasks, Quiz, Log)"

# Commit 2: Task Manager
git add .
git commit -m "Part 3: Implement Task Manager with add/delete/mark-done and DB simulation"

# Commit 3: Quiz feature
git add .
git commit -m "Part 3: Add cybersecurity quiz with 12 questions, scoring, and feedback"

# Commit 4: NLP simulation
git add .
git commit -m "Part 3: Implement NLP keyword detection with fallback for chat responses"

# Commit 5: Activity Log
git add .
git commit -m "Part 3: Add Activity Log to track all chatbot actions with timestamps"

# Commit 6: Polish and integration
git add .
git commit -m "Part 3: Polish UI styling, add quick-action buttons, integrate all features"
```

Then tag the release:
```bash
git tag -a v3.0 -m "Part 3 complete: All 4 features implemented"
git push origin main --tags
```

---

## STEP 8 – SUBMISSION CHECKLIST

Before submitting, confirm:

- [ ] App builds with no errors (`Ctrl + Shift + B`)
- [ ] All 4 tabs work correctly
- [ ] NLP responds to at least 10 different phrasings
- [ ] Quiz has more than 10 questions (ours has 12)
- [ ] Activity Log records: tasks added, quiz started, quiz answers, topics asked
- [ ] Tasks can be: added, viewed, marked done, deleted
- [ ] README.md file explains what the app does
- [ ] GitHub repo has minimum 6 commits
- [ ] GitHub repo has a tag (release)
- [ ] YouTube video recorded (unlisted) covering the code and features

---

## 📝 FEATURE SUMMARY FOR YOUR README

```markdown
## CyberAwarenessBot – Part 3

### Features
1. **Chat + NLP**: Type naturally about cybersecurity topics.
   The bot recognises keywords and their variants (e.g., "2FA",
   "two-factor", "multi-factor" all give the same response).

2. **Task Manager**: Add, view, mark complete, and delete
   cybersecurity reminder tasks. Tasks include a title,
   description, and optional reminder date.

3. **Cybersecurity Quiz**: 12 multiple-choice and true/false
   questions covering phishing, passwords, 2FA, malware, social
   engineering, and safe browsing. Questions are shuffled randomly.
   Immediate feedback and a final score are shown.

4. **Activity Log**: Every significant action (tasks added,
   quiz answers, topics discussed) is recorded with a timestamp.

### Technologies
- C# / WPF (Windows Presentation Foundation)
- .NET 8.0
- XAML for UI layout
- ObservableCollection for real-time ListView binding
- String-based NLP keyword matching simulation
```

---

## 🔴 COMMON PITFALLS TO AVOID

1. **Don't forget `x:Name`** — if the XAML control has no name, the C# code
   can't find it and you'll get a compile error.

2. **`ObservableCollection` vs `List`** — Use `ObservableCollection<T>` for
   collections bound to ListViews. Regular `List<T>` won't update the UI automatically.

3. **`RefreshTaskList()` for property changes** — When you change a property
   on an existing object (like `IsDone = true`), `ObservableCollection` doesn't
   know to refresh. Call `RefreshTaskList()` manually to force a re-render.

4. **Tab index order** — The tabs are at indices 0, 1, 2, 3 in code. If you
   reorder tabs in XAML, update the `SwitchToTab(n)` calls in C#.

5. **`StringComparison.OrdinalIgnoreCase`** — Always use this when comparing
   strings for NLP. Without it, "Phishing" won't match "phishing".

---

*Good luck, Kutlwano! This is a solid Part 3 implementation. 🛡️*
