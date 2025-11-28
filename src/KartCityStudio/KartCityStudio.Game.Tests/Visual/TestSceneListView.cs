using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using KartCity.Common.Client;
using KartCity.Common.Consts;
using KartCity.Common.IO.SmartStream;
using KartCityStudio.Game.Graphics.UserInterface;
using KartLibrary.Consts;
using KartLibrary.File;
using KartLibrary.Game.Record;
using KartLibrary.IO;
using KartLibrary.Record;
using NUnit.Framework;
using NUnit.Framework.Internal;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using Logger = osu.Framework.Logging.Logger;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneListView : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        private KCSListView listview;

        private ListViewHeaderItem nameHeader;
        private ListViewHeaderItem idHeader;
        private ListViewHeaderItem scoreHeader;

        [Resolved] private KartStorageSystem storageSystem { get; set; }

        public TestSceneListView()
        {
            Add(listview = new KCSListView()
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColour = Colour4.Black,
            });
            addBasicSteps();
            addKsvTestSteps();
        }

        private void addBasicSteps()
        {
            AddLabel("Basic Steps");
            AddStep("Initialize listview", () =>
            {
                listview.Items.Clear();
                listview.Headers.Clear();
                listview.Headers.Add(nameHeader = new ListViewHeaderItem("Name", "Name", 0.7f));
                listview.Headers.Add(idHeader = new ListViewHeaderItem("ID", "ID", 0.3f));
            });
            AddStep("Add 100 items to ListView.", () =>
            {
                listview.Items.AddRange(Enumerable.Range(0, 100).Select(x => new ListViewItem(new(string, LocalisableString)[]
                {
                    ("Name", $"Item{listview.Items.Count + x}"),
                    ("ID", $"{listview.Items.Count}"),
                    ("Score", $"{(((x + 0x12345678) * 0xFC905B) % 100)}"),
                })));
            });
            AddStep("Adjust field size", () =>
            {
                nameHeader.FieldWidth.Value = 0.5f;
                idHeader.FieldWidth.Value = 0.5f;
            });
            AddStep("Add a new field: Score.", () =>
            {
                listview.Headers.Add(scoreHeader = new ListViewHeaderItem("Score", "Score", 0.3f));
                nameHeader.FieldWidth.Value = 0.4f;
                idHeader.FieldWidth.Value = 0.3f;
            });
            AddStep("Change header to chinese name.", () =>
            {
                nameHeader.Text.Value = "名字";
                idHeader.Text.Value = "編號";
                scoreHeader.Text.Value = "分數";
            });
            AddStep("Remove a field: ID", () =>
            {
                listview.Headers.Remove(idHeader);
            });
            AddStep("Modify one of texts of first ListViewItem.", () =>
            {
                ListViewItem item = listview.Items[0];
                item.Texts["Name"] = $"Modified!{DateTime.Now.Microsecond}";
            });
            AddStep("Remove one of texts of first ListViewItem.", () =>
            {
                ListViewItem item = listview.Items[0];
                if(item.Texts.ContainsKey("Name"))
                    item.Texts.Remove("Name");
            });
        }

        private void addKsvTestSteps()
        {
            AddLabel("KSV Testing");
            AddStep("Initialize listview", () =>
            {
                listview.Items.Clear();
                listview.Headers.Clear();
                listview.Headers.Add(nameHeader = new ListViewHeaderItem("Time", "Time", 0.05f));
                listview.Headers.Add(idHeader = new ListViewHeaderItem("Position", "Position", 0.1f));
                listview.Headers.Add(idHeader = new ListViewHeaderItem("Quaternion", "Quaternion", 0.1f));
                listview.Headers.Add(idHeader = new ListViewHeaderItem("Slot", "Slot", 0.25f));
                listview.Headers.Add(idHeader = new ListViewHeaderItem("Status", "Status", 0.3f));
                listview.Headers.Add(idHeader = new ListViewHeaderItem("Unknown", "Unknown", 0.2f));
            });
            AddStep("Read KSV Info", () =>
            {
                KartStorageFile? ksvFile = storageSystem.GetFile("zeta/kr/ksv/35th_2set_04.ksv"); //
                if (ksvFile is not null)
                {
                    using (Stream stream = ksvFile.CreateStream())
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        int ksvLength = reader.ReadInt32();
                        byte[] data = reader.ReadSmartStreamToBytes(ksvLength);
                        using (MemoryStream ksvDataStream = new MemoryStream(data))
                        {
                            BinaryReader ksvDataReader = new BinaryReader(ksvDataStream);
                            KSVInfo ksvInfo = ksvDataReader.ReadKSVInfo();
                            Logger.Log($"Title: {ksvInfo.RecordTitle}");
                            Logger.Log($"Title: {ksvInfo.TrackName}");
                            foreach (PlayerInfo playerInfo in ksvInfo.Players)
                            {
                                Logger.Log($"Player: {playerInfo.PlayerName}");
                                Logger.Log($"Player: {playerInfo.Equipment.Equ5:b16}");
                            }
                            RecordStamp[] stamps = ksvInfo.Records[0].Stamps;
                            string[] itemName = new[]
                            {
                                "烏雲",
                                "墨烏",
                                "魔王",
                                "飛碟",
                                "水蠅",
                                "磁鐵",
                                "紅氣",
                                "飛彈",
                                "香蕉",
                                "水彈",
                                "盾牌",
                                "天使",
                                "解碟",
                                "自爆",
                                "藍氣",
                                "　　"
                            };

                            ksvInfo.CountryCode = CountryCode.TW;
                            ksvInfo.IsOffical = false;
                            ksvInfo.RecorderName = "KartRider";
                            ksvInfo.RecordTitle = $"hyz {DateTime.Now:yyMMddHHmmss}";
                            // ksvInfo.ContestType = ContestType.ItemIndividual;
                            for (int i = 0; i < ksvInfo.Players.Length; i++)
                            {
                                ksvInfo.Players[i] = ksvInfo.Players[i] with
                                {
                                    PlayerName = $"Player {i + 1}",
                                    ClubName = $"KartRider"
                                };
                            }
                            for (int i = 0; i < ksvInfo.Records.Length; i++)
                            {
                                RecordData recordData = ksvInfo.Records[i];
                                for (int j = 0; j < recordData.Stamps.Length; j++)
                                {
                                    RecordStamp recordStamp = recordData.Stamps[j];
                                    if (recordStamp.Time >= 7000)
                                    {
                                        recordStamp.X = 1406.181f;
                                        recordStamp.Y = 662.992f;
                                        recordStamp.Z = -1950.926f;
                                    }
                                    // double time = recordStamp.Time >= 7000
                                    //     ? ((recordStamp.Time - 7000) * 3f + 7000)
                                    //     : recordStamp.Time;
                                    int slot = 0;
                                    int item = (j & 0b1110);
                                    if ((recordStamp.Status & 0b0_0000_111_0000_0000) != 0)
                                    {
                                        if ((recordStamp.Status & 0b0_1111_000_0000_0000) != 0b01111_000_0000_0000)
                                        {
                                            recordStamp.Status =
                                                (ushort)((recordStamp.Status & 0b1_0000_111_1111_1111) | (item << 11));
                                        }
                                    }

                                    recordData.Stamps[j] = recordStamp;
                                    // recordData.Stamps[j] = recordStamp with
                                    // {
                                    //     Time = recordStamp.Time,
                                    //     Status = (ushort)((slot << 8) | (item << 11) | 0b00_000_000)
                                    // };
                                }

                                ksvInfo.Records[i] = recordData;
                            }
                            string[] currentSlotInfo = new[] { itemName[^1], itemName[^1], itemName[^1] , itemName[^1] };
                            IEnumerable<ListViewItem> listViewItems = stamps.Select(x =>
                            {
                                string time = $"{x.Time:0.000} ms";
                                string position = $"{x.X:0.000} {x.Y:0.000} {x.Z:0.000}";
                                string quadernion = $"{x.Angle}";
                                string status = $"{string.Join(',', x.GetCarStatus())}";
                                string unknown = $"{(string.Join("", (x.Status >> 8).ToString("b8").Replace('0', '０').Replace('1', '１')))}";

                                // Unknown:
                                //     0xxx_x0yy
                                //     xxx: 0000: 烏雲
                                //          0001: 墨色烏雲
                                //          0010: 大魔王
                                //          0011: 宇宙船
                                //          0100: 水蒼蠅
                                //          0101: 磁鐵
                                //          0110: 紅氣
                                //          0111: 飛彈
                                //          1000: 香蕉
                                //          1001: 水彈
                                //          1010: 盾牌
                                //          1011: 天使
                                //          1100: 電磁波
                                //          1101: 定時水炸彈
                                //          1110: 藍氣
                                //          1111: NO ITEM

                                //
                                //     0111_0001:       Blue booster
                                //     0011_0001:        Red booster
                                //     0111_1001: [Use]  Red booster
                                //     1111_1001: [Use] Blue booster

                                if ((x.Status & 0b0000_0111_0000_0000) != 0)
                                {
                                    currentSlotInfo[((x.Status >> 8) & 0b111) - 1] = itemName[(x.Status >> 11) & 0b1111];
                                }

                                string slotStr = "|" + string.Join("|", currentSlotInfo) + "|";

                                return new ListViewItem(new KeyValuePair<string, LocalisableString>[]
                                {
                                    new KeyValuePair<string, LocalisableString>("Time", time),
                                    new KeyValuePair<string, LocalisableString>("Position", position),
                                    new KeyValuePair<string, LocalisableString>("Quaternion", quadernion),
                                    new KeyValuePair<string, LocalisableString>("Status", status),
                                    new KeyValuePair<string, LocalisableString>("Slot", slotStr),
                                    new KeyValuePair<string, LocalisableString>("Unknown", unknown),
                                });
                            });
                            listview.Items.AddRange(listViewItems);


                            using (FileStream outFileStream = new FileStream("36th_1set_13_modified_unpacked.ksv", FileMode.Create))
                            {
                                BinaryWriter bw = new BinaryWriter(outFileStream);
                                bw.WriteKSVInfo(ksvInfo);
                            }

                            using (FileStream outFileStream = new FileStream($"{DateTime.Now:yyMMddHHmmss}_modified.ksv", FileMode.Create))
                            {
                                using MemoryStream tmpKsvStream = new MemoryStream();
                                BinaryWriter ksvWriter = new BinaryWriter(tmpKsvStream);
                                BinaryWriter fileWriter = new BinaryWriter(outFileStream);
                                ksvWriter.WriteKSVInfo(ksvInfo);
                                fileWriter.Write(0);
                                byte[] ksvData = tmpKsvStream.ToArray();
                                Logger.Log(ksvData.Length.ToString());
                                fileWriter.WriteAsSmartStreamData(ksvData, SmartStreamMode.CompressedEncrypted, true, key: 0x36699336);
                                fileWriter.Flush();
                            }

                            using (FileStream outFileStream = new FileStream("36th_1set_13.ksv", FileMode.Create))
                            {
                                BinaryWriter bw = new BinaryWriter(outFileStream);
                                bw.Write(data);
                            }
                        }
                    }
                }
            });
        }
    }
}
