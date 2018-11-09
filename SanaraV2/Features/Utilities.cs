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
using System.Text;

namespace SanaraV2.Features
{
    public static class Utilities
    {
        /// <summary>
        /// Every commands take a string[] in parameter so they can be called with any number of arguments.
        /// This function transform it to a string adding spaces between each elements of the array
        /// </summary>
        /// <param name="args">The string[] to deal with</param>
        public static string AddArgs(string[] args)
        {
            if (args.Length == 0)
                return (null);
            return (string.Join(" ", args));
        }

        /// <summary>
        /// Check if file extension is the one of an image
        /// </summary>
        public static bool IsImage(string extension)
        {
            return (extension == "gif" || extension == "png" || extension == "jpg"
                || extension == "jpeg");
        }

        /// <summary>
        /// Generate a random code containing numbers
        /// </summary>
        /// <param name="nbDigits">The number of digits in the code</param>
        public static string GenerateRandomCode(int nbDigits, Random r)
        {
            StringBuilder code = new StringBuilder();
            for (int i = 0; i < nbDigits; i++)
                code.Append(r.Next(10));
            return (code.ToString());
        }
    }
}
