using KartLibrary.Consts;
using KartLibrary.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using KartCity.Common.Client;
using KartCity.Common.Consts;
using KartCity.Common.IO;
using KartCity.Common.Xml;
using KartLibrary.Xml;

namespace KartLibrary.Client
{
    [KartObjectImplement]
    public class PinObject : KartObject
    {
        public override string ClassName => "PinObject";

        public byte Type { get; set; }
        public short SzId { get; set; }
        public CountryCode CountryCode { get; set; }
        public CountryCode AlternateCountryCode { get; set; }
        public short MajorId { get; set; }
        public short PackageVersion { get; set; }
        public short ClientVersion { get; set; }
        public byte AccountType { get; set; }
        public string AccountParam { get; set; } = "";
        public string HomeUrl { get; set; } = "";
        public string PatchUrl { get; set; } = "";
        public ServerRegion[] ServerRegions { get; set; } = [];
        public BinaryXmlTag? Storage { get; set; }
        public BinaryXmlTag? Extra { get; set; }

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            Type = reader.ReadByte();
            if (Type == 1)
            {
                SzId = reader.ReadInt16();
                CountryCode = (CountryCode)reader.ReadByte();
                AlternateCountryCode = (CountryCode)reader.ReadByte();
                MajorId = reader.ReadInt16();
                PackageVersion = reader.ReadInt16();
                ClientVersion = reader.ReadInt16();
                PatchUrl = reader.ReadKRString();
                ServerRegions =
                [
                    new ServerRegion()
                    {
                        Description = "Default",
                        ServerEndPoints = readIPEndPoints(reader)
                    }
                ];
            }
            else
            {
                SzId = reader.ReadInt16();
                CountryCode = (CountryCode)reader.ReadByte();
                AlternateCountryCode = (CountryCode)reader.ReadByte();
                MajorId = reader.ReadInt16();
                PackageVersion = reader.ReadInt16();
                ClientVersion = reader.ReadInt16();
                AccountType = reader.ReadByte();
                AccountParam = reader.ReadKRString();
                HomeUrl = reader.ReadKRString();
                PatchUrl = reader.ReadKRString();
                int serverCount = reader.ReadInt32();
                ServerRegions = new ServerRegion[serverCount];
                for (int i = 0; i < serverCount; i++)
                {
                    ServerRegion serverRegion = new ServerRegion();
                    byte u2 = reader.ReadByte();
                    serverRegion.Description = reader.ReadKRString();
                    if (reader.ReadByte() != 0)
                    {
                        serverRegion.AccountConfig = reader.ReadBinaryXmlTag(Encoding.Unicode);
                    }
                    serverRegion.ServerEndPoints = readIPEndPoints(reader);
                    if (reader.ReadByte() != 0)
                    {
                        serverRegion.UnknownTag = reader.ReadBinaryXmlTag(Encoding.Unicode);
                    }

                    ServerRegions[i] = serverRegion;
                }

                if (reader.ReadByte() != 0)
                {
                    Storage = reader.ReadBinaryXmlTag(Encoding.Unicode);
                }
                
                if (reader.ReadByte() != 0)
                {
                    Extra = reader.ReadBinaryXmlTag(Encoding.Unicode);
                }
            }
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            writer.Write((byte)Type);
            if (Type == 1)
            {
                writer.Write((short)SzId);
                writer.Write((byte)CountryCode);
                writer.Write((byte)AlternateCountryCode);
                writer.Write((short)MajorId);
                writer.Write((short)PackageVersion);
                writer.Write((short)ClientVersion);
                writer.WriteKRString(PatchUrl);

                writeIPEndPoints(writer, ServerRegions[0].ServerEndPoints);
            }
            else
            {
                writer.Write((short)SzId);
                writer.Write((byte)CountryCode);
                writer.Write((byte)AlternateCountryCode);
                writer.Write((short)MajorId);
                writer.Write((short)PackageVersion);
                writer.Write((short)ClientVersion);
                writer.Write((byte)AccountType);
                writer.WriteKRString(AccountParam);
                writer.WriteKRString(HomeUrl);
                writer.WriteKRString(PatchUrl);
                
                writer.Write((int)ServerRegions.Length);
                foreach(var serverRegion in ServerRegions)
                {
                    writer.Write((byte)0x01);
                    writer.WriteKRString(serverRegion.Description);
                    if (serverRegion.AccountConfig is not null)
                    {
                        writer.Write((byte)0x01);
                        writer.Write(serverRegion.AccountConfig.ToBinary(Encoding.Unicode));
                    }
                    else
                    {
                        writer.Write((byte)0x00);
                    }
                    
                    writeIPEndPoints(writer, serverRegion.ServerEndPoints);
                    
                    if (serverRegion.UnknownTag is not null)
                    {
                        writer.Write((byte)0x01);
                        writer.Write(serverRegion.UnknownTag.ToBinary(Encoding.Unicode));
                    }
                    else
                    {
                        writer.Write((byte)0x00);
                    }
                }

                if (Storage is not null)
                {
                    writer.Write((byte)0x01);
                    writer.Write(Storage.ToBinary(Encoding.Unicode));
                }
                else
                {
                    writer.Write((byte)0x00);
                }
                
                if (Extra is not null)
                {
                    writer.Write((byte)0x01);
                    writer.Write(Extra.ToBinary(Encoding.Unicode));
                }
                else
                {
                    writer.Write((byte)0x00);
                }
            }
        }

        private IPEndPoint[] readIPEndPoints(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            IPEndPoint[] output = new IPEndPoint[count];
            for (int i = 0; i < count; i++)
            {
                uint addressInteger = reader.ReadUInt32();
                int addressPort = reader.ReadUInt16();
                output[i] = new IPEndPoint(addressInteger, addressPort);
            }

            return output;
        }

        private void writeIPEndPoints(BinaryWriter writer, IPEndPoint[] points)
        {
            writer.Write((int)points.Length);
            foreach(IPEndPoint endPoint in points)
            {
                writer.Write((uint)(BitConverter.ToUInt32(endPoint.Address.GetAddressBytes())));
                writer.Write((ushort)endPoint.Port);
            }
        }
        
    }
}
