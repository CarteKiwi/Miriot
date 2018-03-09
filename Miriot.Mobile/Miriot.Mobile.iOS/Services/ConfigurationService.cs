using Miriot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Miriot.iOS.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public async Task<Dictionary<string, string>> GetKeysByProviderAsync(string providerName)
        {
            var uri = new Uri("ms-appx:///Configuration.xml");
            //var sampleFile = await StorageFile.GetFileFromApplicationUriAsync(uri);

            var contents = "";// await FileIO.ReadTextAsync(sampleFile);

            var doc = new XmlDocument();
            doc.LoadXml(contents);

            var node = doc.DocumentElement.SelectSingleNode($"/api/{providerName}");

            //var l = node?.Attributes.Select(a => new { a.NodeName, a.InnerText });

            //return l?.ToDictionary(t => t.NodeName, t => t.InnerText);
            return new Dictionary<string, string>();
        }
    }
}
