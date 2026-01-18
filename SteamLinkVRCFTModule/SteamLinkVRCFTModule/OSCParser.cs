using System.Collections;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SteamLinkVRCFTModule;

public class Oscm
{
    public string Address = "";

    public readonly ArrayList Values = new();

    //int = 0, float = 1, blob = 2, string = 3, error = -1
    private readonly List<int> _valType = new();

    private int GetAddress(ref byte[] msg, int index)
    {
        while (index < msg.Length)
        {
            if (msg[index] == 0x2c)
            {
                //Address = Encoding.ASCII.GetString(msg, 0, index - 1);
                index++;
                return index;
            }

            //This is sloppy, But need to verify if OSC spec is one null char on address or if it's up to 4?
            if (msg[index] == 0x00)
            {
                if (Address == "")
                {
                    Address = Encoding.ASCII.GetString(msg, 0, index);
                }
            }

            index++;
        }

        return -1;
    }

    public int GetParams(ref byte[] msg, int index, ILogger log)
    {
        var valmax = 0;
        //we start at the parameter not the "," in an osc string, and then we need
        // an additional to account for offset
        var typetagstart = index - 1;
        while (index < msg.Length)
        {
            switch (msg[index])
            {
                //null i.e. end of params
                case 0x00:
                    return (4 - ((index - typetagstart) % 4) + index);
                //float
                case 0x66:
                    _valType.Add(1);
                    valmax++;
                    break;
                //int
                case 0x69:
                    _valType.Add(0);
                    valmax++;
                    break;
                //blob
                case 0x62:
                    _valType.Add(2);
                    valmax++;
                    break;
                //string
                case 0x72:
                    _valType.Add(3);
                    valmax++;
                    break;
                default:
                    _valType.Add(-1);
                    break;
            }

            index++;
        }

        return -1;
    }

    private int GetValues(ref byte[] message, int i, ILogger log)
    {
        var valuecount = 0;
        var maxVal = _valType.Count();
        while (i < message.Length)
        {
            var msgsize = new byte[4];
            switch (_valType[valuecount])
            {
                case 0:
                    if (BitConverter.IsLittleEndian)
                    {
                        msgsize[0] = message[i];
                        msgsize[1] = message[i + 1];
                        msgsize[2] = message[i + 2];
                        msgsize[3] = message[i + 3];
                        Array.Reverse(msgsize);
                        Values.Add(BitConverter.ToInt32(msgsize, 0));
                    }
                    else
                    {
                        Values.Add(BitConverter.ToInt32(message, i));
                    }

                    i = i + 3;
                    valuecount++;

                    break;
                case 1:
                    if (BitConverter.IsLittleEndian)
                    {
                        msgsize[0] = message[i];
                        msgsize[1] = message[i + 1];
                        msgsize[2] = message[i + 2];
                        msgsize[3] = message[i + 3];
                        Array.Reverse(msgsize);
                        Values.Add(BitConverter.ToSingle(msgsize, 0));
                    }
                    else
                    {
                        Values.Add(BitConverter.ToSingle(message, i));
                    }

                    i = i + 3;
                    valuecount++;

                    break;
                case 2:
                    //TODO yea not happening (blob implementation)
                    break;
                case 3:
                    log.LogInformation("string");
                    var initialI = i;
                    while (message[i] != 0x00)
                    {
                        i++;
                    }

                    Values.Add(Encoding.ASCII.GetString(message, initialI, i));
                    valuecount++;
                    //OSC padding to 32 bit chunks (4 byte)
                    i = i + ((i - initialI) % 4);
                    //i++;

                    break;
                default:
                    log.LogInformation("error default XXXXXXXX");
                    //TODO error handling
                    break;
            }

            if (valuecount == maxVal)
            {
                return message.Length;
            }

            i++;
        }

        return -1;
    }

    public Oscm(ref byte[] message, ILogger iLogger)
    {
        var i = 0;
        i = GetAddress(ref message, i);
        if (i == -1)
        {
            iLogger.LogInformation("fail at addr");
            return;
        }

        i = GetParams(ref message, i, iLogger);
        if (i == -1)
        {
            iLogger.LogInformation("fail at param");
            return;
        }

        i = GetValues(ref message, i, iLogger);
        if (i == -1)
        {
            iLogger.LogInformation("fail at value");
            return;
        }
    }
}

public static class OscParser
{
    private static readonly byte[] BufAscii = "#bundle"u8.ToArray();

    public static bool IsBundle(ref byte[] buff)
    {
        if (buff == null || buff.Length < 8)
        {
            return false;
        }

        var bundletest = new byte[7];
        Array.Copy(buff, bundletest, 7);
        return bundletest.SequenceEqual(BufAscii);
    }

    public static uint SwapEndianness(uint x)
    {
        return ((x & 0x000000ff) << 24) + // First byte
               ((x & 0x0000ff00) << 8) + // Second byte
               ((x & 0x00ff0000) >> 8) + // Third byte
               ((x & 0xff000000) >> 24); // Fourth byte
    }

    public static int TrueMod(int a, int b)
    {
        return (Math.Abs(a * b) + a) % b;
    }
}