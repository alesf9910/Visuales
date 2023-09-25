using System.Text.RegularExpressions;
using Visuales.Models;

namespace Visuales
{
    internal class Helper
    {
        private static string[] Video = { ".mp4", ".mkv", ".avi", ".mov", ".mpg" };
        private static string[] Audio = { ".mp3", ".wav", ".flac", ".m4a", ".ico" };
        private static string[] Image = { ".jpg", ".jpeg", ".png", ".gif", ".svg"};
        private static string[] Subtitle = { ".srt", ".ass", ".vtt" };
        private static string[] Document = { ".doc", ".docx", ".pdf", ".txt", ".xls", ".xlsx" };
        private static string[] Compressed = { ".zip", ".rar", ".tar", ".gz" };
        private static string[] Executable = { ".ipa", ".apk", ".bat", ".cmd", ".sh", ".exe", ".app", ".command", ".bin", ".run" };
        internal static FileType FromExtension(string extension)
        {
            extension = extension.ToLower();
            if (Video.Contains(extension))
            {
                return FileType.Video;
            }
            else if (Audio.Contains(extension))
            {
                return FileType.Audio;
            }
            else if (Image.Contains(extension))
            {
                return FileType.Image;
            }
            else if (Subtitle.Contains(extension))
            {
                return FileType.Subtitle;
            }
            else if (Document.Contains(extension))
            {
                return FileType.Document;
            }
            else if (Compressed.Contains(extension))
            {
                return FileType.Compressed;
            }
            else if (Executable.Contains(extension))
            {
                return FileType.Executable;
            }
            return FileType.Other;
        }
        internal static string GetSize(long? size)
        {
            if (size == null) return "0 B";
            string[] units = { "B", "KB", "MB", "GB" };
            int indexUnit = 0;
            while(size > 1024 && indexUnit < units.Length-1)
            {
                size = size / 1024;
                indexUnit++;
            }
            return string.Concat(size, " ", units[indexUnit]);
        }
        internal static string GetBackUrl(string url)
        {
            return new Uri(new Uri(url), "..").AbsoluteUri;
        }
        internal static Dictionary<string,string> ExtractAs(string html)
        {
            Dictionary<string, string> res = new();
            Regex reg = new Regex("<a[^>]*?href=\"(.*?)\"[^>]*?>(.*?)</a>");
            var matches = reg.Matches(html);
            foreach(Match match in matches)
            {
                string innerText = match.Groups[2].Value;
                string href = match.Groups[1].Value;
                if (string.IsNullOrEmpty(href)) continue;
                res[innerText] = href;
            }
            return res;
        }
        internal static string CombineUrl(params string[] urls)
        {
            string url = urls[0];
            for (int i = 1; i < urls.Length; i++)
            {
                if (url[url.Length - 1] == '/')
                {
                    url += urls[i];
                }
                else
                {
                    url += "/" + urls[i];
                }

            }
            return url;
        }
        internal static string ProcessName(string file)
        {
            int pos = file.LastIndexOf(".");
            if(pos != -1)
            {
                return file.Substring(0, pos);
            }
            return file;
        }
        
    }
}
