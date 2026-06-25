
//  MainWindow.xaml.cs  –  CyberAwarenessBot  Part 3
//  Author : Kutlwano (Digitra Solutions)
//  Purpose: Advanced chatbot with Task Manager, Quiz, NLP simulation,
//           and Activity Log — all inside a WPF Windows Forms–style GUI.


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberAwarenessBot
{
    
    //  DATA MODELS
    //  These simple classes hold the data for each feature.
    //  They are displayed in the ListView controls via data binding.
    

    /// <summary>
    /// Represents a single cybersecurity task created by the user.
    /// A task can have a title, description, optional reminder date,
    /// and a flag that marks it as completed.
    /// </summary>
    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? ReminderDate { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public bool IsDone { get; set; } = false;

        // ── Display helpers used by the GridView columns ──
        // A tick icon shows whether the task is done
        public string DoneIcon => IsDone ? "✅" : "⬜";

        // Show the reminder date nicely, or "None" if not set
        public string ReminderDisplay => ReminderDate.HasValue
            ? ReminderDate.Value.ToString("yyyy-MM-dd")
            : "None";

        // Show when the task was created
        public string AddedDisplay => AddedDate.ToString("yyyy-MM-dd HH:mm");
    }

    /// <summary>
    /// Represents one entry in the Activity Log.
    /// Every significant action the bot takes gets recorded here
    /// so the user can review what happened.
    /// </summary>
    public class LogEntry
    {
        public string Timestamp { get; set; } = "";
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// Represents one quiz question with four multiple-choice options,
    /// the correct answer key (A/B/C/D), and an explanation shown
    /// after the user answers.
    /// </summary>
    public class QuizQuestion
    {
        public string QuestionText { get; set; } = "";
        public string OptionA { get; set; } = "";
        public string OptionB { get; set; } = "";
        public string OptionC { get; set; } = "";
        public string OptionD { get; set; } = "";
        public string CorrectAnswer { get; set; } = ""; // "A", "B", "C", or "D"
        public string Explanation { get; set; } = "";
    }

    // ─────────────────────────────────────────────────────────────────
    //  MAIN WINDOW CODE-BEHIND
    
    public partial class MainWindow : Window
    {
        // ── In-memory storage (simulates a database for this demo) ──
        // ObservableCollection automatically updates the ListView when items change.
        private ObservableCollection<CyberTask> _tasks = new();
        private ObservableCollection<LogEntry>  _log   = new();

        // ── Task counter (simulates an auto-increment primary key) ──
        private int _nextTaskId = 1;

        // ── Quiz state ──
        private List<QuizQuestion> _questions = new();  // All questions
        private int    _currentQuestionIndex = -1;       // Which question we're on
        private int    _score  = 0;                      // Correct answers so far
        private int    _total  = 0;                      // Questions answered so far
        private bool   _quizRunning = false;             // Is the quiz active?
        private string _selectedAnswer = "";             // What the user selected

        
        //  CONSTRUCTOR – runs when the window first opens
        
        public MainWindow()
        {
            InitializeComponent();

            // Wire up the data collections to the ListViews
            TaskListView.ItemsSource = _tasks;
            LogListView.ItemsSource  = _log;

            // Load the bank of quiz questions
            LoadQuizQuestions();

            // Greet the user in the chat window
            AppendChat("🤖 Bot", "Hello! I'm your Cyber Awareness Bot. How can I help you today?\n" +
                                  "You can:\n" +
                                  "  • Add a cybersecurity task (try: 'add task')\n" +
                                  "  • Take a quiz (try: 'start quiz')\n" +
                                  "  • View your activity log (try: 'show log')\n" +
                                  "  • Ask about phishing, passwords, 2FA, malware, and more!");

            // Log this startup event
            AddLog("Application started – Cyber Awareness Bot Part 3 ready.");
        }

        
        //  SECTION 1: CHAT / NLP
        //  This is the core "intelligence" of the bot.
        //  We simulate NLP using keyword detection + string matching.
        

        /// <summary>
        /// Called when the user presses Enter inside the input box.
        /// We intercept the Enter key so the user doesn't have to
        /// click the Send button every time.
        /// </summary>
        private void UserInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Only react to the Enter key (not Shift+Enter etc.)
            if (e.Key == Key.Enter)
                ProcessUserInput();
        }

        /// <summary>
        /// Called when the user clicks the ➤ Send button.
        /// </summary>
        private void SendButton_Click(object sender, RoutedEventArgs e)
            => ProcessUserInput();

        /// <summary>
        /// Main NLP processor – reads what the user typed,
        /// figures out the intent using keyword matching,
        /// and generates an appropriate response.
        ///
        /// This is the "NLP simulation" required by Task 3 of Part 3.
        /// We use string.Contains() for keyword detection,
        /// which is flexible enough to catch different phrasings.
        /// </summary>
        private void ProcessUserInput()
        {
            // Grab and clean the user's input
            string raw   = UserInputBox.Text.Trim();
            string input = raw.ToLower(); // lowercase so keyword matching is case-insensitive

            // Ignore empty input – don't send blank messages
            if (string.IsNullOrEmpty(input)) return;

            // Show the user's message in the chat
            AppendChat("👤 You", raw);

            // Clear the input box so the user can type the next message
            UserInputBox.Clear();

            // ── LOG the raw user input (Activity Log) ──
            AddLog($"User input received: \"{raw}\"");

            // ── INTENT DETECTION ──
            // We check for keywords in a priority order:
            // quiz-related → task-related → knowledge topics → log/help → fallback

            string response;

            // ── QUIZ intents ──
            if (ContainsAny(input, "start quiz", "begin quiz", "play quiz", "quiz", "game"))
            {
                response = StartQuizFromChat();
            }

            // ── TASK intents ──
            else if (ContainsAny(input, "add task", "create task", "new task", "add a task",
                                        "i need to", "remind me", "set a reminder",
                                        "enable two-factor", "enable 2fa", "update my password",
                                        "review account", "review privacy"))
            {
                // The user wants to create a task – switch to the Tasks tab and pre-fill the title
                response = SuggestTask(raw);
            }
            else if (ContainsAny(input, "view tasks", "show tasks", "my tasks", "list tasks"))
            {
                SwitchToTab(1); // Tasks tab is index 1
                response = $"Switching to your Task Manager. You currently have {_tasks.Count} task(s).";
                AddLog("User viewed task list.");
            }
            else if (ContainsAny(input, "show log", "activity log", "what have you done",
                                        "recent actions", "history"))
            {
                SwitchToTab(3); // Activity Log tab
                response = "Here's your activity log! It shows the last actions taken by the bot.";
                AddLog("User requested activity log.");
            }

            // ── KNOWLEDGE: Phishing ──
            else if (ContainsAny(input, "phishing", "phish", "scam email", "fake email",
                                         "suspicious email", "spam"))
            {
                response = GetPhishingInfo();
                AddLog("User asked about phishing.");
            }

            // ── KNOWLEDGE: Passwords ──
            else if (ContainsAny(input, "password", "passphrase", "strong password",
                                         "weak password", "update my password", "change password"))
            {
                response = GetPasswordInfo();
                AddLog("User asked about passwords.");
            }

            // ── KNOWLEDGE: Two-Factor Authentication ──
            else if (ContainsAny(input, "two-factor", "2fa", "mfa", "multi-factor",
                                         "authenticator", "otp", "one-time password"))
            {
                response = Get2FAInfo();
                AddLog("User asked about 2FA/MFA.");
            }

            // ── KNOWLEDGE: Malware ──
            else if (ContainsAny(input, "malware", "virus", "ransomware", "trojan",
                                         "spyware", "adware", "worm"))
            {
                response = GetMalwareInfo();
                AddLog("User asked about malware.");
            }

            // ── KNOWLEDGE: Social Engineering ──
            else if (ContainsAny(input, "social engineering", "pretexting", "baiting",
                                         "vishing", "tailgating"))
            {
                response = GetSocialEngineeringInfo();
                AddLog("User asked about social engineering.");
            }

            // ── KNOWLEDGE: Safe Browsing ──
            else if (ContainsAny(input, "safe browsing", "https", "vpn", "secure website",
                                         "browser safety", "private mode", "incognito"))
            {
                response = GetSafeBrowsingInfo();
                AddLog("User asked about safe browsing.");
            }

            // ── KNOWLEDGE: Privacy ──
            else if (ContainsAny(input, "privacy", "data protection", "personal data",
                                         "gdpr", "popia", "account privacy"))
            {
                response = GetPrivacyInfo();
                AddLog("User asked about privacy.");
            }

            // ── GREETING ──
            else if (ContainsAny(input, "hello", "hi", "hey", "good morning",
                                         "good afternoon", "howzit", "sawubona"))
            {
                response = "Hello! 👋 How can I help you stay cyber-safe today?";
            }

            // ── HELP ──
            else if (ContainsAny(input, "help", "what can you do", "commands",
                                         "options", "menu"))
            {
                response = GetHelpText();
            }

            // ── FAREWELL ──
            else if (ContainsAny(input, "bye", "goodbye", "exit", "quit", "see you"))
            {
                response = "Stay safe online! Goodbye 👋 Remember: Think before you click!";
                AddLog("User said goodbye.");
            }

            // ── FALLBACK: we didn't understand the input ──
            else
            {
                response = NlpFallback(input);
                AddLog($"NLP fallback triggered for: \"{raw}\"");
            }

            // Display the bot's response
            AppendChat("🤖 Bot", response);
        }

        /// <summary>
        /// NLP Fallback: When no keyword matches, we still try to be helpful.
        /// We look for partial matches to cybersecurity-related terms
        /// so the bot doesn't just say "I don't understand" every time.
        /// </summary>
        private string NlpFallback(string input)
        {
            // Try partial keyword matches as a second pass
            if (input.Contains("hack") || input.Contains("attack"))
                return "⚠️ Are you asking about cyber attacks? Try asking about 'phishing', 'malware', or 'social engineering' for specific threats.";

            if (input.Contains("safe") || input.Contains("secure") || input.Contains("protect"))
                return "🔒 Great mindset! Ask me about 'passwords', 'two-factor authentication', 'safe browsing', or 'privacy' to improve your security.";

            if (input.Contains("email"))
                return "📧 Email security is important! Ask me about 'phishing' to learn how to spot suspicious emails.";

            // Generic fallback with suggestions
            return "🤔 I didn't quite understand that. Could you rephrase it?\n\n" +
                   "Try topics like:\n" +
                   "  • phishing   • passwords   • 2FA\n" +
                   "  • malware    • privacy     • safe browsing\n" +
                   "Or say 'help' to see what I can do.";
        }

        
        //  SECTION 2: TASK MANAGER
        

        /// <summary>
        /// Adds a new task when the user clicks the ➕ Add Task button
        /// on the Tasks tab.
        /// </summary>
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            // Validate: title is required
            string title = TaskTitleBox.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please enter a task title.", "Validation",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Build the task object
            var task = new CyberTask
            {
                Id          = _nextTaskId++,
                Title       = title,
                Description = TaskDescBox.Text.Trim(),
                ReminderDate = TaskReminderDate.SelectedDate,
                AddedDate   = DateTime.Now,
                IsDone      = false
            };

            // Add to the observable collection → ListView updates automatically
            _tasks.Add(task);

            // Log what we did
            string reminderInfo = task.ReminderDate.HasValue
                ? $" (Reminder: {task.ReminderDisplay})"
                : " (no reminder)";
            AddLog($"Task added: '{task.Title}'{reminderInfo}");

            // Clear the form fields
            TaskTitleBox.Clear();
            TaskDescBox.Clear();
            TaskReminderDate.SelectedDate = null;

            // Update status bar
            SetStatus($"✅ Task '{task.Title}' added successfully!");
        }

        /// <summary>
        /// Marks the selected task as done (ticks the checkbox column).
        /// </summary>
        private void MarkDone_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is CyberTask selected)
            {
                selected.IsDone = true;

                // Force the ListView to refresh so the tick icon updates
                RefreshTaskList();

                AddLog($"Task marked as done: '{selected.Title}'");
                SetStatus($"✅ Task '{selected.Title}' marked as completed.");
            }
            else
            {
                SetStatus("⚠️ Please select a task first.");
            }
        }

        /// <summary>
        /// Deletes the selected task from the list.
        /// </summary>
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is CyberTask selected)
            {
                var result = MessageBox.Show($"Delete task '{selected.Title}'?",
                                             "Confirm Delete",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    AddLog($"Task deleted: '{selected.Title}'");
                    _tasks.Remove(selected);
                    SetStatus($"🗑️ Task '{selected.Title}' deleted.");
                }
            }
            else
            {
                SetStatus("⚠️ Please select a task to delete.");
            }
        }

        /// <summary>
        /// Refreshes the task ListView (useful after marking tasks done).
        /// </summary>
        private void RefreshTasks_Click(object sender, RoutedEventArgs e)
            => RefreshTaskList();

        /// <summary>
        /// Forces the ListView to re-render by resetting its ItemsSource.
        /// WPF doesn't automatically re-render bound properties that
        /// don't implement INotifyPropertyChanged, so we do this manually.
        /// </summary>
        private void RefreshTaskList()
        {
            TaskListView.ItemsSource = null;
            TaskListView.ItemsSource = _tasks;
        }

        
        //  SECTION 3: CYBERSECURITY QUIZ
        

        /// <summary>
        /// Loads the bank of 12 cybersecurity quiz questions.
        /// Questions cover: phishing, passwords, 2FA, malware,
        /// social engineering, and safe browsing – as per the spec.
        /// Mix of multiple-choice and true/false formats.
        /// </summary>
        private void LoadQuizQuestions()
        {
            _questions = new List<QuizQuestion>
            {
                // ── Q1: Phishing ──
                new QuizQuestion
                {
                    QuestionText  = "What should you do if you receive an email asking for your password?",
                    OptionA = "A) Reply with your password",
                    OptionB = "B) Delete the email",
                    OptionC = "C) Report the email as phishing",
                    OptionD = "D) Ignore it",
                    CorrectAnswer = "C",
                    Explanation   = "✅ Correct! Reporting phishing emails helps prevent scams and protects others."
                },
                // ── Q2: Password safety (True/False format) ──
                new QuizQuestion
                {
                    QuestionText  = "TRUE or FALSE: Using the same password on multiple websites is safe.",
                    OptionA = "A) True – it's convenient",
                    OptionB = "B) False – it's a major security risk",
                    OptionC = "C) True – only if it's a strong password",
                    OptionD = "D) False – but only for social media",
                    CorrectAnswer = "B",
                    Explanation   = "✅ Correct! Reusing passwords means one data breach can compromise all your accounts."
                },
                // ── Q3: 2FA ──
                new QuizQuestion
                {
                    QuestionText  = "What does 2FA stand for?",
                    OptionA = "A) Two-File Authentication",
                    OptionB = "B) Two-Factor Authentication",
                    OptionC = "C) Two-Form Access",
                    OptionD = "D) Two-Firewall Application",
                    CorrectAnswer = "B",
                    Explanation   = "✅ Correct! Two-Factor Authentication adds a second verification step beyond your password."
                },
                // ── Q4: Malware ──
                new QuizQuestion
                {
                    QuestionText  = "Which type of malware encrypts your files and demands payment to restore them?",
                    OptionA = "A) Spyware",
                    OptionB = "B) Adware",
                    OptionC = "C) Ransomware",
                    OptionD = "D) Worm",
                    CorrectAnswer = "C",
                    Explanation   = "✅ Correct! Ransomware encrypts your data and demands a ransom. Always keep backups!"
                },
                // ── Q5: Social Engineering ──
                new QuizQuestion
                {
                    QuestionText  = "A stranger calls pretending to be IT support and asks for your login credentials. What is this called?",
                    OptionA = "A) Phishing",
                    OptionB = "B) Vishing (voice phishing)",
                    OptionC = "C) Tailgating",
                    OptionD = "D) Baiting",
                    CorrectAnswer = "B",
                    Explanation   = "✅ Correct! Vishing is voice-based phishing. Legitimate IT support will NEVER ask for your password."
                },
                // ── Q6: Safe Browsing ──
                new QuizQuestion
                {
                    QuestionText  = "What does the padlock icon (🔒) in a browser address bar indicate?",
                    OptionA = "A) The website is 100% safe",
                    OptionB = "B) Your connection to the site is encrypted (HTTPS)",
                    OptionC = "C) The site has been verified by the government",
                    OptionD = "D) You are in private/incognito mode",
                    CorrectAnswer = "B",
                    Explanation   = "✅ Correct! HTTPS encrypts data in transit but doesn't guarantee the site itself is legitimate."
                },
                // ── Q7: Passwords (True/False) ──
                new QuizQuestion
                {
                    QuestionText  = "TRUE or FALSE: A strong password should be at least 12 characters long and include numbers, symbols, and mixed case.",
                    OptionA = "A) True",
                    OptionB = "B) False – 6 characters is enough",
                    OptionC = "C) False – only letters are needed",
                    OptionD = "D) False – only numbers are needed",
                    CorrectAnswer = "A",
                    Explanation   = "✅ Correct! Longer, complex passwords are exponentially harder to brute-force."
                },
                // ── Q8: Phishing indicators ──
                new QuizQuestion
                {
                    QuestionText  = "Which of the following is a red flag that an email might be phishing?",
                    OptionA = "A) It comes from your bank's official domain",
                    OptionB = "B) It creates urgency like 'Act now or your account will be closed!'",
                    OptionC = "C) It includes the bank's registered address",
                    OptionD = "D) It addresses you by your full name",
                    CorrectAnswer = "B",
                    Explanation   = "✅ Correct! Creating urgency is a classic phishing tactic to pressure you into acting without thinking."
                },
                // ── Q9: Social Engineering ──
                new QuizQuestion
                {
                    QuestionText  = "Someone leaves a USB drive labelled 'Salary Info' in the company parking lot. You plug it in. This is an example of:",
                    OptionA = "A) Vishing",
                    OptionB = "B) Pharming",
                    OptionC = "C) Baiting",
                    OptionD = "D) Tailgating",
                    CorrectAnswer = "C",
                    Explanation   = "✅ Correct! Baiting uses physical media to install malware when curiosity gets the better of you."
                },
                // ── Q10: 2FA ──
                new QuizQuestion
                {
                    QuestionText  = "Which is the MOST secure form of two-factor authentication?",
                    OptionA = "A) SMS text message code",
                    OptionB = "B) Email verification code",
                    OptionC = "C) Hardware security key (e.g. YubiKey)",
                    OptionD = "D) Security question",
                    CorrectAnswer = "C",
                    Explanation   = "✅ Correct! Hardware keys are the most secure because they cannot be intercepted like SMS codes."
                },
                // ── Q11: Malware (True/False) ──
                new QuizQuestion
                {
                    QuestionText  = "TRUE or FALSE: Antivirus software alone is sufficient protection against all modern cyber threats.",
                    OptionA = "A) True – antivirus catches everything",
                    OptionB = "B) False – you need layered security: antivirus, updates, 2FA, and awareness",
                    OptionC = "C) True – if you update it weekly",
                    OptionD = "D) False – antivirus is useless",
                    CorrectAnswer = "B",
                    Explanation   = "✅ Correct! Modern threats require a layered 'defence in depth' approach beyond just antivirus."
                },
                // ── Q12: Safe Browsing ──
                new QuizQuestion
                {
                    QuestionText  = "Which of the following is the SAFEST practice when using public Wi-Fi?",
                    OptionA = "A) Browse normally – public Wi-Fi is always secure",
                    OptionB = "B) Only visit HTTP websites",
                    OptionC = "C) Use a VPN to encrypt your traffic",
                    OptionD = "D) Share your hotspot password with others",
                    CorrectAnswer = "C",
                    Explanation   = "✅ Correct! A VPN encrypts your traffic so eavesdroppers on the same network cannot intercept it."
                }
            };

            // Shuffle the questions so the order is different each time
            ShuffleQuestions();
        }

        /// <summary>
        /// Shuffles the question list using Fisher-Yates algorithm.
        /// This makes the quiz feel fresh on every run.
        /// </summary>
        private void ShuffleQuestions()
        {
            var rng = new Random();
            for (int i = _questions.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (_questions[i], _questions[j]) = (_questions[j], _questions[i]);
            }
        }

        /// <summary>
        /// Starts the quiz from the Chat tab (via NLP detection).
        /// Switches to the Quiz tab and starts the quiz automatically.
        /// </summary>
        private string StartQuizFromChat()
        {
            SwitchToTab(2); // Quiz tab
            StartQuiz();
            return "🎮 Starting the Cybersecurity Quiz! I've switched to the Quiz tab. Good luck!";
        }

        /// <summary>
        /// Starts or re-starts the quiz from the ▶ Start Quiz button.
        /// </summary>
        private void StartQuiz_Click(object sender, RoutedEventArgs e)
            => StartQuiz();

        /// <summary>
        /// Core quiz start logic – resets state and shows the first question.
        /// </summary>
        private void StartQuiz()
        {
            // Reset all quiz state variables
            _currentQuestionIndex = -1;
            _score = 0;
            _total = 0;
            _quizRunning = true;
            _selectedAnswer = "";

            // Re-shuffle so the order changes
            ShuffleQuestions();

            // Update score display
            UpdateScoreDisplay();

            // Log quiz start
            AddLog("Quiz started.");

            // Show the first question
            ShowNextQuestion();
        }

        /// <summary>
        /// Moves to the next question. Called by the ⏭ Next button.
        /// </summary>
        private void NextQuestion_Click(object sender, RoutedEventArgs e)
            => ShowNextQuestion();

        /// <summary>
        /// Displays the next question, or ends the quiz if all done.
        /// </summary>
        private void ShowNextQuestion()
        {
            _currentQuestionIndex++;

            // Check if we've gone through all questions
            if (_currentQuestionIndex >= _questions.Count)
            {
                EndQuiz();
                return;
            }

            var q = _questions[_currentQuestionIndex];

            // Display the question text
            QuestionLabel.Text = $"Q{_currentQuestionIndex + 1}. {q.QuestionText}";

            // Display the four options in the radio buttons
            AnswerA.Content = q.OptionA;
            AnswerB.Content = q.OptionB;
            AnswerC.Content = q.OptionC;
            AnswerD.Content = q.OptionD;

            // Clear any previous selection
            AnswerA.IsChecked = false;
            AnswerB.IsChecked = false;
            AnswerC.IsChecked = false;
            AnswerD.IsChecked = false;

            // Reset colours to default
            SetAnswerColours(Brushes.Transparent);

            // Hide the feedback panel until the user answers
            FeedbackBorder.Visibility = Visibility.Collapsed;
            FeedbackLabel.Text = "";

            // Disable Next until user answers
            NextBtn.IsEnabled = false;
            _selectedAnswer = "";

            // Enable the radio buttons
            SetAnswersEnabled(true);

            // Update question number display
            QuestionNumLabel.Text = (_currentQuestionIndex + 1).ToString();
        }

        /// <summary>
        /// Called when the user clicks any of the answer radio buttons.
        /// Checks the answer, shows feedback, and enables the Next button.
        /// </summary>
        private void Answer_Click(object sender, RoutedEventArgs e)
        {
            if (!_quizRunning) return;

            // Find which radio button was clicked and get its Tag (A/B/C/D)
            var rb = sender as RadioButton;
            _selectedAnswer = rb?.Tag?.ToString() ?? "";

            var q = _questions[_currentQuestionIndex];
            bool isCorrect = _selectedAnswer == q.CorrectAnswer;

            // Increment counters
            _total++;
            if (isCorrect) _score++;

            UpdateScoreDisplay();

            // Show the feedback panel with result + explanation
            FeedbackBorder.Visibility = Visibility.Visible;
            if (isCorrect)
            {
                FeedbackLabel.Foreground = Brushes.LightGreen;
                FeedbackLabel.Text = $"✅ Correct!\n\n{q.Explanation}";
            }
            else
            {
                FeedbackLabel.Foreground = Brushes.Salmon;
                FeedbackLabel.Text = $"❌ Incorrect. The correct answer was {q.CorrectAnswer}.\n\n{q.Explanation}";
            }

            // Lock the answers so the user can't change their choice
            SetAnswersEnabled(false);

            // Enable the Next button
            NextBtn.IsEnabled = true;

            // Log the answer
            AddLog($"Quiz Q{_currentQuestionIndex + 1}: User answered {_selectedAnswer} – {(isCorrect ? "Correct" : "Wrong")}");
        }

        /// <summary>
        /// Shows the final score and a motivational message when all
        /// questions have been answered.
        /// </summary>
        private void EndQuiz()
        {
            _quizRunning = false;

            // Calculate percentage
            double pct = _questions.Count > 0 ? (_score * 100.0 / _questions.Count) : 0;

            // Pick a message based on how well they did
            string message;
            if (pct == 100)
                message = "🏆 Perfect score! You're a cybersecurity pro!";
            else if (pct >= 80)
                message = "⭐ Great job! You really know your cybersecurity!";
            else if (pct >= 60)
                message = "👍 Good effort! Keep learning to stay safe online.";
            else
                message = "📚 Keep practising! Check the chat tab to learn more about cybersecurity.";

            // Show result in the question area
            QuestionLabel.Text = $"🎉 Quiz Complete!\n\nYour Score: {_score} / {_questions.Count} ({pct:F0}%)\n\n{message}";

            // Hide answer options
            AnswerPanel.Visibility = Visibility.Collapsed;
            FeedbackBorder.Visibility = Visibility.Collapsed;
            NextBtn.IsEnabled = false;

            AddLog($"Quiz completed. Score: {_score}/{_questions.Count} ({pct:F0}%)");
        }

        /// <summary>
        /// Resets the quiz to its initial state (Restart button).
        /// </summary>
        private void RestartQuiz_Click(object sender, RoutedEventArgs e)
        {
            _quizRunning = false;
            _currentQuestionIndex = -1;
            _score = 0;
            _total = 0;

            QuestionLabel.Text = "Press '▶ Start Quiz' to begin!";
            AnswerPanel.Visibility = Visibility.Visible;
            FeedbackBorder.Visibility = Visibility.Collapsed;
            NextBtn.IsEnabled = false;

            AnswerA.IsChecked = false;
            AnswerB.IsChecked = false;
            AnswerC.IsChecked = false;
            AnswerD.IsChecked = false;

            UpdateScoreDisplay();
            AddLog("Quiz restarted.");
        }

        /// <summary>
        /// Updates the Score / Total labels at the top of the Quiz tab.
        /// </summary>
        private void UpdateScoreDisplay()
        {
            ScoreLabel.Text = _score.ToString();
            TotalLabel.Text = _total.ToString();
        }

        // ──────────────────────────────────────────────────────────
        //  SECTION 4: ACTIVITY LOG
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a new entry to the activity log.
        /// Every significant action the bot takes calls this.
        /// </summary>
        private void AddLog(string description)
        {
            _log.Add(new LogEntry
            {
                Timestamp   = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = description
            });

            // Keep only the last 100 entries to avoid memory bloat
            while (_log.Count > 100)
                _log.RemoveAt(0);
        }

        /// <summary>
        /// Clears the entire activity log.
        /// </summary>
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Clear the entire activity log?",
                                         "Confirm Clear",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _log.Clear();
                // Add a fresh entry so the log isn't completely empty
                AddLog("Activity log cleared by user.");
                SetStatus("📜 Activity log cleared.");
            }
        }

        /// <summary>
        /// Refreshes the log ListView (forces a re-render).
        /// </summary>
        private void RefreshLog_Click(object sender, RoutedEventArgs e)
        {
            LogListView.ItemsSource = null;
            LogListView.ItemsSource = _log;
        }

        // ──────────────────────────────────────────────────────────
        //  SECTION 5: KNOWLEDGE BASE RESPONSES
        //  These methods return educational text about each topic.
        // ──────────────────────────────────────────────────────────

        private string GetPhishingInfo() =>
            "🎣 PHISHING\n\n" +
            "Phishing is a cyber attack where criminals impersonate trusted organisations " +
            "(banks, tech companies, government) to steal your credentials or personal data.\n\n" +
            "🚩 Red flags to watch for:\n" +
            "  • Urgent language ('Act now!' / 'Your account will be locked!')\n" +
            "  • Suspicious sender address (e.g. support@paypa1.com)\n" +
            "  • Generic greetings ('Dear Customer' instead of your name)\n" +
            "  • Links that hover to reveal a different URL\n\n" +
            "✅ What to do: Do NOT click links. Report it as phishing. Contact the organisation directly.";

        private string GetPasswordInfo() =>
            "🔑 PASSWORD SECURITY\n\n" +
            "Weak passwords are one of the biggest entry points for attackers.\n\n" +
            "✅ Best practices:\n" +
            "  • Use at least 12-16 characters\n" +
            "  • Mix uppercase, lowercase, numbers, and symbols\n" +
            "  • Never reuse passwords across sites\n" +
            "  • Use a password manager (e.g. Bitwarden, KeePass)\n" +
            "  • Change passwords if you suspect a breach\n\n" +
            "❌ Avoid: your name, birthdate, 'password123', or keyboard patterns like 'qwerty'.";

        private string Get2FAInfo() =>
            "🔐 TWO-FACTOR AUTHENTICATION (2FA)\n\n" +
            "2FA adds a second layer of security beyond your password. " +
            "Even if a hacker steals your password, they can't log in without the second factor.\n\n" +
            "🔢 Types (most → least secure):\n" +
            "  1. Hardware key (YubiKey) – most secure\n" +
            "  2. Authenticator app (Google Authenticator, Authy)\n" +
            "  3. Push notification (Duo, Microsoft Authenticator)\n" +
            "  4. SMS code – convenient but can be SIM-swapped\n\n" +
            "✅ Enable 2FA on: email, banking, social media, and work accounts NOW.";

        private string GetMalwareInfo() =>
            "🦠 MALWARE\n\n" +
            "Malware is malicious software designed to damage, disrupt, or gain unauthorised access.\n\n" +
            "📦 Types:\n" +
            "  • Virus – attaches to files and spreads\n" +
            "  • Ransomware – encrypts files and demands payment\n" +
            "  • Spyware – secretly monitors your activity\n" +
            "  • Trojan – disguised as legitimate software\n" +
            "  • Worm – self-replicates across networks\n\n" +
            "✅ Protection:\n" +
            "  • Keep OS and software updated\n" +
            "  • Use reputable antivirus/EDR software\n" +
            "  • Don't download pirated software\n" +
            "  • Back up data regularly (3-2-1 rule)";

        private string GetSocialEngineeringInfo() =>
            "🎭 SOCIAL ENGINEERING\n\n" +
            "Social engineering exploits human psychology rather than technical vulnerabilities. " +
            "The attacker manipulates YOU into giving them access.\n\n" +
            "🎯 Common techniques:\n" +
            "  • Phishing (email) / Vishing (voice) / Smishing (SMS)\n" +
            "  • Pretexting – fabricating a scenario to gain trust\n" +
            "  • Baiting – leaving infected USB drives in parking lots\n" +
            "  • Tailgating – following someone into a secure area\n\n" +
            "✅ Golden rule: Verify identity before sharing ANY information. " +
            "Legitimate IT staff will NEVER ask for your password.";

        private string GetSafeBrowsingInfo() =>
            "🌐 SAFE BROWSING\n\n" +
            "Your browser is a major attack surface. Stay safe with these habits:\n\n" +
            "✅ Do:\n" +
            "  • Look for HTTPS (padlock 🔒) – encrypted connection\n" +
            "  • Use a VPN on public Wi-Fi\n" +
            "  • Keep browser and extensions updated\n" +
            "  • Use ad-blockers (they also block malicious ads)\n" +
            "  • Enable Safe Browsing in Chrome/Firefox settings\n\n" +
            "❌ Avoid:\n" +
            "  • Clicking pop-up 'You have a virus!' alerts\n" +
            "  • Downloading software from unofficial sites\n" +
            "  • Saving passwords in the browser without a master password";

        private string GetPrivacyInfo() =>
            "🔏 PRIVACY & DATA PROTECTION\n\n" +
            "Your personal data is valuable. Here's how to protect it:\n\n" +
            "✅ Best practices:\n" +
            "  • Review app permissions – does a torch app need your contacts?\n" +
            "  • Use privacy-focused search engines (DuckDuckGo, Brave)\n" +
            "  • Read privacy policies (especially data sharing clauses)\n" +
            "  • Limit personal info shared on social media\n" +
            "  • Enable account privacy settings on all platforms\n\n" +
            "🇿🇦 In South Africa: POPIA (Protection of Personal Information Act) " +
            "gives you rights over how organisations handle your data. " +
            "You can request to see, correct, or delete your data.";

        private string GetHelpText() =>
            "📖 WHAT I CAN HELP YOU WITH\n\n" +
            "💬 Just type naturally! I understand phrases like:\n\n" +
            "🔒 Security topics:\n" +
            "  • 'What is phishing?' / 'phishing'\n" +
            "  • 'Help with passwords' / 'password'\n" +
            "  • 'What is 2FA?' / 'two-factor'\n" +
            "  • 'malware' / 'ransomware'\n" +
            "  • 'social engineering'\n" +
            "  • 'safe browsing' / 'VPN'\n" +
            "  • 'privacy' / 'data protection'\n\n" +
            "📋 Tasks:\n" +
            "  • 'Add task' / 'Remind me to enable 2FA'\n" +
            "  • 'Show my tasks' / 'View tasks'\n\n" +
            "🎮 Quiz:\n" +
            "  • 'Start quiz' / 'Play game'\n\n" +
            "📜 Log:\n" +
            "  • 'Show log' / 'Activity log' / 'What have you done'";

        // ──────────────────────────────────────────────────────────
        //  SECTION 6: QUICK-ACTION BUTTONS (Chat tab shortcuts)
        // ──────────────────────────────────────────────────────────

        private void QuickTask_Click(object sender, RoutedEventArgs e)
        {
            SwitchToTab(1);
            SetStatus("📋 Task Manager opened. Fill in the form to add a task.");
            AddLog("User opened Task Manager via quick-action button.");
        }

        private void QuickQuiz_Click(object sender, RoutedEventArgs e)
        {
            SwitchToTab(2);
            StartQuiz();
            AddLog("User started quiz via quick-action button.");
        }

        private void QuickLog_Click(object sender, RoutedEventArgs e)
        {
            SwitchToTab(3);
            AddLog("User viewed activity log via quick-action button.");
        }

        private void QuickHelp_Click(object sender, RoutedEventArgs e)
            => AppendChat("🤖 Bot", GetHelpText());

        // ──────────────────────────────────────────────────────────
        //  SECTION 7: HELPER / UTILITY METHODS
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Appends a message to the Chat display area with a speaker label.
        /// Automatically scrolls to the bottom so the newest message is visible.
        /// </summary>
        private void AppendChat(string speaker, string message)
        {
            ChatDisplay.Text += $"\n[{DateTime.Now:HH:mm}] {speaker}:\n{message}\n";
            // Scroll to the bottom of the chat
            ChatScrollViewer.ScrollToBottom();
        }

        /// <summary>
        /// Updates the status bar at the bottom of the window.
        /// </summary>
        private void SetStatus(string message)
            => StatusBar.Text = message;

        /// <summary>
        /// Switches the main TabControl to a specific tab by index.
        ///   0 = Chat, 1 = Tasks, 2 = Quiz, 3 = Activity Log
        /// </summary>
        private void SwitchToTab(int index)
        {
            // Find the TabControl (it's the second item in the main Grid)
            var tabControl = (TabControl)((Grid)Content).Children[1];
            tabControl.SelectedIndex = index;
        }

        /// <summary>
        /// Checks if the input string contains ANY of the given keywords.
        /// This is the core of our NLP keyword detection.
        /// Using params string[] so we can pass any number of keywords.
        /// </summary>
        private static bool ContainsAny(string input, params string[] keywords)
            => keywords.Any(k => input.Contains(k, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Suggests a task to add based on what the user typed in chat.
        /// Switches to the Tasks tab and pre-fills the title field.
        /// This gives a natural NLP→task creation flow.
        /// </summary>
        private string SuggestTask(string rawInput)
        {
            SwitchToTab(1);

            // Pre-fill the task title with a cleaned version of the user's message
            string suggestedTitle = rawInput
                .Replace("add task", "", StringComparison.OrdinalIgnoreCase)
                .Replace("create task", "", StringComparison.OrdinalIgnoreCase)
                .Replace("remind me to", "", StringComparison.OrdinalIgnoreCase)
                .Replace("i need to", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (!string.IsNullOrEmpty(suggestedTitle))
                TaskTitleBox.Text = suggestedTitle;

            AddLog($"Task creation suggested from chat: '{suggestedTitle}'");
            return "📋 I've opened the Task Manager and pre-filled the title for you! " +
                   "Add a description and optional reminder date, then click '➕ Add Task'.";
        }

        /// <summary>
        /// Enables or disables all four answer radio buttons.
        /// We disable them after the user selects an answer so they
        /// can't change their mind after seeing feedback.
        /// </summary>
        private void SetAnswersEnabled(bool enabled)
        {
            AnswerA.IsEnabled = enabled;
            AnswerB.IsEnabled = enabled;
            AnswerC.IsEnabled = enabled;
            AnswerD.IsEnabled = enabled;
        }

        /// <summary>
        /// Sets the background colour of all answer radio buttons.
        /// Used to highlight correct/incorrect answers (currently unused
        /// in this version but available for future enhancement).
        /// </summary>
        private void SetAnswerColours(Brush brush)
        {
            AnswerA.Background = brush;
            AnswerB.Background = brush;
            AnswerC.Background = brush;
            AnswerD.Background = brush;
        }
    }
}
