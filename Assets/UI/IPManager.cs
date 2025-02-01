using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
/// <summary>
/// Sluzi pre ziskavanie sietovych parametrov lokalneho pocitaca <br />
/// Vyzaduje si povolenie o pristupe k sieti pri spusteni aplikacie
/// </summary>
public static class IPManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Addfam"></param>
    /// <returns></returns>
    public static string GetIP(AddressForm Addfam)
    {
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == AddressForm.IPv6 && !Socket.OSSupportsIPv6)
        {
            return null;
        }

        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif 
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    //IPv4
                    if (Addfam == AddressForm.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }

                    //IPv6
                    else if (Addfam == AddressForm.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return output;
    }
    public enum AddressForm
    {
        IPv4, IPv6
    }
}