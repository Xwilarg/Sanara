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
        public Character(ulong id, string name)
        {
            this.id = id;
            if (!Directory.Exists("Saves/Users"))
                Directory.CreateDirectory("Saves/Users");
            if (!File.Exists("Saves/Users/" + id + ".dat"))
            {
                File.WriteAllText("Saves/Users/" + id + ".dat",
                    name + Environment.NewLine + id + Environment.NewLine +
                    "No" + Environment.NewLine + "0");
            }
        }

        private void Meet()
        {
            string[] content = File.ReadAllLines("Saves/Users/" + id + ".dat");
            if (content[2] == "No")
            {
                File.WriteAllText("Saves/Users/" + id + ".dat",
                    content[0] + Environment.NewLine + id + Environment.NewLine +
                    DateTime.UtcNow.ToString("ddMMyyHHmmss") + Environment.NewLine + content[3]);
            }
        }

        public void IncreaseNbMessage()
        {
            Meet();
            string[] content = File.ReadAllLines("Saves/Users/" + id + ".dat");
            File.WriteAllText("Saves/Users/" + id + ".dat",
                content[0] + Environment.NewLine + id + Environment.NewLine +
                content[2] + Environment.NewLine + (Convert.ToInt32(content[3]) + 1));
        }

        private ulong id;
    }
}