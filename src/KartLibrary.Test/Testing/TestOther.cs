using System.Diagnostics;
using System.Globalization;
using System.Text;
using eP.Command;
using eP.Testing;
using KartCity.Common.IO.SmartStream;
using KartCity.Common.Xml;
using KartLibrary.Consts;
using KartLibrary.IO;
using KartLibrary.Xml;

namespace KartLibrary.Tests.Testing;

public class TestOther:TestStage
{
    [Command("decode_ss", "Decode SmartStream binary file.")]
    private CommandExecuteResult commandDecodeSmartStream(IConsole console, CommandArgumentQueue argumentQueue)
    {
        bool inSize = false;
        if (argumentQueue.CommandArgumentType == CommandArgumentType.Option)
        {
            string option = argumentQueue.PopOption();
            if (option == "s")
            {
                console.WriteLine("Read size from file.");
                inSize = true;
            }
        }
        string filePath = argumentQueue.PopArgumentString();
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            BinaryReader reader = new BinaryReader(fs);
            int size = inSize ? reader.ReadInt32() : (int)fs.Length;
            byte[] inData = reader.ReadBytes(size);
            byte[] outData = SmartStreamUtility.DecodeSmartStreamData(inData);
            SmartInStream smartInStream = new SmartInStream(inData);
            console.WriteLine($"{outData.Length} {smartInStream.Length}");
            System.IO.File.WriteAllBytes(Path.ChangeExtension(filePath, ".sout"), outData);
        }

        return new CommandExecuteResult(ResultType.Success, "");
    }
    
    [Command("try_decode_ss", "Try to decode really smart stream.")]
    private CommandExecuteResult commandTryDecodeSmartStream(IConsole console, CommandArgumentQueue argumentQueue)
    {
        string filePath = argumentQueue.PopArgumentString();
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            using (BufferedStream bufferedStream = new BufferedStream(fs))
            {
                while (true)
                {
                    
                }
                BinaryReader reader = new BinaryReader(bufferedStream);
                int size = (int)(fs.Length - fs.Position);
                byte[] inData = reader.ReadBytes(size);
                byte[] outData = SmartStreamUtility.DecodeSmartStreamData(inData);
                SmartInStream smartInStream = new SmartInStream(inData);
                console.WriteLine($"{outData.Length} {smartInStream.Length}");
                System.IO.File.WriteAllBytes(Path.ChangeExtension(filePath, ".sout"), outData);
            }
        }

        return new CommandExecuteResult(ResultType.Success, "");
    }
    
    [Command("start_kart", "")]
    private CommandExecuteResult commandStartKart(IConsole console, CommandArgumentQueue argumentQueue)
    {
        string path = argumentQueue.PopArgumentString();
        string passport = argumentQueue.PopArgumentString();
        Process process = new Process();
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.FileName = path;
        process.StartInfo.Arguments = $"-passport:{passport}";
        process.Start();
        while (!process.HasExited)
        {
            string? stdoutLine = process.StandardOutput.ReadLine();
            if(stdoutLine is not null)
                console.WriteLine(stdoutLine);
        }
        return new CommandExecuteResult(ResultType.Success, "");
    }
    
    [Command("bmlToXml")]
    private CommandExecuteResult commandBmlToXml(IConsole console, CommandArgumentQueue argumentQueue)
    {
        string path = argumentQueue.PopArgumentString();
        using (FileStream fileStream = new FileStream(path, FileMode.Open))
        {
            BinaryReader reader = new BinaryReader(fileStream);
            BinaryXmlTag binaryXmlTag = reader.ReadBinaryXmlTag(Encoding.Unicode);
            System.IO.File.WriteAllBytes(Path.ChangeExtension(path, ".xml"), new UnicodeEncoding(false, false).GetBytes(binaryXmlTag.ToString()));
        }
        return new CommandExecuteResult(ResultType.Success, "");
    }

    [Command("decodeSpec")]
    private CommandExecuteResult CommandDecodeSpec(IConsole console, CommandArgumentQueue argumentQueue)
    {
        List<byte> tmpList = [];
        while(argumentQueue.Count > 0)
            tmpList.Add(byte.Parse(argumentQueue.PopArgumentString(), NumberStyles.HexNumber));

        byte[] data = tmpList.ToArray();

        string result = data.Length switch
        {
            1 => $"{KartSpecEncode.DecodeByte(data[0]):x2}",
            2 => $"{KartSpecEncode.DecodeInt16(BitConverter.ToInt16(data)):x4}",
            4 => $"{KartSpecEncode.DecodeInt32(BitConverter.ToInt32(data)):x4}",
            _ => throw new Exception()
        };
        
        console.WriteLine($"{result}");

        return new CommandExecuteResult(ResultType.Success, "");
    }
}