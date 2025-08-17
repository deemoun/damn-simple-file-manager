using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using DamnSimpleFileManager;

namespace DamnSimpleFileManager.Utils
{
    public class LinkManager
    {
        private List<LinkItem> links = new();
        private string? pendingUrlForDescription = null;
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public LinkManager()
        {
            links = LinkStorage.Load();
        }

        private bool IsValidUrl(string url) =>
            url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

        public bool AddLink(string url)
        {
            string input = url.Trim();
            if (string.IsNullOrWhiteSpace(input) || !IsValidUrl(input))
                return false;

            links.Add(new LinkItem { Url = input, Description = pendingUrlForDescription });
            pendingUrlForDescription = null;
            LinkStorage.Save(links);
            return true;
        }

        public void SetDescription(string url, string description)
        {
            var item = links.FirstOrDefault(l => l.Url == url);
            if (item != null)
            {
                item.Description = description;
                LinkStorage.Save(links);
            }
            else
            {
                pendingUrlForDescription = description;
            }
        }

        public bool DeleteLink(string url)
        {
            var item = links.FirstOrDefault(l => l.Url == url);
            if (item == null) return false;
            links.Remove(item);
            LinkStorage.Save(links);
            return true;
        }

        public List<LinkItem> GetAllLinks() => links;

        public bool ImportFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            try
            {
                var info = new FileInfo(filePath);
                if (info.Length > MaxFileSizeBytes)
                {
                    Logger.Log($"Import file '{filePath}' exceeds max size; skipping import.");
                    return false;
                }

                var json = File.ReadAllText(filePath);
                var importedLinks = JsonSerializer.Deserialize<List<LinkItem>>(json);
                if (importedLinks == null) return false;
                links = importedLinks;
                LinkStorage.Save(links);
                return true;
            }
            catch (IOException ex)
            {
                Logger.LogError($"Error reading import file '{filePath}'", ex);
            }
            catch (JsonException ex)
            {
                Logger.LogError($"Error deserializing import file '{filePath}'", ex);
            }
            return false;
        }

        public bool ExportToFile(string filePath)
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(links, new JsonSerializerOptions { WriteIndented = true }));
            return true;
        }

        public bool OpenLink(string url)
        {
            var item = links.FirstOrDefault(l => l.Url == url);
            if (item == null) return false;
            Process.Start(new ProcessStartInfo(item.Url) { UseShellExecute = true });
            return true;
        }
    }
}
