using System.Collections.Generic;
using System.IO;

namespace SanaraV2.Community
{
    public class CommunityManager
    {
        public CommunityManager()
        {
            if (!Directory.Exists("Saves/Assets"))
                Directory.CreateDirectory("Saves/Assets");
            if (Directory.Exists("../../Assets"))
                foreach (var file in Directory.GetFiles("../../Assets"))
                {
                    var fi = new FileInfo(file);
                    File.Copy(file, "Saves/Assets/" + fi.Name, true);
                }
            _profiles = new Dictionary<ulong, Profile>();
        }


        public void AddProfile(ulong id, Profile p) => _profiles.Add(id, p);
        public Profile GetProfile(ulong id)
        {
            if (_profiles.ContainsKey(id)) return _profiles[id];
            return null;
        }
        private Dictionary<ulong, Profile> _profiles;
    }
}
