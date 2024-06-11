using System.Text.RegularExpressions;

namespace PacketGenerator;

internal class ReadWriteFile
{
    private string filePath;

    public ReadWriteFile(string filePath)
    {
        this.filePath = filePath;
    }

    public void MakeHandler(string clientPath)
    {
        MakeCSharpHandle();

        string packetHandler = clientPath + "PacketHandler.cs";

        System.IO.File.Copy("PacketHandler.cs", packetHandler, true);

        System.IO.File.Delete("PacketHandler.cs");
    }

    private void MakeCSharpHandle()
    {
        List<string> types = ReadFile(filePath);
        string[] packetHandlers = new string[1];
        string[] clientPacketHandlers = new string[1];

        foreach (string type in types)
        {
            string[] words = type.Split("_");

            string msgName = "";
            foreach (string word in words)
                msgName += FirstCharToUpper(word);

            // handler 포멧팅
            string packetName = $"S_{type}";
            packetHandlers[0] += string.Format(CSharpHandleFormat.handlerFormat, msgName, packetName);

            // 수행할 함수 포맷팅
            clientPacketHandlers[0] += string.Format(CSharpHandleFormat.clienthandleFormat, packetName);
        }

        string cSharpPacketHandler = "";

        foreach (string line in File.ReadAllLines("Templates/PacketHandler.cs"))
        {
            if (Regex.IsMatch(line, @"{[0-9]}"))
            {
                int n = line.IndexOf('{');
                char c = line[n + 1];
                int i = int.Parse(c.ToString());

                if (packetHandlers.Length > i)
                {
                    cSharpPacketHandler += packetHandlers[i];
                }
            }
            else
                cSharpPacketHandler += line;
            cSharpPacketHandler += "\n";
        }

        File.WriteAllText("PacketHandler.cs", cSharpPacketHandler);

        // Client PacketHandler 만들기
        //cSharpPacketHandler = "";

        //foreach (string line in File.ReadAllLines("../../../Templates/ClientPacketHandler.cs"))
        //{
        //    if (Regex.IsMatch(line, @"{[0-9]}"))
        //    {
        //        int n = line.IndexOf('{');
        //        char c = line[n + 1];
        //        int i = int.Parse(c.ToString());

        //        if (clientPacketHandlers.Length > i)
        //        {
        //            cSharpPacketHandler += clientPacketHandlers[i];
        //        }
        //    }
        //    else
        //        cSharpPacketHandler += line;
        //    cSharpPacketHandler += "\n";
        //}

        //File.WriteAllText("ClientPacketHandler.cs", cSharpPacketHandler);
    }

    public void MakeProto(string destPath)
    {
        List<string> types = ReadFile(filePath);
        string handle = "";
        Dictionary<string, string> clientInside = new Dictionary<string, string>();
        Dictionary<string, string> serverInside = new Dictionary<string, string>();

        bool isClient = false;
        bool isServer = false;

        if (File.Exists(destPath))
        {
            string subStr = "";
            foreach (string line in File.ReadAllLines(destPath))
            {
                if (isClient)
                {
                    if (line.Contains("{"))
                        continue;
                    else if (line.Contains("}"))
                    {
                        isClient = false;
                        continue;
                    }
                    else
                    {
                        clientInside[subStr] += line + "\n";
                    }
                }

                if (isServer)
                {
                    if (line.Contains("{"))
                        continue;
                    else if (line.Contains("}"))
                    {
                        isServer = false;
                        continue;
                    }
                    else
                    {
                        serverInside[subStr] += line + "\n";
                    }

                }

                if (line.Contains("C_"))
                {
                    isClient = true;
                    int index = line.IndexOf("C_");
                    subStr = line.Substring(index + 2);
                    if (types.Contains(subStr))
                    {
                        clientInside[subStr] = "";
                    }

                    continue;
                }
                else if (line.Contains("S_"))
                {
                    isServer = true;
                    int index = line.IndexOf("S_");
                    subStr = line.Substring(index + 2);
                    if (types.Contains(subStr))
                    {
                        serverInside[subStr] = "";
                    }

                    continue;
                }
            }
        }

        foreach (string type in types)
        {
            string client = "";
            string server = "";
            if (clientInside.ContainsKey(type))
            {
                if (clientInside[type].Length > 2)
                    client = clientInside[type].Substring(0, clientInside[type].Length - 1);
            }
            if (serverInside.ContainsKey(type))
            {
                if (serverInside[type].Length > 2)
                    server = serverInside[type].Substring(0, serverInside[type].Length - 1); ;
            }

            handle += String.Format(ProtoFormat.handleFormat, type, server, client);
        }

        string paths = Path.GetDirectoryName(destPath);

        string csharpText = string.Format(ProtoFormat.csharpFormat, handle);
        File.WriteAllText("ProtocolC.proto", csharpText);
        System.IO.File.Copy("ProtocolC.proto", paths + "\\ProtocolC.proto", true);


        System.IO.File.Delete("ProtocolC.proto");
    }

    private List<string> ReadFile(string filePath)
    {
        List<string> types = new List<string>();

        bool startParsing = false;
        foreach (string line in File.ReadAllLines(filePath))
        {
            if (!startParsing && line.Contains("enum INGAME"))
            {
                startParsing = true;
                continue;
            }

            if (!startParsing)
                continue;

            if (line.Contains("{"))
                continue;

            if (line.Contains("NULL"))
                continue;

            if (line.Contains("}"))
                break;

            string[] type = line.Trim().Split(" =");
            if (type.Length == 0)
                continue;

            types.Add(type[0]);
        }

        return types;
    }
    private static string FirstCharToUpper(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";
        return input[0].ToString().ToUpper() + input.Substring(1).ToLower();
    }
}