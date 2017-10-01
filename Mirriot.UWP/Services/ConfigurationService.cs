using Miriot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace Miriot.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public async Task<Dictionary<string, string>> GetKeysAsync()
        {
            var uri = new Uri("ms-appx:///Configuration.xml");
            var sampleFile = await StorageFile.GetFileFromApplicationUriAsync(uri);

            var contents = await FileIO.ReadTextAsync(sampleFile);

            var doc = new XmlDocument();
            doc.LoadXml(contents);

            var node = doc.DocumentElement.SelectSingleNode("/api/cs");

            var l = node?.Attributes.Select(a => new { a.NodeName, a.InnerText });

            return l?.ToDictionary(t => t.NodeName, t => t.InnerText);
        }
    }
}
