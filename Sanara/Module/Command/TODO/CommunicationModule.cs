/*


        [Command("Poll", RunMode = RunMode.Async)]
        public async Task PollAsync(string title, params string[] choices)
        {
            if (choices.Length < 2)
                throw new CommandFailed("You must provide at least 1 choice.");
            if (choices.Length > 9)
                throw new CommandFailed("You can't provide more than 9 choices.");

            // All emotes to be added as reactions
            var emotes = new[] { new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣"), new Emoji("4️⃣"), new Emoji("5️⃣"), new Emoji("6️⃣"), new Emoji("7️⃣"), new Emoji("8️⃣"), new Emoji("9️⃣") };

            StringBuilder desc = new StringBuilder();
            int i = 0;
            foreach (var c in choices)
            {
                desc.AppendLine(emotes[i] + ": " + c);
                i++;
            }

            var msg = await ReplyAsync(embed: new EmbedBuilder
            {
                Title = title,
                Color = Color.Blue,
                Description = desc.ToString()
            }.Build());
            await msg.AddReactionsAsync(emotes.Take(i).ToArray());
        }
*/