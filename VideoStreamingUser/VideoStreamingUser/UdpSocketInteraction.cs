using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VideoStreamingUser
{
    public class UdpSocketInteraction
    {
        private Socket udpSocket;
        private EndPoint remotePoint;

        private Dictionary<int, PackagesControler> allPackages;

        public UdpSocketInteraction()
        {
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            remotePoint = new IPEndPoint(IPAddress.Parse(Settings.address), Settings.port);

            allPackages = new Dictionary<int, PackagesControler>();

            udpSocket.Bind(remotePoint);
        }

        public byte[] GetByteArrayDataFromUdpSocket()
        {
            byte[] data = null;
            int recive_bytes = 0;
  
            do
            {
                byte[] reciveFullData = new byte[256];
                recive_bytes = udpSocket.ReceiveFrom(reciveFullData, ref remotePoint);

                byte[] reciveData = reciveFullData.Take(recive_bytes).ToArray();

                int packet_id = BitConverter.ToInt32(reciveData, 0);
                int current_package = BitConverter.ToInt32(reciveData, 4);
                int count_of_all_packages = BitConverter.ToInt32(reciveData, 8);

                byte[] imageData = reciveData.Skip(12).ToArray();

                PackageData packageData = new PackageData()
                {
                    current_package_id = current_package,
                    data = imageData,
                };


                if (allPackages.ContainsKey(packet_id))
                {
                    PackagesControler currentPackageControler = allPackages[packet_id];
                    currentPackageControler.PackagesList.Add(packageData);

                    if (currentPackageControler.PackagesList.Count == currentPackageControler.Count_of_all_packages)
                    {

                        data = currentPackageControler.PackagesList
                            .OrderBy(item => item.current_package_id)
                            .Select(item=>item.data)
                            .ToList()
                            .SelectMany(item => item)
                            .ToArray();

                        allPackages[packet_id] = null;
                    }

                }
                else
                {
                    PackagesControler currentPackageControler = new PackagesControler()
                    {
                        Count_of_all_packages = count_of_all_packages,
                        PackagesList = new List<PackageData>(),
                    };
                    currentPackageControler.PackagesList.Add(packageData);

                    allPackages[packet_id] = currentPackageControler;
                }
            }
            while (udpSocket.Available > 0);

            return data;
        }

    }
}
