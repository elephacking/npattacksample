using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace NPServer {
    class Program {

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetNamedPipeClientProcessId(IntPtr Pipe, out long ClientProcessId);


        static void Main(string[] args) {

            //Allow all authenticated users
            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                            PipeAccessRights.ReadWrite, AccessControlType.Allow));

            while (true) {

                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("NPAttackPipe", PipeDirection.InOut,10, PipeTransmissionMode.Byte,
                     PipeOptions.Asynchronous, 1024, 1024, pipeSecurity)) {

                    Console.WriteLine("NamedPipeServerStream object created.");
                    // Wait for a client to connect
                    Console.Write("Waiting for client connection...");
                    pipeServer.WaitForConnection();

                    Console.WriteLine("Client connected.");
                    try {
                        // Read user input and send that to the client process.
                        using (StreamReader sr = new StreamReader(pipeServer)) {
                            string commandLine = sr.ReadLine();
                            long clientPID = 0;

                            GetNamedPipeClientProcessId(pipeServer.SafePipeHandle.DangerousGetHandle(), out clientPID);

                            if (clientPID == 0) {
                                Console.WriteLine("Failed to get client PID, bailing...");
                                continue;
                            }

                            Process clientProcess = Process.GetProcessById((int)clientPID);

                            if (clientProcess.ProcessName != "NPClient") {
                                Console.WriteLine("Hax0r detected, execution denied!");
                                continue;
                            }

                            Process.Start(commandLine);
                            Console.WriteLine($"NPClient validated, lanched process {commandLine}");
                        }
                    }
                    // Catch the IOException that is raised if the pipe is broken
                    // or disconnected.
                    catch (Exception e) {
                        Console.WriteLine("ERROR: {0}", e.Message);
                    } finally {
                        pipeServer.Close();
                    }
                }
            }
        }
    }
}
