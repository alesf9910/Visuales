namespace Visuales.Models
{
    public abstract class Link
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public bool isFile()
        {
            return this is File;
        }
    }
}
