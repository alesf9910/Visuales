using System.Diagnostics;
using System.Net;
using System.Xml;
using Visuales.Models;
using File = Visuales.Models.File;
namespace Visuales
{
    public class Visual
    {
        public event EventHandler? OnLoad = null;
        private HttpClient Client;
        public List<Link>? Links { get; private set; }
        public Visual()
        {
            Client = new HttpClient();
        }
        public Visual(WebProxy webProxy)
        {
            Client = new HttpClient(new HttpClientHandler()
            {
                Proxy = webProxy
            });
        }
        public Visual(string baseAddress) : this()
        {
            Client.BaseAddress = new Uri(baseAddress);
        }
        public Visual(string baseAddress, WebProxy webProxy) : this(webProxy)
        {
            Client.BaseAddress = new Uri(baseAddress);           
        }
        public async Task LoadUrl(string path = "")
        {
            Links = new List<Link>();
            string html = await Client.GetStringAsync(path);
            Dictionary<string, string> links = Helper.ExtractAs(html);
            foreach(var (innerHtml, href) in links) {
                string url = href;
                string name = innerHtml;
                if (!url.Contains("http")) url = Helper.CombineUrl(path, url);
                Link link;
                try
                {
                    link = await ProcessLink(name, url);
                }
                catch(Exception e)
                {
                    continue;
                }
                if (link != null) Links.Add(link);
            }
            if (OnLoad != null) OnLoad.Invoke(this, new EventArgs());
        }

        public async Task LoadUrl(Folder folder, bool back = false)
        {
            if (back)
            {
                await LoadUrl(Helper.GetBackUrl(folder.Url));
            }
            else
            {
                await LoadUrl(folder.Url);
            }
        }
        private async Task<Link> ProcessLink(string? name, string url)
        {
            var response = await Client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string? mt;
                var ct = response.Content.Headers.ContentType;
                if (ct == null) mt  = "text/html";
                else mt = ct?.MediaType;
                if (mt == "text/html")
                {
                    return new Folder()
                    {
                        Name = name,
                        Url = url
                    };
                }
                else
                {
                    string extension = Path.GetExtension(url);
                    File file = new()
                    {
                        Name = Helper.ProcessName(name),
                        Url = url,
                        Extension = extension,
                        FileType = Helper.FromExtension(extension)
                    };
                    response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.Content.Headers.Contains("Content-Length"))
                            file.Size = Helper.GetSize(response.Content.Headers.ContentLength);
                    }
                    return file;
                }
            }
            return null;
        }
        public List<File> GetFiles()
        {
            return (List<File>)Links.Where(link => link is File);
        }
        public List<Folder> GetFolders()
        {
            return (List<Folder>)Links.Where(link => link is Folder);
        }
    }
}
