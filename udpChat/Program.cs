using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;


namespace UdpChat
{
    class Program
    {
        static int localPort;
        static int remotePort;
        static Socket listeningSocket;
        static Dictionary<string, int[]> users = new Dictionary<string, int[]>();
        static string localUser;
        static string remoteUser = "";
        static string Path = "History";
        static string usersPath = "users.txt";
        static IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localPort);



        static void Main(string[] args)
        {
            DirectoryInfo dir = new DirectoryInfo(Path);
            if (!dir.Exists)
            {
                dir.Create();
            }
            ReadUsers();
            ChoseUser();
            Console.WriteLine($"Привет, {localUser}!");
            Console.WriteLine("Чтобы обмениваться сообщениями введите сообщение и нажмите Enter");
            Console.WriteLine();


            try
            {


                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Task listeningTask = new Task(Listen);
                listeningTask.Start();

                // Sending messages
                while (true)
                {
                    string message = Console.ReadLine();

                    byte[] data = Encoding.Unicode.GetBytes($"{localUser}: {message}");
                    EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), remotePort);
                    listeningSocket.SendTo(data, remotePoint);
                    if (remoteUser != "")
                    {
                        using (var sw = new StreamWriter($@"{Path}\{localUser}{remoteUser}", true, Encoding.Default))
                        {
                            sw.WriteLine($"{localUser}: {message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                Close();
                Console.ReadLine();
            }
        }

        static void ReadUsers()
        {
            var fs = new FileStream(usersPath, FileMode.OpenOrCreate);
            fs.Close();
            using (StreamReader sr = new StreamReader(usersPath))
            {
                string name;
                while ((name = sr.ReadLine()) != null)
                {
                    users[name] = new int[2] { Convert.ToInt32(sr.ReadLine()), Convert.ToInt32(sr.ReadLine()) };
                }
            }
        }

        static void ChoseUser()
        {
            Console.Write("Введите ваше имя: ");
            localUser = Console.ReadLine();
            if (users.ContainsKey(localUser))
            {
                localPort = users[localUser][0];
                remotePort = users[localUser][1];
            }
            else
            {
                Console.Write("Введите порт для приема сообщений: ");
                localPort = Int32.Parse(Console.ReadLine());
                Console.Write("Введите порт для отправки сообщений: ");
                remotePort = Int32.Parse(Console.ReadLine());
                users[localUser] = new int[2] { localPort, remotePort };
                using (StreamWriter sw = new StreamWriter(usersPath, true, Encoding.Default))
                {
                    sw.WriteLine(localUser);
                    sw.WriteLine(localPort);
                    sw.WriteLine(remotePort);
                }
            }
        }

        static void GetHistory()
        {
            FileStream fs = new FileStream($@"{Path}\{localUser}{remoteUser}", FileMode.OpenOrCreate);
            fs.Close();
            using (StreamReader sr = new StreamReader($@"{Path}\{localUser}{remoteUser}"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }


        private static void Listen()
        {
            try
            {

                IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localPort);
                listeningSocket.Bind(localIP);

                while (true)
                {
                    // receive message
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    byte[] data = new byte[256];

                    EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                    do
                    {
                        bytes = listeningSocket.ReceiveFrom(data, ref remoteIp);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (listeningSocket.Available > 0);

                    if (remoteUser == "")
                    {
                        remoteUser = builder.ToString().Split(':')[0];

                        Console.WriteLine($"Подключение к {remoteUser}...");
                        GetHistory();
                    }

                    Console.WriteLine($"{builder.ToString()}");
                    using (StreamWriter sw = new StreamWriter($@"{Path}\{localUser}{remoteUser}", true, Encoding.Default))
                    {
                        sw.WriteLine(builder.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        private static void Close()
        {
            if (listeningSocket != null)
            {
                listeningSocket.Shutdown(SocketShutdown.Both);
                listeningSocket.Close();
                listeningSocket = null;
            }
        }
    }
}


