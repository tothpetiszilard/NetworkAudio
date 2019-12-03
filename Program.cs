using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;

using System.Diagnostics;

using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;

namespace NetworkAudio
{
    class Program
    {
        private static ISoundIn soundIn;

        static void Main(string[] args)
        {



            if (args.Length > 0)
            {
                if (args[0] == "-r")
                {
                    
                    /* Receiver / Server / */
                    if (args.Length == 1)
                    {
                        Console.Write("Listening on ports: TCP 50001 (default)");
                        Console.Write(" and UDP 50000 (default)");
                        ReceiverMain(50001, 50000);
                    }
                    else if (args.Length == 2)
                    {
                        try
                        {
                            if ((0 < int.Parse(args[1])) && (int.Parse(args[1]) < 65536))
                            {
                                Console.Write("Listening on ports: TCP ");
                                Console.Write(args[1]);
                                Console.Write(" and UDP 50000 (default)");
                                ReceiverMain(int.Parse(args[1]), 50000);
                            }
                            else
                            {
                                Console.Write("ERROR: Invalid TCP port defined");
                                return;
                            }
                        }
                        catch (Exception exc)
                        {
                            Console.Write("ERROR: " + exc.Message);
                            return;
                        }
                        
                        
                        }
                    else if (args.Length == 3)
                    {
                        try
                        {
                            if ((0 < int.Parse(args[1])) && (int.Parse(args[1]) < 65536) && (0 < int.Parse(args[2])) && (int.Parse(args[2]) < 65536))
                            {
                                Console.Write("Listening on ports: TCP ");
                                Console.Write(args[1]);
                                Console.Write(" and UDP ");
                                Console.WriteLine(args[2]);
                                ReceiverMain(int.Parse(args[1]), int.Parse(args[2]));
                            }
                            else
                            {
                                Console.Write("ERROR: Invalid TCP port defined");
                                return;
                            }
                        }
                        catch (Exception exc)
                        {
                            Console.Write("ERROR: " + exc.Message);
                            return;
                        }

                    }
                    else
                    {
                        Console.WriteLine("Usage as receiver: NetworkAudio.exe -r [TCPPort] [UDPPort] \nDefault ports are: 50001 for TCP (control) and 50000 for UDP (audio stream)");
                        Console.WriteLine("Usage as sender: NetworkAudio.exe -s <IP Address> [TCPPort] [UDPPort] \nDefault ports are: 50001 for TCP (control) and 50000 for UDP (audio stream)");
                        Console.ReadKey();
                        return;
                    }
                }
                else if ((args[0] == "-s") && (args.Length > 1))
                {
                    /* Sender / Client /*/
                    try
                    {
                        if (args.Length == 2)
                        {
                            Console.Write("Connecting to " + args[1] + " on ports: TCP 50001 (default)");
                            Console.WriteLine(" and UDP 50000 (default)");
                            SenderMain(args[1], 50001, 50000);
                        }
                        else if (args.Length == 3)
                        {
                            Console.Write("Connecting to " + args[1] + " on ports: TCP " + args[2]);
                            Console.WriteLine(" and UDP 50000 (default)");
                            SenderMain(args[1], int.Parse(args[2]), 50000);
                        }
                        else if (args.Length == 4)
                        {
                            Console.Write("Connecting to " + args[1] + " on ports: TCP " + args[2]);
                            Console.WriteLine(" and UDP " + args[3]);
                            SenderMain(args[1], int.Parse(args[2]), int.Parse(args[3]));
                        }
                        else
                        {
                            Console.WriteLine("Usage as receiver: NetworkAudio.exe -r [TCPPort] [UDPPort] \nDefault ports are: 50001 for TCP (control) and 50000 for UDP (audio stream)");
                            Console.WriteLine("Usage as sender: NetworkAudio.exe -s <IP Address> [TCPPort] [UDPPort] \nDefault ports are: 50001 for TCP (control) and 50000 for UDP (audio stream)");
                            Console.ReadKey();
                            return;
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.Write("ERROR: " + exc.Message);
                        return;
                    }
                    
                }
                else
                {
                    Console.WriteLine("Usage as receiver: NetworkAudio.exe -r [TCPPort] [UDPPort] \nDefault ports are: 50001 for TCP (control) and 50000 for UDP (audio stream)");
                    Console.WriteLine("Usage as sender: NetworkAudio.exe -s <IP Address> [TCPPort] [UDPPort] \nDefault ports are: 50001 for TCP (control) and 50000 for UDP (audio stream)");
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Usage as receiver: NetworkAudio.exe -r [TCPPort] [UDPPort] \nDefault ports are: 50001 for TCP (control) and 50000 for UDP (audio stream)");
                Console.WriteLine("Usage as sender: NetworkAudio.exe -s <IP Address> [TCPPort] [UDPPort] \nDefault ports are: 50001 for TCP (control) and 50000 for UDP (audio stream)");
                Console.ReadKey();
                return;
            }
            
        }

        static WasapiOut soundOut;

        static void ReceiverMain(int tcpPort, int udpPort)
        {
            //UdpReceiver soundIn;
            soundIn = new UdpReceiver(tcpPort, udpPort);
            soundIn.Initialize();

            IWaveSource source = new SoundInSource(soundIn) { FillWithZeros = true };

            soundIn.Start();

            //create a soundOut instance to play the data
            soundOut =  new WasapiOut();
            //initialize the soundOut with the echoSource
            //the echoSource provides data from the "source" and applies the echo effect
            //the "source" provides data from the "soundIn" instance
            soundOut.Initialize(source);

            //play
            soundOut.Play();

            /*System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 2000;
            aTimer.Enabled = true;*/

            Console.WriteLine("Receiving audio... Press any key to exit the program.");
            Console.ReadKey();
        }

        static void SenderMain(string IPAddress, int tcpPort, int udpPort)
        {
            UdpSender udpSend;
            soundIn = new WasapiLoopbackCapture(30, new WaveFormat(44100, 16, 2));
            {
                //important: always initialize the soundIn instance before creating the
                //SoundInSource. The SoundInSource needs the WaveFormat of the soundIn,
                //which gets determined by the soundIn.Initialize method.

                soundIn.Initialize();

                //wrap a sound source around the soundIn instance
                //in order to prevent playback interruptions, set FillWithZeros to true
                //otherwise, if the SoundIn does not provide enough data, the playback stops
                IWaveSource source = new SoundInSource(soundIn) { FillWithZeros = true };

                udpSend = new UdpSender(soundIn,IPAddress,tcpPort,udpPort);

                soundIn.DataAvailable += udpSend.AudioCaptureEvent;

                //start capturing data
                soundIn.Start();

                
                Console.WriteLine("Sending audio... Press any key to exit the program.");
                Console.ReadKey();
            }
        }
        /*
        static bool IsKodiRunning()
        {
            bool retVal_b = false;
            Process[] pname = Process.GetProcessesByName("notepad");
            if (pname.Length == 0)
                retVal_b = false;
            else
                retVal_b = true;

            return retVal_b;
        }

        static bool kodiStarted = false;

        // Specify what you want to happen when the Elapsed event is raised.
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (false == kodiStarted)
            {
                if (true == IsKodiRunning())
                {
                    Console.WriteLine("Kodi started.");
                    soundIn.Stop();
                    soundOut.Stop();
                    kodiStarted = true;
                }
            }
            else
            {
                if (false == IsKodiRunning())
                {
                    Console.WriteLine("Kodi closed.");
                    //play
                    soundIn.Start();
                    soundOut.Play();
                    kodiStarted = false;
                }
            }
            
        }*/

    }
}
