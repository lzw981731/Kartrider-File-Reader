using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using KartLibrary.Xml;
using KartLibrary.Record;
using KartLibrary.Consts;
using System.Numerics;
using KartCity.Common.Client;
using KartCity.Common.Consts;
using KartCity.Common.IO;
using KartCity.Common.Xml;
using KartLibrary.Game.Engine;
using KartLibrary.Game.Record;
using Vortice.Win32;

namespace KartLibrary.IO
{
    public static class KSVBinaryRExt
    {
        public static byte ReadKartSpecByte(this BinaryReader br) => KartSpecEncode.DecodeByte(br.ReadByte());
        
        public static float ReadKartSpecSingle(this BinaryReader br) => KartSpecEncode.DecodeSingle(br.ReadInt32());
        
        public static int ReadKartSpecInt32(this BinaryReader br) => KartSpecEncode.DecodeInt32(br.ReadInt32());
        
        public static short ReadKartSpecInt16(this BinaryReader br) => KartSpecEncode.DecodeInt16(br.ReadInt16());

        public static KSVInfo ReadKSVInfo(this BinaryReader br)
        {
            KSVInfo ki = new KSVInfo();
            uint headerClassIdentifier = br.ReadUInt32();
            ki.RecordHeaderVersion = KSVStructVersion.GetHeaderVersion(headerClassIdentifier);
            ki.RecordTitle = br.ReadKRString();
            ki.CountryCode = (CountryCode)br.ReadInt16();
            ki.Unknown1_1 = br.ReadByte();
            ki.ContestType = (ContestType)br.ReadByte();
            ki.PlayerNameHash = br.ReadUInt32();
            ki.Unknown1_2 = br.ReadUInt32();
            ki.RecorderAccount = br.ReadKRString();
            ki.RecorderName = br.ReadKRString();
            ki.RecordingDate = br.ReadKRDateTime();
            ki.RecordChecksum = br.ReadUInt32();
            ki.IsOffical = br.ReadByte() == 1;
            ki.Description = br.ReadKRString();
            ki.TrackName = br.ReadKRString();
            ki.Unknown3 = br.ReadInt32();
            ki.BestTime = new TimeSpan(0, 0, 0, 0, br.ReadInt32());
            ki.ContestImg = br.ReadKRString();
            if (ki.RecordHeaderVersion >= 5)
            {
                int len = br.ReadInt32();
                ki.Unknown4 = br.ReadBytes(len);
            }

            if (ki.RecordHeaderVersion >= 8)
            {
                ki.Unknown5 = br.ReadByte(); 
            }

            if (ki.RecordHeaderVersion >= 9)
                ki.Speed = (SpeedType)(br.ReadByte());

            if (ki.RecordHeaderVersion >= 12)
            {
                ki.Unknown6 = br.ReadByte();
            }
            int playerCount = br.ReadInt32();
            PlayerInfo[] players = new PlayerInfo[playerCount];
            for (int i = 0; i < playerCount; i++)
                players[i] = br.ReadPlayerInfo(ki.RecordHeaderVersion);
            ki.Players = players;
            
            uint recordClassIdentifier = br.ReadUInt32();
            ki.RecordVersion = KSVStructVersion.GetVersion(recordClassIdentifier);
            int recordCount = br.ReadInt32();
            RecordData[] records = new RecordData[recordCount];
            for (int i = 0; i < recordCount; i++)
                records[i] = br.ReadRecordData(ki.RecordHeaderVersion);
            ki.Records = records;
            return ki;
        }

        public static PlayerInfo ReadPlayerInfo(this BinaryReader br, int KSVHeaderVersion)
        {
            PlayerInfo pi = new PlayerInfo();
            pi.PlayerName = br.ReadKRString();
            pi.ClubName = br.ReadKRString();
            pi.Equipment = br.ReadPlayerEquipment(KSVHeaderVersion);
            return pi;
        }

        public static PlayerEquipment ReadPlayerEquipment(this BinaryReader br, int KSVHeaderVersion)
        {
            PlayerEquipment pe = new PlayerEquipment();
            pe.Character = br.ReadInt16();
            if (KSVHeaderVersion >= 10)
                pe.KartPaint = br.ReadInt16();
            pe.CharacterColor = br.ReadInt16();
            pe.Kart = br.ReadInt16();
            pe.Plate = br.ReadInt16();
            pe.Goggle = br.ReadInt16();
            pe.Balloon = br.ReadInt16();
            pe.Equ2 = br.ReadInt16();
            pe.Headband = br.ReadInt16();
            pe.Replay = br.ReadInt16();
            pe.Cane = br.ReadInt16();
            pe.Equ3 = br.ReadInt16();
            pe.Apparel = br.ReadInt16();
            pe.Equ4 = br.ReadInt16();
            pe.PlateText = br.ReadKRString();
            pe.Equ5 = br.ReadInt16();
            pe.Equ6 = br.ReadInt16();
            pe.Equ7 = br.ReadInt16();
            pe.Equ8 = br.ReadInt16();
            pe.Equ9 = br.ReadInt16();
            pe.Equ10 = br.ReadInt16();
            pe.Equ11 = br.ReadInt16();
            if (KSVHeaderVersion >= 11)
            {
                pe.Equ12 = br.ReadInt16();
                pe.Equ13 = br.ReadInt16();
            }
            return pe;
        }

        public static RecordData ReadRecordData(this BinaryReader br, int KSVHeaderVersion)
        {
            RecordData rd = new RecordData();
            int totalCount = br.ReadInt32();
            RecordStamp[] rss = new RecordStamp[totalCount];
            for (int i = 0; i < totalCount; i++)
            {
                rss[i] = br.ReadRecordStramp(KSVHeaderVersion);
            }
            rd.Stamps = rss;
            return rd;
        }

        public static RecordStamp ReadRecordStramp(this BinaryReader br, int KSVHeaderVersion)
        {
            RecordStamp rs = new RecordStamp();
            rs.Time = br.ReadInt16() * 100;
            rs.X = br.ReadInt16() * 0.1f;
            rs.Y = br.ReadInt16() * 0.1f;
            rs.Z = br.ReadInt16() / 50.0f + 590;
            float angle_W = br.ReadInt16() / 10000f;
            float angle_X = br.ReadInt16() / 10000f;
            float angle_Y = br.ReadInt16() / 10000f;
            float angle_Z = br.ReadInt16() / 10000f;
            rs.Angle = new System.Numerics.Quaternion(angle_X, angle_Y, angle_Z, angle_W);
            rs.Status = br.ReadUInt16();
            return rs;
        }
    }

    public static class KartObjectReaderExt
    {
        public static KartObject ReadKartObject(this BinaryReader br, KartObjectBuffer? buffer)
        {
            uint classStamp;
            KartObject output;
            if (buffer is not null)
            {
                int isnull = br.ReadUInt16();
                if(isnull == 0x47BB)
                {
                    short objIndex = br.ReadInt16();
                    output = buffer.GetKartObject(objIndex) ?? throw new IndexOutOfRangeException();
                }
                else
                {
                    classStamp = br.ReadUInt32();
                    short objIndex = br.ReadInt16();
                    output = KartObjectManager.CreateObject(classStamp);
                    output?.DecodeObject(br, buffer);
                    if (output is null || !buffer.AddKartObjectForRead(objIndex, output))
                        throw new Exception();
                }
            }
            else
            {
                classStamp = br.ReadUInt32();
                output = KartObjectManager.CreateObject(classStamp);
                output?.DecodeObject(br, buffer);
                if (output is null)
                    throw new Exception();
            }
            return output;
        }

        public static TBase ReadKartObject<TBase>(this BinaryReader br, KartObjectBuffer? buffer) where TBase : KartObject
        {
            uint classStamp;
            TBase output;
            if (buffer is not null)
            {
                int isnull = br.ReadUInt16();
                if (isnull == 0x47BB)
                {
                    short objIndex = br.ReadInt16();
                    KartObject kartObject = buffer.GetKartObject(objIndex) ?? throw new IndexOutOfRangeException();
                    if(kartObject is TBase decTBase)
                        output = decTBase;
                    else
                        throw new InvalidCastException();
                }
                else
                {
                    classStamp = br.ReadUInt32();
                    short objIndex = br.ReadInt16();
                    output = KartObjectManager.CreateObject<TBase>(classStamp);
                    output?.DecodeObject(br, buffer);
                    if (output is null || !buffer.AddKartObjectForRead(objIndex, output))
                        throw new Exception();
                }
            }
            else
            {
                classStamp = br.ReadUInt32();
                output = KartObjectManager.CreateObject<TBase>(classStamp);
                output?.DecodeObject(br, buffer);
                if (output is null)
                    throw new Exception();
            }
            return output;
        }

        // AA27 BB27
        public static T ReadField<T>(this BinaryReader br, KartObjectBuffer? buffer, DecodeFieldFunc<T> decodeFieldFunc)
        {
            if(buffer is not null)
            {
                ushort token = br.ReadUInt16();
                if (token == 0x27AA)
                {
                    short fieldObjIndex = br.ReadInt16();
                    T decodedField = decodeFieldFunc(br, buffer);
                    if (decodedField is null)
                        throw new Exception("Decoded object can't be null.");
                    buffer.AddObjectForRead(fieldObjIndex, decodedField);
                    return decodedField;
                }
                else if (token == 0x27BB)
                {
                    short fieldObjIndex = br.ReadInt16();
                    object? fieldObj = buffer.GetObject(fieldObjIndex);
                    if (fieldObj is not null)
                    {
                        if (fieldObj is T outField)
                            return outField;
                        else
                            throw new InvalidCastException();
                    }
                    else
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            else
            {
                return decodeFieldFunc(br, buffer);
            }
        }

        public static byte[] ReadCacheableBytes(this BinaryReader br, KartObjectBuffer? buffer)
        {
            return br.ReadField(buffer, (x, _) => x.ReadBytes(x.ReadInt32()));
        }
    }

    public delegate T DecodeFieldFunc<T>(BinaryReader reader, KartObjectBuffer? buffer);
}
