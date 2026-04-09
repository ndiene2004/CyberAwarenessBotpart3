// ============================================================
// AUDIOMANAGER.CS - Handles voice greeting
// Plays the WAV file if it exists
// ============================================================

using System;
using System.Media;
using System.Threading;

namespace CybersecurityChatbot
{// start of namespace
    public class AudioManager
    {//start of class
        public static void PlayGreeting()
        {// start of main method 
            try
            {
                // Get the folder where the program is running
                string folder = AppDomain.CurrentDomain.BaseDirectory;
                string audioFile = folder + "greeting.wav";

                // Check if the file exists
                if (System.IO.File.Exists(audioFile))
                {
                    SoundPlayer player = new SoundPlayer(audioFile);
                    player.Play();  // Play the sound

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n🔊 Playing welcome message...");
                    Console.ResetColor();

                    Thread.Sleep(3000);  // Wait for sound to finish
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n⚠️ Voice file not found. Place 'greeting.wav' in:\n   {folder}");
                    Console.ResetColor();
                    Thread.Sleep(2000);
                }
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n⚠️ Error: {error.Message}");
                Console.ResetColor();
                Thread.Sleep(2000);
            }
        } // end of main method 
    }//end of class AudoioManager
}//end of the nemespace
