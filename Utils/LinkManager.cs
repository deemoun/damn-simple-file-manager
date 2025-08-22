using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DamnSimpleFileManager.Utils
{
    public class LinkManager
    {
        private List<LinkItem> links = new();
        private string? pendingUrlForDescription = null;

        public LinkManager()
        {
            links = LinkStorage.Load();
        }

        private bool IsValidUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

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
            var json = File.ReadAllText(filePath);
            var imported = JsonSerializer.Deserialize<List<LinkItem>>(json) ?? new();
            var validLinks = new List<LinkItem>();
            foreach (var item in imported)
            {
                if (item != null && IsValidUrl(item.Url))
                {
                    validLinks.Add(item);
                }
                else
                {
                    Logger.Log($"Skipping invalid URL during import: {item?.Url}");
                }
            }
            links = validLinks;
            LinkStorage.Save(links);
            return true;
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
            if (!IsValidUrl(item.Url))
            {
                Logger.Log($"Blocked attempt to open invalid URL: {item.Url}");
                return false;
            }
            Process.Start(new ProcessStartInfo(item.Url) { UseShellExecute = true });
            return true;
        }
    }
}
