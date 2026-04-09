// ============================================================
// UIMANAGER.CS - Handles all visual stuff
// ASCII art, colors, typing effect, borders
// ============================================================

using System;
using System.Threading;

namespace CybersecurityChatbot
{// start of namespace
    public class UIManager
    {// start of class
        // Show the ASCII art logo
        public static void ShowLogo()
        {// start of main method 
            Console.ForegroundColor = ConsoleColor.Cyan;

            string logo = @"
╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║     ██████╗██╗   ██╗██████╗ ███████╗██████╗ ███████╗        ║
║    ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗██╔════╝        ║
║    ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝███████╗        ║
║    ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗╚════██║        ║
║    ╚██████╗   ██║   ██████╔╝███████╗██║  ██║███████║        ║
║     ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝╚══════╝        ║
║                                                              ║
║            🔒  CYBERSECURITY AWARENESS BOT  🔒               ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝";

            Console.WriteLine(logo);
            Console.ResetColor();
            Thread.Sleep(2000);
        }

        // Typing effect - makes text appear letter by letter
        public static void TypeWriter(string message)
        {
            foreach (char c in message)
            {
                Console.Write(c);
                Thread.Sleep(40);  // 40 milliseconds between letters
            }
            Console.WriteLine();
            Thread.Sleep(300);
        }

        // Ask for user's name with validation
        public static string AskForName()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║              WELCOME TO THE CHAT           ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n🤖 Hello! What's your name? ");
            Console.ResetColor();

            string name = Console.ReadLine();

            // Keep asking until they give a valid name
            while (string.IsNullOrWhiteSpace(name))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("⚠️ Please enter your name: ");
                Console.ResetColor();
                name = Console.ReadLine();
            }

            return name;
        }

        // Show welcome message with typing effect
        public static void ShowWelcome(string name)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            TypeWriter($"\n✨ Welcome, {name}! ✨");
            TypeWriter("🔒 I'm your Cybersecurity Awareness Assistant!");
            TypeWriter("💡 Type 'help' for topics or 'exit' to leave.");
            Console.ResetColor();
        }

        // Draw a separator line
        public static void DrawSeparator()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n" + new string('─', 60));
            Console.ResetColor();
        }

        // Show bot's response
        public static void ShowBotResponse(string response)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("🤖 Bot: ");
            Console.ResetColor();
            TypeWriter(response);
        }

        // Show message for invalid input
        public static void ShowInvalidInput()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("🤖 Bot: ");
            Console.ResetColor();
            TypeWriter("Hmm, I didn't understand that. Try asking about phishing, passwords, or links!");
        } // end of main method 
    }// end of class UIManager
}// end of the namespace