using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Revit_Automation.Source.Licensing
{
    internal class LicenseValidator
    {
        // Define the Request model
        public class RequestModel
        {
            public string HostName { get; set; }
            public string IPAddress { get; set; }
            public string ProductName { get; set; }
        }

        // Define the response model
        public class ResponseModel
        {
            public bool ValidLicense { get; set; }
        }

        // Function to get the local IP address
        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }

        public static bool ValidateLicense()
        {
            #if DEBUG
                return true;
            #endif

            TcpClient client = new TcpClient();
            bool isValidLicense = false;
            try
            {
                // Set the server's IP address and port number
                IPAddress serverIp = IPAddress.Parse("192.168.29.10");
                int serverPort = 8080;

                // Create a TCP client and connect to the server
                client.Connect(serverIp, serverPort);

                Console.WriteLine("Connected to server {0}:{1}", serverIp, serverPort);

                // Get the network stream from the client
                NetworkStream stream = client.GetStream();

                // Create the request model
                RequestModel request = new RequestModel
                {
                    HostName = Environment.MachineName,  //Get the machine name
                    IPAddress = GetLocalIPAddress(), //Get the local IP address in the current network
                    ProductName = "Auto Revit 2022"
                };

                // Serialize the request model to JSON
                string jsonRequest = JsonConvert.SerializeObject(request);

                // Convert the JSON request to bytes
                byte[] requestData = Encoding.UTF8.GetBytes(jsonRequest);

                // Send the request to the server
                stream.Write(requestData, 0, requestData.Length);
                Console.WriteLine("Sent: {0}", jsonRequest);

                // Receive the response from the server
                byte[] responseData = new byte[1024];
                int bytesRead = stream.Read(responseData, 0, responseData.Length);
                string jsonResponse = Encoding.UTF8.GetString(responseData, 0, bytesRead);

                // Deserialize the response JSON to the response model
                ResponseModel responseObject = JsonConvert.DeserializeObject<ResponseModel>(jsonResponse);

                isValidLicense = responseObject.ValidLicense;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: {0}", ex.Message);
            }
            finally
            {
                client.Close();
            }
            return isValidLicense;
        }
    }
}
