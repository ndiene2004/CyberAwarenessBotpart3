// ============================================================
// PROGRAM.CS - This is just the front door / entry point
// It only starts the chatbot - nothing else!
// ============================================================

using System;

namespace CybersecurityChatbot
{//start of namespace
    class Program
    {//start of class 
        static void Main(string[] args)
        {// start of main method 
            // Make console window bigger
            Console.SetWindowSize(120, 40);
            Console.Title = "🛡️ CYBERSECURITY AWARENESS BOT 🛡️";

            // Start the chatbot (everything else is in other files)
            ChatBot.Start();
        } //end of namespace
    }//end of class program
}//end of main method 