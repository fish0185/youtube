using System;

using JustSaying.Models;

namespace YoutubeApp.Domain
{
    public class CreateSongCommand : Message
    {
        public string Url { get; set; }
        public string Email { get; set; }
    }
}
