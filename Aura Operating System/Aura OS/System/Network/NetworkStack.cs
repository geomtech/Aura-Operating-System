﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Network Intialization + Packet Handler
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using Aura_OS.HAL.Drivers.Network;
using Aura_OS.System.Network.ARP;
using Aura_OS.System.Network.IPV4;

namespace Aura_OS.System.Network
{
    /// <summary>
    /// Implement a Network Stack for all network devices and protocols
    /// </summary>
    public static class NetworkStack
    {
        internal static TempDictionary<NetworkDevice> AddressMap { get; private set; }

        /// <summary>
        /// Initialize the Network Stack to prepare it for operation
        /// </summary>
        public static void Init()
        {
            AddressMap = new TempDictionary<NetworkDevice>();

            // VMT Scanner issue workaround
            ARPPacket.VMTInclude();
            ARPPacket_Ethernet.VMTInclude();
            ARPReply_Ethernet.VMTInclude();
            ARPRequest_Ethernet.VMTInclude();
            ICMPPacket.VMTInclude();
            ICMPEchoReply.VMTInclude();
            ICMPEchoRequest.VMTInclude();
            IPV4.UDP.UDPPacket.VMTInclude();
            IPV4.TCP.TCPPacket.VMTInclude();
        }

        /// <summary>
        /// Configure a IP configuration on the given network device.
        /// <remarks>Multiple IP Configurations can be made, like *nix environments</remarks>
        /// </summary>
        /// <param name="nic"><see cref="NetworkDevice"/> that will have the assigned configuration</param>
        /// <param name="config"><see cref="IPV4.Config"/> instance that defines the IP Address, Subnet
        /// Mask and Default Gateway for the device</param>
        public static void ConfigIP(NetworkDevice nic, IPV4.Config config)
        {
            NetworkConfig.Add(nic, config);
            Console.WriteLine("Config added in dictionnary");
            AddressMap.Add(config.IPAddress.Hash, nic);
            IPV4.Config.Add(config);
            nic.DataReceived = HandlePacket;
        }

        internal static void HandlePacket(byte[] packetData)
        {
            Kernel.debugger.Send("Packet Received Length=");
            if (packetData == null)
            {
                Console.WriteLine("Error packet data null");
                return;
            }
            Kernel.debugger.Send(packetData.Length.ToString());

            UInt16 etherType = (UInt16)((packetData[12] << 8) | packetData[13]);
            switch (etherType)
            {
                case 0x0806:
                    ARPPacket.ARPHandler(packetData);
                    break;
                case 0x0800:
                    IPV4.IPPacket.IPv4Handler(packetData);
                    break;
            }
        }

        /// <summary>
        /// Called continously to keep the Network Stack going.
        /// </summary>
        public static void Update()
        {
            IPV4.OutgoingBuffer.Send();
        }
    }
}
