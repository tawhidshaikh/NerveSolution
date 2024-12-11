// TCP Client
using System;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

class TcpClientApp
{
    public static void Main()
    {
        Console.Write("Enter server IP: ");
        string serverIp = Console.ReadLine();
        Console.Write("Enter server port: ");
        int serverPort = int.Parse(Console.ReadLine());

        try
        {
            using TcpClient client = new(serverIp, serverPort);
            NetworkStream stream = client.GetStream();

            Console.Write("Enter message (e.g., SetA-Two): ");
            string message = Console.ReadLine();
            string encryptedMessage = Encrypt(message);

            byte[] data = Encoding.UTF8.GetBytes(encryptedMessage);
            stream.Write(data, 0, data.Length);

            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string encryptedResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string decryptedResponse = Decrypt(encryptedResponse);
                Console.WriteLine("Server: " + decryptedResponse);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static string Encrypt(string plaintext)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes("1234567890123456");
        aes.IV = Encoding.UTF8.GetBytes("1234567890123456");
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
