using PacketGenerator;
using System.Diagnostics;

string defaultPath = "../../";

ReadWriteFile readWriteFile = new ReadWriteFile(defaultPath + "protoc-21.12-win64/bin/Enum.proto");

string clientPath = defaultPath + "../Rolscape-Server/GameServer/ServerPacketHandler.h";
string generatePath = "..\\..\\protoc-21.12-win64\\bin\\GenPacketsCPlus.bat";

try
{
    readWriteFile.MakeHandler(clientPath);

    string destPath = defaultPath + "protoc-21.12-win64/bin/Protocol.proto";

    readWriteFile.MakeProto(destPath);

    Console.WriteLine("PacketGenerator Complete");

    ProcessStartInfo processInfo = new ProcessStartInfo(generatePath);
    processInfo.UseShellExecute = true;
    processInfo.WindowStyle = ProcessWindowStyle.Normal;

    using (Process? process = Process.Start(processInfo))
    {
        if (process == null)
            throw new Exception("Error Process");

        process.WaitForExit();
    }

    Console.WriteLine("Create Packet Success Enter");
    Console.Read();
}
catch (Exception e)
{
    Console.WriteLine($"PacketGenerator Error: {e}");

    Console.Read();
}