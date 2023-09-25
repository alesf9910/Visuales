using Visuales;
using File = Visuales.Models.File;

Visual visual = new Visual();
visual.OnLoad += OnLoad;

while (true)
{
    Console.Write("Url>> ");
    string url = Console.ReadLine();
    await visual.LoadUrl(url);
}


void OnLoad(object? sender, EventArgs e)
{
    foreach (var link in visual.Links)
    {
        if (link is File)
        {
            var file = link as File;
            Console.WriteLine(file.Name);
            Console.WriteLine(file.Extension);
            Console.WriteLine(file.Size);
            Console.WriteLine(file.Url);
            Console.WriteLine(file.FileType);
        }
        else
        {
            Console.WriteLine(link.Name);
            Console.WriteLine(link.Url);
        }
        Console.WriteLine("***************************");
    }
}