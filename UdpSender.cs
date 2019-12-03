using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Sockets;
using CSCore;
using CSCore.SoundIn;

namespace NetworkAudio
{
    class UdpSender
    {
        Socket udpSocket;
        OwnWaveFormat owf;
        WaveFormat oldFormat;
        IPAddress _ipAddress;
        int _tcpPort;

        public UdpSender(ISoundIn audioIf, string ipAddress, int tcpPort, int udpPort)
        {
            oldFormat = audioIf.WaveFormat;
            _ipAddress = IPAddress.Parse(ipAddress);
            _tcpPort = tcpPort;
            convertWaveFormat(oldFormat);
            sendWaveFormat();
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Connect(_ipAddress, udpPort);       
        }

        public void AudioCaptureEvent(object sender, DataAvailableEventArgs e)
        {
            if (oldFormat != e.Format)
            {
                convertWaveFormat(e.Format);
                sendWaveFormat();
            }
            udpSocket.Send(e.Data, e.ByteCount, SocketFlags.DontRoute);
        }

        private void convertWaveFormat (WaveFormat wf)
        {
            if (wf.WaveFormatTag == AudioEncoding.Extensible)
            {
                owf = new OwnWaveFormatExtensible(wf.SampleRate, wf.BitsPerSample, wf.Channels, ((WaveFormatExtensible)wf).SubFormat, ((WaveFormatExtensible)wf).ChannelMask);
            }
            else
            {
                owf = new OwnWaveFormat(wf.SampleRate, wf.BitsPerSample, wf.Channels, wf.WaveFormatTag, wf.ExtraSize);
            }
        }

        private void sendWaveFormat()
        {
            NetworkStream ns;
            BinaryFormatter b = new BinaryFormatter();
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(new IPEndPoint(_ipAddress, _tcpPort));
            ns = new NetworkStream(s);
            b.Serialize(ns, owf);
            ns.Close();
            s.Close();
        }

    }
}
