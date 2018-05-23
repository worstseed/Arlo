using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using static System.String;

namespace Arlo.SerialCommunication
{
    public class PortChat
    {
        // Flag to enable serial read.
        private static bool _continue;
        public static SerialPort SerialPort;

        // Robot makes defined number of squares (372 for a full circle, 372/4 = 93 for 90 degrees).
        private const int NumberOfSquares = 4;
        private const int WriteSpeed = 80;
        public const int WriteDistance = 200;
        private const int WriteAngle = 93;

        // Not optimized, "safe" movement (serial commands do not interupt each other).
        public const int WaitTime = 3000;
        public const int ShortWaitTime = 1500;


        public static void Main()
        {
            // Thread for reading serial.
            var readThread = new Thread(Read);

            // COM port name for my machine.
            SerialPort = new SerialPort("COM5", 115200);

            // Set communication parameters (use defaults).
            SerialPort.PortName = SetPortName(SerialPort.PortName);
            SerialPort.BaudRate = SetPortBaudRate(SerialPort.BaudRate);
            SerialPort.Parity = SetPortParity(SerialPort.Parity);
            SerialPort.DataBits = SetPortDataBits(SerialPort.DataBits);
            SerialPort.StopBits = SetPortStopBits(SerialPort.StopBits);
            SerialPort.Handshake = SetPortHandshake(SerialPort.Handshake);

            Console.Clear();

            SerialPort.ReadTimeout = 500;
            SerialPort.WriteTimeout = 500;

            // Open communication via serial port.
            SerialPort.Open();
            // Enable serial read.
            _continue = true;
            // Start thread for reading.
            readThread.Start();

            // Redirect output to file "out.txt".
            var filestream = new FileStream("out.txt", FileMode.Create);
            var streamwriter = new StreamWriter(filestream) { AutoFlush = true };
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);

            Thread.Sleep(WaitTime);

            // Robot movement.
            for (var i = 0; i < 4 * NumberOfSquares; i++)
            {
                // rst - reset encoders.
                SerialPort.WriteLine(
                    Format("rst", WriteDistance, WriteDistance, WriteSpeed));
                Thread.Sleep(ShortWaitTime);

                // Move robot.
                SerialPort.WriteLine(
                    Format("MOVE {0} {1} {2}", WriteDistance, WriteDistance, WriteSpeed));
                Thread.Sleep(WaitTime);

                // dist - read encoders.
                Console.Write("READ_MOVE ");
                SerialPort.WriteLine(
                    Format("dist", WriteDistance, WriteDistance, WriteSpeed));
                Thread.Sleep(WaitTime);

                Console.WriteLine();

                SerialPort.WriteLine(
                    Format("rst", WriteDistance, WriteDistance, WriteSpeed));
                Thread.Sleep(ShortWaitTime);

                // Turn robot.
                SerialPort.WriteLine(
                    Format("TURN {0} {1}", WriteAngle, WriteSpeed));
                Thread.Sleep(ShortWaitTime);

                Console.Write("READ_TURN ");
                SerialPort.WriteLine(
                    Format("dist", WriteDistance, WriteDistance, WriteSpeed));
                Thread.Sleep(WaitTime);

                Console.WriteLine();
            }
            Thread.Sleep(WaitTime);

            // Disable serial communication.
            _continue = false;

            // Join threads.
            readThread.Join();
            // Close serial port.
            SerialPort.Close();
        }

        // Read serial port, clear information, write to file.
        /* Output should look like:
         * MOVE 200 200 
         * READ_MOVE 200 200         *
         * TURN 93 
         * READ_TURN 92 -91
         * 
         * first line is the command sent to robot,
         * second line is the rusult of the encoders read.
         */
        public static void Read()
        {
            string[] substrings = { "rst", "dist" };
            while (_continue)
            {
                try
                {
                    var message = SerialPort.ReadLine();
                    var write = true;

                    foreach (var sub in substrings)
                        if (message.Contains(sub)) write = false;

                    if (!write) continue;
                    message = message.Replace("\\r", Empty);
                    message = message.Replace(WriteSpeed.ToString(), Empty);
                    if (message.Length > 1)
                        Console.WriteLine(message);
                }
                catch (TimeoutException) { }
            }
        }

        public static string SetPortName(string defaultPortName)
        {
            Console.WriteLine("Available Ports:");
            foreach (var s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            var portName = Console.ReadLine();

            if (portName != null && (portName == "" || !(portName.ToLower()).StartsWith("com")))
            {
                portName = defaultPortName;
            }
            return portName;
        }
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
            var baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }
        public static Parity SetPortParity(Parity defaultPortParity)
        {
            Console.WriteLine("Available Parity options:");
            foreach (var s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
            var parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity, true);
        }
        public static int SetPortDataBits(int defaultPortDataBits)
        {
            Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
            var dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits.ToUpperInvariant());
        }
        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            Console.WriteLine("Available StopBits options:");
            foreach (var s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter StopBits value (None is not supported and \n" +
                          "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
            var stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        }
        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            Console.WriteLine("Available Handshake options:");
            foreach (var s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
            var handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        }
    }
}