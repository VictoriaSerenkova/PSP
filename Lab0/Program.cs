using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace Lab0
{
    class Program
    {
        static int port = 8005; // порт для приема входящих запросов
        static void Main(string[] args)
        {
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("192.168.56.1"), port);

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(10);

                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[256]; // буфер для получаемых данных

                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);
                    double[,] x = JsonConvert.DeserializeObject<double[,]>(builder.ToString());
                    var request1 = $"{builder.ToString()}|".Replace("\0", "");
                    var request2 = "";
                    for(int i = 0; i < x.GetLength(0); i++)
                    {
                        for(int j = 0; j < x.GetLength(1); j++)
                        {
                            var minor = GetMinor(x, i, j);
                            if (i > x.GetLength(0) / 2 - 1 && j > x.GetLength(1) / 2 - 1) request2 += JsonConvert.SerializeObject(minor) +"|";
                            else request1 += JsonConvert.SerializeObject(minor) + "|";
                        }
                    }
                    var answer1 = Client.createClient(8010, "192.168.56.1", request1.Replace("\0", ""));
                    var answer2 = Client.createClient(8015, "192.168.56.1", request2.Replace("\0", ""));
                    string[] ranks1 = answer1.Split(",");
                    string[] ranks2 = answer2.Split(",");
                    double maxRank1 = Convert.ToDouble(ranks1[0]);
                    for(int i = 1; i < ranks1.Length - 1; i++)
                    {
                        if (Convert.ToDouble(ranks1[i]) > maxRank1) maxRank1 = Convert.ToDouble(ranks1[i]);
                    }
                    double maxRank2 = Convert.ToDouble(ranks2[0]);
                    for (int i = 1; i < ranks2.Length - 1; i++)
                    {
                        if (Convert.ToDouble(ranks2[i]) > maxRank2) maxRank2 = Convert.ToDouble(ranks2[i]);
                    }
                    double rank = 0;
                    if (maxRank1 > maxRank2) rank = maxRank1;
                    else rank = maxRank2;
                    // отправляем ответ
                    data = Encoding.UTF8.GetBytes(rank.ToString());
                    handler.Send(data);
                    // закрываем сокет
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static double[,] GetMinor(double[,] matrix, int row, int column)
        {
            if (matrix.GetLength(0) != matrix.GetLength(1)) throw new Exception(" Число строк в матрице не совпадает с числом столбцов");
            double[,] buf = new double[matrix.GetLength(0) - 1, matrix.GetLength(0) - 1];
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if ((i != row) || (j != column))
                    {
                        if (i > row && j < column) buf[i - 1, j] = matrix[i, j];
                        if (i < row && j > column) buf[i, j - 1] = matrix[i, j];
                        if (i > row && j > column) buf[i - 1, j - 1] = matrix[i, j];
                        if (i < row && j < column) buf[i, j] = matrix[i, j];
                    }
                }
            return buf;
        }
    }
}