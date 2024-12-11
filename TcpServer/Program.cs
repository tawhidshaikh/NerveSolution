// TCP Server
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

class TcpServer
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> DataCollection = new()
    {
        ["SetA"] = new ConcurrentDictionary<string, int> { ["One"] = 1, ["Two"] = 2 },
        ["SetB"] = new ConcurrentDictionary<string, int> { ["Three"] = 3, ["Four"] = 4 },
        ["SetC"] = new ConcurrentDictionary<string, int> { ["Five"] = 5, ["Six"] = 6 },
        ["SetD"] = new ConcurrentDictionary<string, int> { ["Seven"] = 7, ["Eight"] = 8 },
        ["SetE"] = new ConcurrentDictionary<string, int> { ["Nine"] = 9, ["Ten"] = 10 }
    };

    public static void Main()
    {
        TcpListener listener = new(IPAddress.Any, 8080);
        listener.Start();
        Console.WriteLine("Server started...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected...");
            ThreadPool.QueueUserWorkItem(ClientHandler, client);
        }
    }

    private static void ClientHandler(object state)
    {
        TcpClient client = (TcpClient)state;
        NetworkStream stream = client.GetStream();
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string encryptedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            string decryptedMessage = Decrypt(encryptedMessage);

            string[] parts = decryptedMessage.Split('-');
            if (parts.Length == 2 && DataCollection.TryGetValue(parts[0], out var subset) && subset.TryGetValue(parts[1], out int value))
            {
                for (int i = 0; i < value; i++)
                {
                    string timeResponse = Encrypt(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    byte[] timeData = Encoding.UTF8.GetBytes(timeResponse);
                    stream.Write(timeData, 0, timeData.Length);
                    Thread.Sleep(1000);
                }
            }
            else
            {
                string emptyResponse = Encrypt("EMPTY");
                byte[] emptyData = Encoding.UTF8.GetBytes(emptyResponse);
                stream.Write(emptyData, 0, emptyData.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            stream.Close();
            client.Close();
        }
    }

    private static string Encrypt(string plaintext)
    {
        // Use a basic symmetric encryption scheme (e.g., AES)
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes("1234567890123456"); // 16-byte key
        aes.IV = Encoding.UTF8.GetBytes("1234567890123456"); // 16-byte IV
        ICryptoTransform encryptor = aes.CreateEncryptor();

        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    private static string Decrypt(string ciphertext)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes("1234567890123456");
        aes.IV = Encoding.UTF8.GetBytes("1234567890123456");
        ICryptoTransform decryptor = aes.CreateDecryptor();

        byte[] ciphertextBytes = Convert.FromBase64String(ciphertext);
        byte[] decryptedBytes = decryptor.TransformFinalBlock(ciphertextBytes, 0, ciphertextBytes.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
