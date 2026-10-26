using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Puzzle.Gravity
{
    [Serializable] public sealed class ProgressData
    {
        public string dataVersion, levelId;
        public int[] blockIds, sizes;
        public PuzzleSnapshot current;
        public PuzzleSnapshot[] history;
        public string[] completed = new string[0];
        public string[] seenLessons = new string[0];
        public bool reduceMotion, largeText;
        public bool usedUndo;
        public bool sound = true;
        public PuzzleCollection collection;
    }
    [Serializable] public sealed class SaveEnvelope { public int version=1, sequence; public string payload, checksum; }
    public sealed class ProgressSaveService
    {
        // Storage IDs are intentionally stable across display-name changes.
        public static string DefaultDirectory
        {
            get
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                return StorageDirectory(Application.persistentDataPath,true);
#else
                return StorageDirectory(Application.persistentDataPath,false);
#endif
            }
        }
        public static string StorageDirectory(string persistentRoot,bool windows)
        {
            if(windows)persistentRoot=Path.Combine(Directory.GetParent(persistentRoot).FullName,"Android Puzzle");
            return Path.Combine(persistentRoot,"MellowGravity");
        }
        // Keep the pre-resize portal definition checksum byte-for-byte compatible.
        [Serializable] private sealed class LegacyCargo { public int x,y,size,gx,gy; }
        [Serializable] private sealed class LegacyLevel
        {
            public string room,displayName,lesson;public PortalDefinition[] portals;
            public int family,width,height;public Cell[] walls;public LegacyCargo[] cargo;public int[] route;
        }
        [Serializable] private sealed class LegacyResizeLevel
        {
            public string room,displayName,lesson;public PortalDefinition[] portals;public ResizeDefinition[] resizers;
            public int family,width,height;public Cell[] walls;public CargoDefinition[] cargo;public int[] route;
        }
        public static string LevelVersion(PuzzleDefinition level,string originalVersion)
        {
            if(level.HasBlinkBlocks)return Checksum("blink-v1|"+JsonUtility.ToJson(level));
            if(level.HasResizers)
            {
                var old=new LegacyResizeLevel{room=level.room,displayName=level.displayName,lesson=level.lesson,portals=level.portals,resizers=level.resizers,family=level.family,width=level.width,height=level.height,walls=level.walls,cargo=level.cargo,route=level.route};
                return Checksum("resize-v1|"+JsonUtility.ToJson(old));
            }
            if(!level.HasPortals)return originalVersion;
            var legacy=new LegacyLevel{room=level.room,displayName=level.displayName,lesson=level.lesson,portals=level.portals,family=level.family,width=level.width,height=level.height,walls=level.walls,route=level.route,
                cargo=level.cargo.Select(c=>new LegacyCargo{x=c.x,y=c.y,size=c.size,gx=c.gx,gy=c.gy}).ToArray()};
            return Checksum("face-portal-v2|"+JsonUtility.ToJson(legacy));
        }
        private readonly string directory;
        private int sequence;
        public string LastError { get; private set; }
        public ProgressSaveService(string directory) { this.directory=directory; }
        public static string Checksum(string value)
        {
            uint hash=2166136261;
            foreach(char c in value) { hash ^= c; hash *= 16777619; }
            return hash.ToString("x8");
        }
        public ProgressData Load()
        {
            ProgressData latest=null; sequence=0;
            for(int slot=0;slot<2;slot++)
            {
                try
                {
                    string path=Path.Combine(directory,"gravity-progress-"+slot+".json");
                    if (!File.Exists(path)) continue;
                    var envelope=JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(path));
                    if (envelope == null || envelope.version != 1 || envelope.sequence <= sequence || envelope.payload == null || Checksum(envelope.payload) != envelope.checksum) continue;
                    var data=JsonUtility.FromJson<ProgressData>(envelope.payload);
                    if (data == null) continue;
                    latest=data; sequence=envelope.sequence;
                }
                catch(Exception e) { LastError=e.Message; }
            }
            return latest;
        }
        public bool Save(ProgressData data)
        {
            try
            {
                Directory.CreateDirectory(directory);
                string payload=JsonUtility.ToJson(data);
                int next=sequence+1;
                var envelope=new SaveEnvelope { sequence=next, payload=payload, checksum=Checksum(payload) };
                string path=Path.Combine(directory,"gravity-progress-"+(next%2)+".json");
                string temporary=path+".tmp";
                File.WriteAllText(temporary,JsonUtility.ToJson(envelope));
                // The other, previously confirmed slot remains untouched throughout replacement.
                if(File.Exists(path)) File.Delete(path);
                File.Move(temporary,path);
                sequence=next; LastError=null;
                return true;
            }
            catch(Exception e) { LastError=e.Message; return false; }
        }
        public static bool Restore(ProgressData data, string version, PuzzleSession session)
        {
            if(data == null || data.dataVersion != version || data.levelId != session.Level.room ||
                data.blockIds == null || data.sizes == null ||
                !data.blockIds.SequenceEqual(Enumerable.Range(0,session.Level.cargo.Length)) ||
                !data.sizes.SequenceEqual(session.Level.cargo.Select(c=>c.size))) return false;
            return session.Restore(data.current,data.history,data.usedUndo);
        }
    }
}
