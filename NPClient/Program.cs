using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPClient {
    class Program {
        static void Main(string[] args) {

            Console.WriteLine($"Started NPClient with PID {Process.GetCurrentProcess().Id}");

            while (true) {

                Console.Write("Enter Command: ");
                string command = Console.ReadLine();

                using (NamedPipeClientStream pipeClient =
                         new NamedPipeClientStream("NPAttackPipe")) {

                    // Wait for a client to connect
                    Console.Write("Connecting to NPAttack pipe...");
                    pipeClient.Connect();
                    Console.WriteLine("Client connected.");
                    try {
                        // Read user input and send that to the client process.
                        using (StreamWriter sw = new StreamWriter(pipeClient)) {
                            sw.AutoFlush = true;
                            sw.WriteLine(command);
                        }
                    }
                    // Catch the IOException that is raised if the pipe is broken
                    // or disconnected.
                    catch (IOException e) {
                        Console.WriteLine("ERROR: {0}", e.Message);
                    } finally {
                        pipeClient.Close();
                    }
                }
            }
        }
    }
}
