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
using Discord;
using System;
using System.Collections.Generic;

namespace SanaraV2.Features.Tools
{
    public static class Response
    {
        public class Shell
        {
            public List<Tuple<string, string>> explanations; // Each element is a tuple Command name / Command description
            public string title;
            public string url;
        }

        public class Image
        {
            public Color discordColor;
            public string colorUrl;
            public string colorHex;
            public string name;
        }

        public class Translation
        {
            public string sourceLanguage;
            public string sentence;
        }

        public class JapaneseTranslation
        {
            public JapaneseWord[] words;
            public string[] definition;
            public string[] speechPart;
        }

        public class JapaneseWord
        {
            public string word;
            public string reading;
            public string romaji;
        }

        public class Urban
        {
            public string definition;
            public string example;
            public string word;
            public string link;
        }
    }
}
