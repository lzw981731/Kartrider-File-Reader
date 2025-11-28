using System.Net;
using System.Text;
using KartLibrary.Client;
using KartLibrary.IO;
using eP.Command;
using eP.Extension;
using eP.Testing;
using KartCity.Common.IO;
using KartCity.Common.IO.SmartStream;
using KartCity.Common.Xml;
using KartLibrary.Encrypt;
using KartLibrary.Xml;

namespace KartLibrary.Tests.Testing;

public class TestKartServer: TestStage
{
    public TestKartServer()
    {
        KartObjectManager.Initialize();
    }
    
    [Command("dumpPinObj")]
    private CommandExecuteResult commandDumpPinObj(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        string fileName = argumentQueue.PopArgumentString();
        using FileStream stream = new FileStream(fileName, FileMode.Open);
        BinaryReader reader = new BinaryReader(stream);
        int krDataLen = reader.ReadInt32();
        byte[] extractedSmartData = reader.ReadSmartStreamToBytes(krDataLen);
        using Stream pinStream = new MemoryStream(extractedSmartData);
        BinaryReader pinReader = new BinaryReader(pinStream);
        PinObject pinObject = pinReader.ReadKartObject<PinObject>(null);
        commandConsole.WriteLine("[PinObject]");
        commandConsole.WriteLine($"SzId:\t {pinObject.SzId}");
        commandConsole.WriteLine($"CountryCode:\t {pinObject.CountryCode}");
        commandConsole.WriteLine($"AltCountryCode:\t {pinObject.AlternateCountryCode}");
        commandConsole.WriteLine($"MajorId:\t {pinObject.MajorId}");
        commandConsole.WriteLine($"PkgVer:\t {pinObject.PackageVersion}");
        commandConsole.WriteLine($"ClientVer:\t {pinObject.ClientVersion}");
        commandConsole.WriteLine($"AccountType:\t {pinObject.AccountType}");
        commandConsole.WriteLine($"AccountParam:\t {pinObject.AccountParam}");
        commandConsole.WriteLine($"HomeUrl:\t {pinObject.HomeUrl}");
        commandConsole.WriteLine($"PatchUrl:\t {pinObject.PatchUrl}");
        foreach (ServerRegion region in pinObject.ServerRegions)
        {
            commandConsole.WriteLine("\t[ServerRegion]");
            commandConsole.WriteLine($"\tDesc:\t {region.Description}");
            commandConsole.WriteLine($"\tAccountConfig:\t {region.AccountConfig}");
            commandConsole.WriteLine($"\tUnknownTag:\t {region.UnknownTag}");
            foreach (IPEndPoint endPoint in region.ServerEndPoints)
            {
                commandConsole.WriteLine($"\t\t[EndPoints]: \t{endPoint}");
            }
        }
        commandConsole.WriteLine($"Storage:\n {pinObject.Storage}");
        commandConsole.WriteLine($"Extra:\n {pinObject.Extra}");

        pinObject.AccountType = 3;
        pinObject.SzId = 3001;
        pinObject.ServerRegions = new[]
        {
            pinObject.ServerRegions[0]
        };
        pinObject.ServerRegions[0].Description = "Default Server";

        pinObject.ServerRegions[0].ServerEndPoints = Enumerable.Range(0, 9)
            .Select(x => new IPEndPoint(IPAddress.Parse("192.168.0.152"), 39311)).ToArray();

        pinObject.Extra = 
            new BinaryXmlTag("extra")
                .AddContinue(new BinaryXmlTag("secModule")
                    .SetAttributeContinue("type", "1")
                )
                .AddContinue(new BinaryXmlTag("hideLoginDialogWithLoginParam"))
            ;
        using (FileStream fs = new FileStream(fileName + "_", FileMode.Create))
        {
            using MemoryStream memoryStream = new MemoryStream();
            BinaryWriter memWriter = new BinaryWriter(memoryStream);
            memWriter.Write(pinObject.ClassStamp);
            pinObject.EncodeObject(memWriter, null);

            BinaryWriter fileWriter = new BinaryWriter(fs);
            fileWriter.WriteAsSmartStreamData(memoryStream.ToArray(),SmartStreamMode.CompressedEncrypted, true, 0x39939339);
        }
        
        return new CommandExecuteResult(ResultType.Success, "");
    }

    [Command("dumpChStatic")]
    private CommandExecuteResult commandDumpChStatic(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        bool ss = false;
        if (argumentQueue.CommandArgumentType == CommandArgumentType.Option)
        {
            string option = argumentQueue.PopOption();
            if (option == "s")
            {
                commandConsole.WriteLine("Read size from file.");
                ss = true;
            }
        }
        string path = argumentQueue.PopArgumentString();
        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            BinaryReader reader = new BinaryReader(fs);
            if (ss)
            {
                SmartInStream smStream = reader.ReadSmartStream(reader.ReadInt32());
                reader = new BinaryReader(smStream);
            }

            List<(byte, string)> tmp1 = [];
            List<(short, string, byte, int)> tmp2 = [];
            int len1 = reader.ReadInt32();
            for(int i = 0; i < len1; i++)
                tmp1.Add((reader.ReadByte(), reader.ReadKRString()));
            int len2 = reader.ReadInt32();
            for(int i = 0; i < len2; i++)
                tmp2.Add((reader.ReadInt16(), reader.ReadKRString(), reader.ReadByte(), reader.ReadInt32()));
            foreach(var item in tmp1.OrderBy(x => x.Item1))
                commandConsole.WriteLine($"{item.Item1,-2} {Adler.Adler32(0, Encoding.Unicode.GetBytes(item.Item2)):x8} {item.Item2}");
            foreach(var item in tmp2.OrderBy(x => x.Item1))
                commandConsole.WriteLine($"{item.Item1,-2} {Adler.Adler32(0, Encoding.Unicode.GetBytes(item.Item2)):x8} {item.Item2} {item.Item3,-3} {item.Item4,-5}");
        }
        return new CommandExecuteResult(ResultType.Success, "");
    }

    [Command("decodeUdp")]
    private CommandExecuteResult CommandDecodeUdp(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        List<byte> tmpData = [];
        while(argumentQueue.Count > 0)
            tmpData.Add(byte.Parse(argumentQueue.PopArgumentString()));

        byte[] data = tmpData.ToArray();
        int key = BitConverter.ToInt32(data, 0);
        byte[] payload = data[4..^4];
        byte[] outData = new byte[payload.Length];
        int checksum = BitConverter.ToInt32(data[^4..], 0);

        int calCheckSum = PacketEncrypt.Decrypt(key, outData, 0, payload, 0, payload.Length) ^ 0x4f3816c3 ^ key;
        
        StringBuilder stringBuilder = new StringBuilder();
        
        if(calCheckSum != checksum)
            stringBuilder.AppendLine("Warning: checksum doesn't match.");

        stringBuilder.AppendHexView(outData);

        return new CommandExecuteResult(ResultType.Success, "");
    }
}