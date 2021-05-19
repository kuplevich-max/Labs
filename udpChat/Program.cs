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
        static List<string> history = new List<string>();
        static bool to_send_his = false;    


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
                    history.Add(localUser + ": " + message);
                    if (remoteUser == "")
                    {
                        message = "!" + message;
                    }
                    else
                    {
                        if (to_send_his)
                        {
                            to_send_his = false;
                            string s = "";
                            foreach (string str in history)
                            {
                                s += str + "\n";
                            }
                            message = s + message;
                        }
                    }                    
                    byte[] data = Encoding.Unicode.GetBytes($"{localUser}: {message}");
                    EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), remotePort);
                    listeningSocket.SendTo(data, remotePoint);                   

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

                    if(builder.ToString().Split(' ')[1][0] == '!')
                    {
                        to_send_his = true;
                        Console.WriteLine($"{builder.ToString()}");
                        continue;
                    }

                    if (remoteUser == "")
                    {
                        remoteUser = builder.ToString().Split(':')[0];

                        Console.WriteLine($"Подключение к {remoteUser}...";
                    }
                    
                    Console.WriteLine($"{builder.ToString()}");
                    history.Add(remoteUser + ": " + builder.ToString());                   
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


