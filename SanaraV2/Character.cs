/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.
using System;
using System.IO;

namespace SanaraV2
{
    public class Character
    {
        public Character()
        {
            _nbMessage = 0;
        }

        public Character(ulong name, string nameStr)
        {
            _nbMessage = 0;
            _name = name;
            _nameStr = nameStr;
            File.WriteAllText("Saves/Users/" + _name + ".dat", ReturnInformationsRaw(false));
        }

        private string ReturnInformationsRaw(bool increaseMsg)
        {
            return (_nameStr + Environment.NewLine + _name + Environment.NewLine + _firstMeet + Environment.NewLine + (Convert.ToInt32(GetNbMessage()) + 1).ToString());
        }

        public void SaveAndParseInfos(string[] infos)
        {
            try
            {
                _nameStr = infos[0];
                _name = Convert.ToUInt64(infos[1]);
                _firstMeet = infos[2];
            }
            catch (IndexOutOfRangeException)
            { }
        }

        public void Meet()
        {
            if (_firstMeet == "No")
            {
                _firstMeet = DateTime.UtcNow.ToString("ddMMyyHHmmss");

                File.WriteAllText("Saves/Users/" + _name + ".dat", ReturnInformationsRaw(false));
            }
        }

        public void IncreaseNbMessage()
        {
            _nbMessage++;
            File.WriteAllText("Saves/Users/" + _name + ".dat", ReturnInformationsRaw(true));
        }

        public string GetFirstMeet()
        {
            return (File.ReadAllLines("Saves/Users/" + _name + ".dat")[2]);
        }

        public int GetNbMessage()
        {
            return (Convert.ToInt32(File.ReadAllLines("Saves/Users/" + _name + ".dat")[3]));
        }

        private string _nameStr;
        public ulong _name { private set; get; }
        private int _nbMessage;
        private string _firstMeet;
    }
}