using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Net.NetworkInformation;

using UnityEngine;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;
using System.Diagnostics;

public class TauTrackerClient : MonoBehaviour
{
    public float trackerPositionFactor = 1200.0f;

    public Text visualLog;
    public int counterLog = 0;
    

    private int frames = 0;

    byte[] cached_last_packet = new byte[0];
    private DataPacket packet = new DataPacket();
    Dictionary<byte, List<string>> mapping = new Dictionary<byte, List<string>>();
    private bool wasPaused = false;

    public enum IKEnum
    {
        Disable,
        DebugLines,
        CapsulePrimitive,
    };
    public IKEnum InverseKinematicsType = IKEnum.CapsulePrimitive;

    UDPConnectionController connection;

    public static TauTrackerClient Instance { get; private set; }

    public enum SensorEnum
    {
        RightHand, RightHandThumb, RightHandIndex, RightHandMiddle, RightHandRing, RightHandPinky,
        LeftHand, LeftHandThumb, LeftHandIndex, LeftHandMiddle, LeftHandRing, LeftHandPinky
    };

    public byte GetSensorId(SensorEnum sensor)
    {
        switch (sensor)
        {
            case SensorEnum.RightHand:
                return 1;
            case SensorEnum.RightHandThumb:
                return 2;
            case SensorEnum.RightHandIndex:
                return 3;
            case SensorEnum.RightHandMiddle:
                return 4;
            case SensorEnum.RightHandRing:
                return 5;
            case SensorEnum.RightHandPinky:
                return 6;
            case SensorEnum.LeftHand:
                return 7;
            case SensorEnum.LeftHandThumb:
                return 8;
            case SensorEnum.LeftHandIndex:
                return 9;
            case SensorEnum.LeftHandMiddle:
                return 10;
            case SensorEnum.LeftHandRing:
                return 11;
            case SensorEnum.LeftHandPinky:
                return 12;
            default:
                return 0;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartClient();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public SensorData GetSensorData(SensorEnum sensor)
    {
        if (packet != null)
        {
            SensorData sensorData = packet.GetSensorById(GetSensorId(sensor));
            return sensorData;
        }
        else
        {
            return null;
        }
    }

    void Update()
    {


        if (Input.GetKeyDown(KeyCode.R))
        {
            Vector3 v = gameObject.transform.rotation.eulerAngles;
            gameObject.transform.rotation = Quaternion.Euler(v.x, v.y + 10.0f, v.z);
        }

        if (visualLog != null && visualLog.IsActive())
        {
            counterLog++;
            visualLog.text = "Update() packet read: " + counterLog.ToString() + "\r\n" +
                 "thread effective packet read: " + connection.connection.readerCounter + "\r\n" +
                 "thread packets per second: " + connection.connection.pps + "\r\n" +
                 "stopwatch ms timer: " + connection.connection.stopWatch.ElapsedMilliseconds + "\r\n";// +
                 //BitConverter.ToString(connection.GetMessage());
        }

        if (connection != null)
        {

            byte[] raw_packet = connection.GetMessage();
            if (raw_packet == cached_last_packet)
            {
                packet = null;
                return;
            }
            cached_last_packet = raw_packet;
            packet = new DataPacket(raw_packet, mapping);
            //Console.WriteLine(packet);

            frames++;
            if (frames % 10 == 0)
            {
                mapping = connection.GetMapping();
            }
        }

    }

    //void FixedUpdate(){}

    void StartClient()
    {
        connection = new UDPConnectionController();
        connection.Start();
    }


    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) //paused
        {
            Debug.Log("[TAU]Pausing client: OnApplicationPause");
            wasPaused = true;
            connection.SetIdleStatus(true);
        }
        else
        { //resumed
            if (wasPaused)
            {
                connection.SetIdleStatus(false);
                wasPaused = false;
                Debug.Log("[TAU]Resuming client: OnApplicationPause");
            }
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("[TAU]Closing client: OnApplicationQuit");
        //if (uclient != null) { uclient.Close(); }
        connection.Destroy();
    }

}




public class DataPacket
{
    public Dictionary<string, SensorData> sensors = new Dictionary<string, SensorData>();
    Dictionary<byte, List<string>> mapping;
    public DataPacket() { }


    public DataPacket(byte[] content, Dictionary<byte, List<string>> _mapping)
    {
        int bytecounter = 0;
        int modules = (int)content[bytecounter];
        mapping = _mapping;
        List<Vector3> bones = null;

        for (int i = 0; i < modules; i++)
        {
            string serial = BitConverter.ToInt16(content, i == 0 ? bytecounter += 1 : bytecounter += 4).ToString("X");
            //Console.WriteLine(serial);
            char[] sensors_active = Convert.ToString(content[bytecounter += 2], 2).PadLeft(8, '0').ToCharArray();
            char[] data_integrity = Convert.ToString(content[bytecounter += 1], 2).PadLeft(8, '0').ToCharArray();
            //Convert.ToString(content[bytecounter += 1], 2).PadLeft(8, '0').ToCharArray();
            //bytecounter += 1;

            for (int s = 0; s < 6; s++)
            {
                float q0 = 1.0f, q1 = 0, q2 = 0, q3 = 0, x = 0, y = 0, z = 0;

                string sens_id = serial + s.ToString();
                bool active = sensors_active[7 - s] == '1' ? true : false;
                bool bad_coords = data_integrity[7 - s] == '1' ? true : false;
                if (active)
                {
                    q0 = BitConverter.ToSingle(content, s == 0 ? bytecounter += 1 : bytecounter += 4);
                    q1 = BitConverter.ToSingle(content, bytecounter += 4);
                    q2 = BitConverter.ToSingle(content, bytecounter += 4);
                    q3 = BitConverter.ToSingle(content, bytecounter += 4);
                    x = BitConverter.ToSingle(content, bytecounter += 4);
                    y = BitConverter.ToSingle(content, bytecounter += 4);
                    z = BitConverter.ToSingle(content, bytecounter += 4);

                    int bonelength = BitConverter.ToInt32(content, bytecounter += 4);

                    if (bonelength > 0)
                    {
                        bones = new List<Vector3>();
                        for (int bl = 0; bl < bonelength; bl++)
                        {
                            Vector3 bone = new Vector3(
                                BitConverter.ToSingle(content, bytecounter += 4),
                                BitConverter.ToSingle(content, bytecounter += 4),
                                BitConverter.ToSingle(content, bytecounter += 4)
                            );
                            bones.Add(bone);
                        }
                    }
                }

                SensorData sd = new SensorData(sens_id, "", active, bad_coords, q0, q1, q2, q3, x, y, z, bones);
                sensors[sens_id] = sd;
                //Debug.Log(sens_id + " | " + active + " | " + q0 + " | " + q1 + " | " + q2 + " | " + q3 + " | " + x + " | " + y + " | " + z);

            }

        }

    }

    public SensorData GetSensorById(byte sId)
    {
        //Debug.Log(sId);
        if (mapping != null && mapping.ContainsKey(sId))
        {
            //Debug.Log(mapping[sId][0]);
            foreach (string map_sens_id in mapping[sId])
            {

                if (sensors.ContainsKey(map_sens_id))
                {
                    if (!sensors[map_sens_id].bad_coords) { 
                        return sensors[map_sens_id];
                    }
                }
            }

            // scenario if no non-bad_coords sensors found (return still didnt happen) - return first match
            foreach (string map_sens_id in mapping[sId])
            {
                if (sensors.ContainsKey(map_sens_id))
                {
                    return sensors[map_sens_id];
                }
            }


        }
        return null;
    }
}




public class SensorData
{

    public string id;
    public string name;
    public bool active;
    public bool bad_coords;


    // Quaternion
    public float q0;
    public float q1;
    public float q2;
    public float q3;

    // acceleration 
    //public float ax;
    //public float ay;
    //public float az;

    // position
    public float x;
    public float y;
    public float z;

    public int ik_data_counter = 0;
    public List<Vector3> ik_data;// = new List<Vector3>();

    public SensorData()
    {
        name = "stub";
        active = false;

        q0 = 0.0f;
        q1 = 0.0f;
        q2 = 0.0f;
        q3 = 0.0f;

        x = 0.0f;
        y = 0.0f;
        z = 0.0f;
    }

    public SensorData(String data)
    {
        String[] t = data.Split(' ');

        name = t[0];
        active = t[1] == "1" ? true : false;

        q0 = float.Parse(t[2]);
        q1 = float.Parse(t[3]);
        q2 = float.Parse(t[4]);
        q3 = float.Parse(t[5]);

        x = float.Parse(t[6]);
        y = float.Parse(t[7]);
        z = float.Parse(t[8]);


    }

    public SensorData(string _id, string _name, bool _active, bool _bad_coords, float _q0, float _q1, float _q2, float _q3, float _x, float _y, float _z, List<Vector3> bones = null)
    {
        id = _id; name = _name; active = _active; bad_coords = _bad_coords; q0 = _q0; q1 = _q1; q2 = _q2; q3 = _q3; x = _x; y = _y; z = _z;
        if (bones != null)
        {
            ik_data = bones;
            ik_data_counter = bones.Count;
        }
    }

    //translated to unity notion
    public Vector3 GetPosition()
    {
        return new Vector3(-y, z, x);
    }

    //translated to unity notion
    public Quaternion GetRotation()
    {
        return new Quaternion(q2, -q3, -q1, q0);
    }

}


public class UdpConnection
{
    private UdpClient udpClient;
    private UdpClient udpClientMapping;

    public int readerCounter = 0;
    public int pps = 0; //packets-per-second
    private int currentpps = 0;
    public Stopwatch stopWatch = new Stopwatch();

    private byte[] receivedBytes = new byte[64];
    Dictionary<byte, List<string>> mapping = new Dictionary<byte, List<string>>();
    Thread receiveThread, receiveMappingThread;
    private bool threadRunning = false;
    public bool idle = false;

    public byte GetMappingId(string tag)
    {
        switch (tag)
        {
            case "right_arm.hand":
                return 1;
            case "right_arm.hand.thumb":
                return 2;
            case "right_arm.hand.index":
                return 3;
            case "right_arm.hand.middle":
                return 4;
            case "right_arm.hand.ring":
                return 5;
            case "right_arm.hand.pinky":
                return 6;
            case "left_arm.hand":
                return 7;
            case "left_arm.hand.thumb":
                return 8;
            case "left_arm.hand.index":
                return 9;
            case "left_arm.hand.middle":
                return 10;
            case "left_arm.hand.ring":
                return 11;
            case "left_arm.hand.pinky":
                return 12;
            default:
                return 0;
        }
    }

    public void StartConnection()
    {
        try
        {
            udpClient = new UdpClient();
            udpClientMapping = new UdpClient();
        }
        catch (Exception e)
        {
            Debug.LogError("[TAU]Failed to listen for UDP" + e.Message);
            return;
        }
        Debug.Log("[TAU]Created receiving client");

        //udpClient.Client.Blocking = false;
        udpClient.Client.ReceiveTimeout = 20;
        StartReceiveThread();

        udpClientMapping.Client.ReceiveTimeout = 6000;
        StartMappingReceiveThread();
    }

    private void StartReceiveThread()
    {
        receiveThread = new Thread(() => ListenForMessages(udpClient));
        receiveThread.Priority = System.Threading.ThreadPriority.Highest;
        //receiveThread.IsBackground = true;
        threadRunning = true;
        receiveThread.Start();
        Debug.Log("[TAU]Receive thread started");
    }

    private void StartMappingReceiveThread()
    {
        receiveMappingThread = new Thread(() => ListenForMessagesMapping(udpClientMapping));
        //threadRunning = true;
        receiveMappingThread.Start();
        Debug.Log("[TAU]Receive mapping thread started");
    }

    private void ListenForMessages(UdpClient client)
    {

        stopWatch.Start();
        //Debug.Log("LISTENER READY");
        client.ExclusiveAddressUse = false;
        IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 6000);


        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        //client.ExclusiveAddressUse = false;

        client.Client.Bind(localEp);

        IPAddress multicastaddress = IPAddress.Parse("239.255.255.252");

        if (System.Environment.OSVersion.ToString().ToLower().Contains("windows")) //why, Microsoft, WHY
        {
            Debug.Log("[TAU]Windows OS detected. Going the hard way...");
            //client.JoinMulticastGroup(multicastaddress);
            /* 
            * the beginning of a glorious multicast workaround, mostly needed for Windows.
            * instead of all this, there's just got to be a line "client.JoinMulticastGroup(multicastaddress);"
            * but it works awfully unstable because of metric issue
            * see https://personalnexus.wordpress.com/2015/08/02/multicast-messages-on-windows-server-2008-r2-microsoft-failover-cluster/
            */

            List<System.Net.IPAddress> NICaddresses = new List<System.Net.IPAddress>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.SupportsMulticast)
                {
                    foreach (var MAaddress in ni.GetIPProperties().UnicastAddresses)
                    {
                        string adr = MAaddress.Address.ToString();
                        if (adr != "127.0.0.1" && !adr.Contains("::") && !NICaddresses.Contains(MAaddress.Address))
                        {
                            NICaddresses.Add(MAaddress.Address);
                        }
                    }
                    foreach (var MAaddress in ni.GetIPProperties().MulticastAddresses)
                    {
                        string adr = MAaddress.Address.ToString();
                        if (adr != "127.0.0.1" && !adr.Contains("::") && !NICaddresses.Contains(MAaddress.Address))
                        {
                            NICaddresses.Add(MAaddress.Address);
                        }
                    }
                }
            }
            foreach (var NICaddress in NICaddresses)
            {
                try { client.JoinMulticastGroup(multicastaddress, NICaddress); }
                catch (System.Net.Sockets.SocketException e) { /*Debug.Log("[TAU]System.Net.Sockets.SocketException: " + e.Message);*/ }
                catch (Exception e) { /*Debug.Log("[TAU]Exception: " + e.Message);*/ }
            }

            /* 
             * glorious multicast workaround is over.
            */
        }
        else //for EVERY OTHER OS. A line of code that just has to work, just works. Do you read me, Microsoft?
        {
            client.JoinMulticastGroup(multicastaddress);
        }



        //Debug.Log("LISTENER READY");

        Byte[] received = new byte[] { };

        while (threadRunning)
        {

            if (idle)
            {
                Thread.Sleep(200);
                while (client.Available > 0)
                {
                    client.Receive(ref localEp); // idle-flushing socket buffer, one way or another. I guess disconnect-reconnect would be preferable, but it's not working right now for whatever reason
                    currentpps++;
                    Thread.Sleep(1);
                }
                continue;
            }
            try
            {
                if (client.Available > 0)
                {
                    //while (client.Available > 0)
                    //{
                        received = client.Receive(ref localEp); // Blocks until a message returns on this socket from a remote host.
                        currentpps++;
                    //    Thread.Sleep(1);
                    //}
                    if (received != null && received.Length != 0)
                    {
                        
                        lock (receivedBytes)
                        {
                            readerCounter++;
                            receivedBytes = received;
                        }

                    }

                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode != 10004) Debug.LogError("[TAU]Socket exception while receiving data from udp client: " + e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError("[TAU]Error receiving data from udp client: " + e.Message);
            }

            if (stopWatch.ElapsedMilliseconds >= 1000)
            {
                pps = currentpps;
                currentpps = 0;
                stopWatch = new Stopwatch();
                stopWatch.Start();
            }
            Thread.Sleep(6);
        }
    }

    private void ListenForMessagesMapping(UdpClient client)
    {
        client.ExclusiveAddressUse = false;
        IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 5999);
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        client.Client.Bind(localEp);
        IPAddress multicastaddress = IPAddress.Parse("239.255.255.251");

        if (System.Environment.OSVersion.ToString().Contains("Windows"))
        {

            //client.JoinMulticastGroup(multicastaddress);
            /* 
            * the beginning of a glorious multicast workaround, mostly needed for Windows.
            * instead of all this, there's just got to be a line "client.JoinMulticastGroup(multicastaddress);"
            * but it works awfully unstable because of metric issue
            * see https://personalnexus.wordpress.com/2015/08/02/multicast-messages-on-windows-server-2008-r2-microsoft-failover-cluster/
            */


            List<System.Net.IPAddress> NICaddresses = new List<System.Net.IPAddress>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.SupportsMulticast)
                {
                    foreach (var MAaddress in ni.GetIPProperties().UnicastAddresses)
                    {
                        string adr = MAaddress.Address.ToString();
                        if (adr != "127.0.0.1" && !adr.Contains("::") && !NICaddresses.Contains(MAaddress.Address))
                        {
                            NICaddresses.Add(MAaddress.Address);
                        }
                    }
                    foreach (var MAaddress in ni.GetIPProperties().MulticastAddresses)
                    {
                        string adr = MAaddress.Address.ToString();
                        if (adr != "127.0.0.1" && !adr.Contains("::") && !NICaddresses.Contains(MAaddress.Address))
                        {
                            NICaddresses.Add(MAaddress.Address);
                        }
                    }
                }
            }
            foreach (var NICaddress in NICaddresses)
            {
                try { client.JoinMulticastGroup(multicastaddress, NICaddress); }
                catch (System.Net.Sockets.SocketException) { }
                catch (Exception) { }
            }

            /* 
                * glorious multicast workaround is over.
            */

        }
        else
        {
            client.JoinMulticastGroup(multicastaddress);
        }



        Byte[] received = new byte[] { };

        while (threadRunning)
        {
            if (idle)
            {
                Thread.Sleep(200);
                while (client.Available > 0)
                {
                    client.Receive(ref localEp); // idle-flushing socket buffer, one way or another. I guess disconnect-reconnect would be preferable, but it's not working right now for whatever reason
                }
                continue;
            }
            try
            {
                if (client.Available > 0)
                {
                    while (client.Available > 0)
                    {
                        received = client.Receive(ref localEp); // Blocks until a message returns on this socket from a remote host.
                    }
                    if (received != null && received.Length != 0)
                    {
                        string converted = Encoding.UTF8.GetString(received, 0, received.Length);
                        //Debug.Log(converted);

                        var _mapping = new Dictionary<byte, List<string>>();
                        using (StringReader reader = new StringReader(converted))
                        {
                            string line = string.Empty;
                            do
                            {
                                line = reader.ReadLine();
                                if (line != null)
                                {
                                    var spl_line = line.Split('=');
                                    var mapid = GetMappingId(spl_line[1].Replace("\"", ""));
                                    var sensid = spl_line[0];
                                    if (!_mapping.ContainsKey(mapid))
                                    {
                                        _mapping.Add(mapid, new List<string> { sensid.ToUpper() });
                                    }
                                    else
                                    {
                                        _mapping[mapid].Add(sensid.ToUpper());
                                    }
                                }

                            } while (line != null);
                        }
                        lock (mapping)
                        { mapping = _mapping; }
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode != 10004) Debug.Log("[TAU]Socket exception while receiving data from udp client (mapping): " + e.Message);
            }
            catch (Exception e)
            {
                Debug.Log("[TAU]Error receiving data from udp client (mapping): " + e.Message);
            }
            Thread.Sleep(100);
        }
    }

    public byte[] GetMessage()
    {
        lock (receivedBytes) { return receivedBytes; }
    }

    public Dictionary<byte, List<string>> GetMapping()
    {
        lock (mapping) { return mapping; }
    }

    public void Stop()
    {
        udpClient.Close();
        udpClientMapping.Close();
        receiveThread = receiveMappingThread = null;
        threadRunning = false;

    }
}

class UDPConnectionController
{
    public UdpConnection connection;

    public void Start()
    {
        connection = new UdpConnection();
        connection.StartConnection();
        Debug.Log("[TAU]Controller started UDP");
    }

    public void SetIdleStatus(bool idle_status)
    {
        connection.idle = idle_status;
    }

    public byte[] GetMessage()
    {
        return connection.GetMessage();
    }


    public Dictionary<byte, List<string>> GetMapping()
    {
        return connection.GetMapping();
    }

    public void Destroy()
    {
        connection.Stop();
    }
}
