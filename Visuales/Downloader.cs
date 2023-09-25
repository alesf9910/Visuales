using System.Net;
using System.Net.Http.Headers;
using Visuales.Models;

namespace Visuales
{
    public class Downloader
    {
        CancellationTokenSource cancellationTokenSource = new();
        public delegate void onMerge(long size, long merge);
        public event onMerge? OnMerge= null;
        public delegate void onComplete();
        public event onComplete? OnComplete = null;
        public delegate void onDownloading(long size, long download, int threadId);
        public event onDownloading? OnDownloading = null;
        List<DownloadControl> DownloadControls { get; set; }
        HttpClient Client { get; set; }
        public long FileSize { get; internal set; }
        public string Url { get; internal set; }
        public int Threads { get; internal set; }
        public string Path { get; internal set; }
        public Downloader(string url, string path, int threads)
        {
            Client = new HttpClient();
            Url = url;
            Threads = threads;
            Path = path;
        }
        public Downloader(string url, int threads, string path, WebProxy webProxy)
        {
            Client = new HttpClient(new HttpClientHandler()
            {
                Proxy=webProxy
            });
            Url = url;
            Threads = threads;
            Path = path;
        }
        public async Task StartAsync()
        {
            if (DownloadControls == null)
            {
                await GenerateDownloadControls();
            }
            string dirId = Guid.NewGuid().ToString();
            var dirInfo = Directory.CreateTempSubdirectory(dirId);
            Task[] tasks = new Task[Threads];
            for (int i = 0; i < DownloadControls.Count; i++)
            {
                string file = System.IO.Path.Combine(dirInfo.FullName, i.ToString());
                tasks[i] = Download(i, file, cancellationTokenSource.Token);
            }
            try
            {
                await Task.WhenAll(tasks);
                if (OnComplete != null) OnComplete.Invoke();
                await GenerateFile(dirInfo);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                // Limpieza de recursos: eliminar el directorio temporal
                Directory.Delete(dirInfo.FullName, true);
            }
        }

        public async void Start()
        {
            await StartAsync();
        }
        private async Task GenerateFile(DirectoryInfo dirInfo)
        {
            using FileStream fileStream = new FileStream(Path, FileMode.Create);
            long merge = 0;
            for (int i = 0; i < Threads; i++)
            {
                string file = System.IO.Path.Combine(dirInfo.FullName, i.ToString());
                using FileStream read = new FileStream(file, FileMode.Open);
                byte[] buffer = new byte[10240];
                int bytesRead;
                while ((bytesRead = await read.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    merge += bytesRead;
                    if (OnMerge != null) OnMerge.Invoke(FileSize, merge);
                }
            }
        }
        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }
        private async Task GenerateDownloadControls()
        {
            DownloadControls = new();
            var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, Url));
            if (!response.IsSuccessStatusCode) throw new Exception("Error to connect with server");
            FileSize = response.Content.Headers.ContentLength.GetValueOrDefault(0);
            if (FileSize == 0) throw new Exception("Invalid file size");
            long mega = 1024 * 1024;
            if (FileSize <= mega)
            {
                DownloadControls.Add(new DownloadControl()
                {
                    StartByte = 0,
                    EndByte = FileSize
                });
            }
            else
            {
                long max_size = FileSize / Threads;
                long rest = FileSize % Threads;
                long start = 0;
                for (int i = 1; i < Threads; i++)
                {
                    DownloadControls.Add(new DownloadControl()
                    {
                        StartByte = start,
                        EndByte = start + max_size - 1
                    });
                    start += max_size;
                }
                DownloadControls.Add(new DownloadControl()
                {
                    StartByte = start,
                    EndByte = start + max_size - 1 + rest
                });
            }
        }

        private async Task Download(int id, string file, CancellationToken token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Url);
            request.Headers.Range = new RangeHeaderValue(DownloadControls[id].StartByte, DownloadControls[id].EndByte);
            var response = await Client.SendAsync(request);
            using FileStream fileStream = new FileStream(file, FileMode.Append);
            using Stream read = await response.Content.ReadAsStreamAsync();
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = await read.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                DownloadControls[id].StartByte += bytesRead;
                if (OnDownloading != null) OnDownloading.Invoke(DownloadControls[id].EndByte,
                    DownloadControls[id].StartByte, id);
                if (token.IsCancellationRequested) throw new OperationCanceledException();
            }
        }

    }
}
