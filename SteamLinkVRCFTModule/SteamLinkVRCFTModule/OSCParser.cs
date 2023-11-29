using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace SteamLinkVRCFTModule
{
    public class OSCM
    {
        public string Address = "";
        //int = 0, float = 1, blob = 2, string = 3, error = -1
        public List<Tuple<int, string>> Values = new List<Tuple<int, string>>();


        public int getAddress(ref byte[] msg, int index)
        {
            while (index < msg.Length)
            {
                if (msg[index] == 0x2c)
                {
                    Address = Encoding.ASCII.GetString(msg, 0, index - 1);
                    index++;
                    return index;
                }

                index++;
            }
            return -1;
        }

        public int getParams(ref byte[] msg, int index, ILogger log)
        {
            int valmax = 0;
            //we start at the parameter not the "," in an osc string, and then we need
            // an additional to account for offset
            int typetagstart = index-1;
            while (index < msg.Length)
            {
                switch (msg[index])
                {
                    //null i.e. end of params
                    case 0x00:
                        //TO BE CLEAR there is math for this but I can't be bothered atm
                        switch ((index - typetagstart) % 4)
                        {
                            case 0:
                                index = index + 4;
                                break;
                            case 1:
                                index = index + 3;
                                break;
                            case 2:
                                index = index + 2;
                                break;
                            case 3:
                                index = index + 1;
                                break;
                            default: return -1;
                        }
                        //log.LogInformation("Start  44: {1}, {2},{3},{4},{5}", msg[typetagstart], msg[index - 1], msg[index - 2], msg[index - 3], msg[index - 4]);
                        return index;
                    //float
                    case 0x66:
                        Values.Add(new Tuple<int, string>(1, ""));
                        valmax++;
                        break;
                    //int
                    case 0x69:
                        Values.Add(new Tuple<int, string>(0, ""));
                        valmax++;
                        break;
                    //blob
                    case 0x62:
                        Values.Add(new Tuple<int, string>(2, ""));
                        valmax++;
                        break;
                    //string
                    case 0x72:
                        Values.Add(new Tuple<int, string>(3, ""));
                        valmax++;
                        break;
                    default:
                        Values.Add(new Tuple<int, string>(-1, ""));
                        break;
                }
                index++;
            }
            return -1;
        }
        public int getValues(ref byte[] message, int i, ILogger log)
        {
            int valuecount = 0;
            int maxVal = Values.Count();
            while (i < message.Length)
            {
                switch (Values[valuecount].Item1)
                {
                    case 0:
                        log.LogInformation("int");
                        if (BitConverter.IsLittleEndian)
                        {
                            byte[] msgsize = new byte[4];
                            msgsize[0] = message[i]; msgsize[1] = message[i + 1]; msgsize[2] = message[i + 2]; msgsize[3] = message[i + 3];
                            Array.Reverse(msgsize);
                            Values[valuecount] = new Tuple<int, string>(0, BitConverter.ToInt32(msgsize, 0).ToString());
                        }
                        else
                        {
                            Values[valuecount] = new Tuple<int, string>(0, BitConverter.ToInt32(message, i).ToString());
                        }
                        i = i + 4;
                        valuecount++;

                        break;
                    case 1:
                        if (BitConverter.IsLittleEndian)
                        {
                            //log.LogInformation("little");
                            byte[] msgsize = new byte[4];
                            msgsize[0] = message[i]; msgsize[1] = message[i + 1]; msgsize[2] = message[i + 2]; msgsize[3] = message[i + 3];
                            Array.Reverse(msgsize);
                            Values[valuecount] = new Tuple<int, string>(0, BitConverter.ToSingle(msgsize, 0).ToString());
                        }
                        else
                        {
                            //log.LogInformation("big");
                            Values[valuecount] = new Tuple<int, string>(0, BitConverter.ToSingle(message, i).ToString());
                        }
                        i = i + 4;
                        valuecount++;

                        break;
                    case 2:
                        log.LogInformation("error BLOB");
                        //TODO yea not happening (blob implementation)
                        break;
                    case 3:
                        log.LogInformation("string");
                        int initialI = i;
                        while (message[i] != 0x00)
                        {
                            i++;
                        }
                        Values[valuecount] = new Tuple<int, string>(3, Encoding.ASCII.GetString(message, initialI, i));
                        valuecount++;
                        //OSC padding to 32 bit chunks (4 byte)
                        i = i + ((i-initialI) % 4);
                        i++;

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

        public OSCM(ref byte[] message, ILogger iLogger)
        {
            int i = 0;
            bool hasAddress = false;
            bool paramsComplete = false;
            int typetagstart = 0;
            int valuecount = 0;
            int valmax = 0;
            i = getAddress(ref message, i);
            if (i == -1)
            {
                iLogger.LogInformation("fail at addr");
                return;
            }
            i = getParams(ref message, i, iLogger);
            if (i == -1)
            {
                iLogger.LogInformation("fail at param");
                return;
            }
            //iLogger.LogInformation("before vals {1}, {2} ,{3}, {4}", message[i-1], message[i-2], message[i -3], message[i-4]);
            i = getValues(ref message, i, iLogger);
            if (i == -1)
            {
                iLogger.LogInformation("fail at value");
                return;
            }
            //iLogger.LogInformation("succ");
        }

    }

    static public class OSCParser
    {
        static readonly byte[] bufASCII = Encoding.ASCII.GetBytes("#bundle");
        public static bool IsBundle(ref byte[] buff)
        {
            if(buff == null)
            {
                return false;
            }
            byte[] bundletest = new byte[7];
            Array.Copy(buff, bundletest, 7);
            return Enumerable.SequenceEqual(bundletest, bufASCII);
        }
        public static uint swapEndianness(uint x)
        {
            return ((x & 0x000000ff) << 24) +  // First byte
                   ((x & 0x0000ff00) << 8) +   // Second byte
                   ((x & 0x00ff0000) >> 8) +   // Third byte
                   ((x & 0xff000000) >> 24);   // Fourth byte
        }
        public static int trueMod(int a, int b)
        {
            return (Math.Abs(a * b) + a) % b;
        }

    }
}
