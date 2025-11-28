using KartLibrary.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartCity.Common.IO;
using KartLibrary.Engine.Relements;

namespace KartLibrary.Game.Engine.Track
{
    [KartObjectImplement]
    public class TrackContainer : KartObject
    {
        public override  string ClassName => "TrackContainer";

        public string u1;

        public Relement TrackScene;

        public TrackObject[] TrackObjects;

        public override void DecodeObject(BinaryReader reader, KartObjectBuffer? buffer)
        {
            base.DecodeObject(reader, buffer);
            u1 = reader.ReadKRString();
            TrackScene = reader.ReadKartObject<Relement>(buffer);
            int eventCount = reader.ReadInt32();
            // TrackObjects
            //      ToDummy
            //      ToBlackPlane
            //      ToRoad
            //      ToMinimap
            //      ToItemCube
            //      ToLucci
            //      ToMovableObject
            //      ToEventMesh
            
            if(u1.Length > 0)
                Debug.Print($"trackContainer: {u1}");
            
            List<TrackObject> trackObjects = new List<TrackObject>();
            for(int i = 0; i < eventCount; i++)
                trackObjects.Add(reader.ReadKartObject<TrackObject>(buffer));
            TrackObjects = trackObjects.ToArray();
        }

        public override void EncodeObject(BinaryWriter writer, KartObjectBuffer? buffer)
        {
            base.EncodeObject(writer, buffer);
        }
    }
}
