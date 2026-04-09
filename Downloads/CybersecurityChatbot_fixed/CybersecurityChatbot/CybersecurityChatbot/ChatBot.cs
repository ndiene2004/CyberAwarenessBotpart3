// ============================================================
// CHATBOT.CS - The brain of the chatbot
// Handles conversations, questions, and responses
// ============================================================

using System;

namespace CybersecurityChatbot
{// start of namespace
    public class ChatBot
    {//start of class
        // This is the main method that starts everything
        public static void Start()
        {//start of main method
            // Show the ASCII art logo
            UIManager.ShowLogo();

            // Play the voice greeting
            AudioManager.PlayGreeting();

            // Ask for name and start chatting
            string userName = UIManager.AskForName();
            UIManager.ShowWelcome(userName);

            // Start the conversation loop
            RunConversation(userName);
        }

        // Main conversation loop - keeps chatting until user types 'exit'
        private static void RunConversation(string userName)
        {
            bool isRunning = true;

            while (isRunning)
            {
                // Draw separator line
                UIManager.DrawSeparator();

                // Get user input
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"\n{userName}: ");
                Console.ResetColor();

                string userInput = Console.ReadLine()?.ToLower().Trim();

                // Check for empty input
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    UIManager.ShowInvalidInput();
                    continue;
                }

                // Check for exit command
                if (userInput == "exit" || userInput == "quit" || userInput == "bye")
                {
                    UIManager.TypeWriter($"\n👋 Goodbye, {userName}! Stay safe online!");
                    isRunning = false;
                    break;
                }

                // Get bot response and display it
                string response = GetResponse(userInput);
                UIManager.ShowBotResponse(response);
            }
        }

        // The brain that figures out what to say based on keywords
        private static string GetResponse(string input)
        {
            // Phishing questions
            if (input.Contains("phish") || input.Contains("phishing"))
            {
                return "📧 PHISHING: Fake messages trying to steal your info!\n" +
                       "   • Check sender's email address carefully\n" +
                       "   • Never click suspicious links\n" +
                       "   • Don't share personal info via email";
            }

            // Password questions
            if (input.Contains("password") || input.Contains("pass"))
            {
                return "🔐 PASSWORD SAFETY:\n" +
                       "   • Use at least 12 characters\n" +
                       "   • Mix letters, numbers, and symbols\n" +
                       "   • Don't reuse passwords\n" +
                       "   • Use a password manager!";
            }

            // Suspicious links
            if (input.Contains("link") || input.Contains("url") || input.Contains("click"))
            {
                return "🔗 SUSPICIOUS LINKS:\n" +
                       "   • Hover before clicking to see real URL\n" +
                       "   • Look for 'https://' (the 's' means secure)\n" +
                       "   • Check for misspellings in the address";
            }

            // Malware/virus questions
            if (input.Contains("malware") || input.Contains("virus"))
            {
                return "🦠 MALWARE PROTECTION:\n" +
                       "   • Keep software updated\n" +
                       "   • Use antivirus software\n" +
                       "   • Don't download from untrusted sources";
            }

            // Help
            if (input.Contains("help") || input == "help")
            {
                return "📚 TOPICS: phishing, passwords, links, malware, viruses\n" +
                       "Type 'exit' to leave. Just ask me anything about cybersecurity!";
            }

            // Greetings
            if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey"))
            {
                return "Hello! Ask me about phishing, passwords, or suspicious links! 🔒";
            }

            // Default response when bot doesn't understand
            return "I'm still learning! Try asking about: phishing, passwords, or links. Type 'help' for options.";
        }//end of main method 
    }// end of class ChatBot
}// end  of namespace 
