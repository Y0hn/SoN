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
    /// Ziska ip adresu lokalneho pocitaca
    /// </summary>
    /// <param name="Addfam"></param>
    /// <returns>IP_ADRESA textova forma</returns>
    public static string GetIP(AddressForm Addfam = AddressForm.IPv4)
    {
        // Ak je pozadovana IPv6 a OS ju nepodporuje, vrati "null"
        if (Addfam == AddressForm.IPv6 && !Socket.OSSupportsIPv6)
            return null;

        string output = "";

        // Prejde vsetky pripojovacie zariadenia na siet a zapise poslednu
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN // Ak je operacny system pocitaca Windows

            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;   // WiFi karta
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;        // pripojenie kablom

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) // kontrola ci interface parti do jednej z dvoch kategorii 
                    && 
                item.OperationalStatus == OperationalStatus.Up) // interface je zapnuty
#endif 
                // pre kazdu unicastovu adresu 
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    //IPv4
                    if (Addfam == AddressForm.IPv 
                            && 
                        ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            output = ip.Address.ToString();
                    //IPv6
                    else if (Addfam == AddressForm.IPv6 
                            &&
                        ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                            output = ip.Address.ToString();
                }
        }
        return output;
    }
    public enum AddressForm
    {
        IPv4, IPv6
    }
}