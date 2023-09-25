namespace Visuales.Models
{
    public class File : Link
    {
        public string? Size { get; set; }
        public FileType FileType { get; set; }
        public string? Extension { get; set; }
    }
}
