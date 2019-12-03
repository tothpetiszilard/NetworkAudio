using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetworkAudio
{
    public class UdpReceiver : CSCore.SoundIn.ISoundIn
    {
        //Client uses as receive udp client
        private UdpClient Client;
        private int _offset = 0;

        WaveFormat _waveFormat;

        TcpListener tcpSocket;

        RecordingState _recordingState = RecordingState.Stopped;
        /// <summary>
        /// Occurs when new data got captured and is available. 
        /// </summary>
        public event EventHandler<DataAvailableEventArgs> DataAvailable;

        /// <summary>
        /// Occurs when <see cref="WasapiCapture"/> stopped capturing audio.
        /// </summary>
        public event EventHandler<RecordingStoppedEventArgs> Stopped;

        public UdpReceiver (int tcpPort, int udpPort)
        {
            tcpSocket = new TcpListener(IPAddress.Any, tcpPort);
            Client = new UdpClient(udpPort);
        }

        public void Initialize()
        {
            tcpSocket.Start();
            TcpClient tcpClient = tcpSocket.AcceptTcpClient();
            while (tcpSocket.Server.Connected)
            {
                /* Client is sending data */
            }
            decodeWaveFormat(tcpClient);

            tcpSocket.BeginAcceptTcpClient(TcpRecv,null);
            Console.WriteLine("Wave format received, playing received audio stream.");
            Console.WriteLine("Channels: " + _waveFormat.Channels + " Sample rate: " + _waveFormat.SampleRate + " Resolution: " + _waveFormat.BitsPerSample);
        }

        private void decodeWaveFormat(TcpClient tcpClient)
        {
            BinaryFormatter b = new BinaryFormatter();
            object wf;
            wf = b.Deserialize(tcpClient.GetStream());
            if (((OwnWaveFormat)wf).WaveFormatTag == AudioEncoding.Extensible)
            {
                OwnWaveFormatExtensible owfx = (OwnWaveFormatExtensible)wf;
                _waveFormat = new WaveFormatExtensible(owfx.SampleRate, owfx.BitsPerSample, owfx.Channels, owfx.SubFormat, owfx.ChannelMask);
            }
            else
            {
                OwnWaveFormat owf = (OwnWaveFormat)wf;
                _waveFormat = new WaveFormat(owf.SampleRate, owf.BitsPerSample, owf.Channels, owf.WaveFormatTag, owf.ExtraSize);
            }
        }

        private void TcpRecv(IAsyncResult res)
        {
            TcpClient tcpClient = tcpSocket.EndAcceptTcpClient(res);
            while (tcpSocket.Server.Connected)
            {
                /* Client is sending data */
            }
            decodeWaveFormat(tcpClient);
            Console.WriteLine("Channels: " + _waveFormat.Channels + " Sample rate: " + _waveFormat.SampleRate + " Resolution: " + _waveFormat.BitsPerSample);
            Start();
            tcpSocket.BeginAcceptTcpClient(TcpRecv, null);
            _offset = 0;
            
        }

        private void UdpRecv(IAsyncResult res)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8000);
            byte[] received = Client.EndReceive(res, ref RemoteIpEndPoint);

            //_recordingState = RecordingState.Recording;
            //Process codes
            RaiseDataAvailable(received, _offset, received.Length);
            _offset += received.Length;

            Client.BeginReceive(new AsyncCallback(UdpRecv), null);
            // _recordingState = RecordingState.Stopped;

        }

        private void RaiseDataAvailable(byte[] buffer, int offset, int count)
        {
            if (count <= 0)
                return;

            EventHandler<DataAvailableEventArgs> handler = DataAvailable;
            if (handler != null)
            {
                var e = new DataAvailableEventArgs(buffer, offset, count, WaveFormat);
                handler(this, e);
            }
        }

        private void RaiseStopped(Exception exception)
        {
            EventHandler<RecordingStoppedEventArgs> handler = Stopped;
            if (handler != null)
            {
                var e = new RecordingStoppedEventArgs(exception);
                handler(this, e);
            }
        }

        public void Stop()
        {
            //stopping_b = true;
            
        }
        public void Start()
        {
            //stopping_b = false;
            try
            {
                Client.BeginReceive(new AsyncCallback(UdpRecv), null);
                
            }
            catch (Exception e)
            {
                RaiseStopped(e);
            }
        }

        public RecordingState RecordingState
        {
            get { return _recordingState; }
        }

        /// <summary>
        /// Gets the OutputFormat.
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return _waveFormat; }
        }

        /// <summary>
        /// Stops the capture and frees all resources.
        /// </summary>
        public void Dispose()
        {
            //Dispose(true);
            Client.Close();
            GC.SuppressFinalize(this);
        }

    }
}
