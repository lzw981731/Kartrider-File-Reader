using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using KartCity.Common.IO;
using KartLibrary.Client;
using KartLibrary.Consts;
using KartLibrary.Game.Record;
using KartLibrary.Xml;
using KartLibrary.Record;

namespace KartLibrary.IO
{
    public static class KSVBinaryWExt
    {
        public static void WriteKSVInfo(this BinaryWriter bw, KSVInfo ki)
        {
            uint headerClassIdentifier = KSVStructVersion.GetHeaderClassIdentifier(ki.RecordHeaderVersion);
            bw.Write(headerClassIdentifier);
            bw.WriteKRString(ki.RecordTitle);
            bw.Write((short)ki.CountryCode);
            bw.Write(ki.Unknown1_1);
            bw.Write((byte)ki.ContestType);
            uint PlayerNameHash = GetPlayerNameHash(ki.Players);
            bw.Write(PlayerNameHash);
            bw.Write(ki.Unknown1_2);
            bw.WriteKRString(ki.RecorderAccount);
            bw.WriteKRString(ki.RecorderName);
            bw.WriteKRDateTime(ki.RecordingDate);
            uint RecordCheckSum = GetRecordCheckSum(ki.Records);
            bw.Write(RecordCheckSum);
            bw.Write(ki.IsOffical);
            bw.WriteKRString(ki.Description);
            bw.WriteKRString(ki.TrackName);
            bw.Write(ki.Unknown3);
            bw.Write((int)ki.BestTime.TotalMilliseconds);
            bw.WriteKRString(ki.ContestImg);
            if (ki.RecordHeaderVersion >= 5)
            {
                bw.Write((int)ki.Unknown4.Length);
                if(ki.Unknown4.Length > 0)
                    bw.Write(ki.Unknown4);    
            }
            
            if(ki.RecordHeaderVersion >= 8)
                bw.Write(ki.Unknown5);
            
            if (ki.RecordHeaderVersion >= 9)
                bw.Write((byte)ki.Speed);
            
            if(ki.RecordHeaderVersion >= 12)
                bw.Write(ki.Unknown6);
            
            PlayerInfo[] players = ki.Players;
            bw.Write(players.Length);
            foreach (PlayerInfo player in players)
                bw.WritePlayerInfo(player, ki.RecordHeaderVersion);
            uint recordClassIdentifier = KSVStructVersion.GetRecordClassIdentifier(ki.RecordVersion);
            bw.Write(recordClassIdentifier);
            RecordData[] records = ki.Records;
            bw.Write(records.Length);
            foreach (RecordData rd in records)
                bw.WriteRecordData(rd, ki.RecordHeaderVersion);
        }

        public static void WritePlayerInfo(this BinaryWriter bw, PlayerInfo pi, int KSVHeaderVersion)
        {
            bw.WriteKRString(pi.PlayerName);
            bw.WriteKRString(pi.ClubName);
            bw.WritePlayerEquipment(pi.Equipment, KSVHeaderVersion);
        }

        public static void WritePlayerEquipment(this BinaryWriter bw, PlayerEquipment pe, int KSVHeaderVersion)
        {
            bw.Write(pe.Character);
            if (KSVHeaderVersion >= 10)
                bw.Write(pe.KartPaint);
            bw.Write(pe.CharacterColor);
            bw.Write(pe.Kart);
            bw.Write(pe.Plate);
            bw.Write(pe.Goggle);
            bw.Write(pe.Balloon);
            bw.Write(pe.Equ2);
            bw.Write(pe.Headband);
            bw.Write(pe.Replay);
            bw.Write(pe.Cane);
            bw.Write(pe.Equ3);
            bw.Write(pe.Apparel);
            bw.Write(pe.Equ4);
            bw.WriteKRString(pe.PlateText);
            bw.Write(pe.Equ5);
            bw.Write(pe.Equ6);
            bw.Write(pe.Equ7);
            bw.Write(pe.Equ8);
            bw.Write(pe.Equ9);
            bw.Write(pe.Equ10);
            bw.Write(pe.Equ11);
            if (KSVHeaderVersion >= 11)
            {
                bw.Write(pe.Equ12);
                bw.Write(pe.Equ13);
            }
        }

        public static void WriteRecordData(this BinaryWriter bw, RecordData rd, int KSVHeaderVersion)
        {
            RecordStamp[] rss = rd.Stamps;
            bw.Write(rss.Length);
            foreach (RecordStamp r in rss)
            {
                bw.WriteRecordStramp(r, KSVHeaderVersion);
            }
        }
        static Random rd = new Random();
        public static void WriteRecordStramp(this BinaryWriter bw, RecordStamp data, int KSVHeaderVersion)
        {
            bw.Write((short)(data.Time / 100));
            bw.Write((short)MathF.Round(data.X * 10));
            bw.Write((short)MathF.Round(data.Y * 10));
            bw.Write((short)MathF.Round(data.Z * 10));

            bw.Write((short)MathF.Round((data.Angle.W) * 100));
            bw.Write((short)MathF.Round(data.Angle.X * 100));
            bw.Write((short)MathF.Round(data.Angle.Y * 100));
            bw.Write((short)MathF.Round((data.Angle.Z) * 100));
            /*
            bw.Write((short)((data.angle_W) * 100));
            bw.Write((short)(data.angle_X * 100));
            bw.Write((short)(data.angle_Y * 100));
            bw.Write((short)((data.angle_Z) * 100));
            */
            bw.Write(data.Status);
        }

        public static void WriteKartSpecByte(this BinaryWriter bw, byte value) =>
            bw.Write((byte)KartSpecEncode.EncodeByte(value));
        
        public static void WriteKartSpecSingle(this BinaryWriter bw, float value) =>
            bw.Write((int)KartSpecEncode.EncodeSingle(value));
        
        public static void WriteKartSpecInt32(this BinaryWriter bw, int value) =>
            bw.Write((int)KartSpecEncode.EncodeInt32(value));
        
        public static void WriteKartSpecInt16(this BinaryWriter bw, short value) =>
            bw.Write((short)KartSpecEncode.EncodeInt16(value));

        
        
        private static uint GetPlayerNameHash(PlayerInfo[] players)
        {
            uint output = 0;
            foreach (PlayerInfo player in players)
            {
                byte[] dataStr = Encoding.GetEncoding("UTF-16").GetBytes(player.PlayerName);
                uint strHash = Adler.Adler32(0, dataStr, 0, dataStr.Length);
                output += strHash;
            }
            return output;
        }
        private static uint GetRecordCheckSum(RecordData[] data)
        {
            uint oddSum = 0, evenSum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                RecordStamp[] curStamps = data[i].Stamps;
                for (int j = 0; j < curStamps.Length; j++)
                {
                    if ((j & 1) == 1)
                    {
                        oddSum += (uint)curStamps[j].Status;
                    }
                    else
                    {
                        evenSum += (uint)curStamps[j].Status;
                    }
                }
            }
            return (oddSum << 16) + evenSum;
        }
    }

    public static class KartObjectWriterExt
    {
        public static void WriteKartObject(this BinaryWriter writer, KartObject kartObject,
            KartObjectBuffer? buffer)
        {
            bool isNew = true;
            if (buffer is not null)
            {
                int index = buffer.AddKartObjectForWrite(kartObject, out isNew);
                if (isNew)
                {
                    writer.Write((short) 0x47aa);
                    writer.Write(kartObject.ClassStamp);
                }
                else
                {
                    writer.Write((short) 0x47bb);
                }
                if(buffer.UseInt32Index)
                    writer.Write((int)index);
                else
                    writer.Write((short)index);
            }
            else
            {
                writer.Write(kartObject.ClassStamp);
            }
            if(isNew)
                kartObject.EncodeObject(writer, buffer);
        }
        
        public static void WriteKartObject(this BinaryWriter writer, object field,
            Dictionary<KartObject, short>? objectCache, Dictionary<object, short>? fieldCache)
        {
            
        }
    }
    
    public delegate void EncodeFieldFunc(BinaryWriter writer, object field, Dictionary<KartObject, short>? objectCache, Dictionary<object, short>? fieldCache);
}
