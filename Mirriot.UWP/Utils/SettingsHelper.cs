using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace Miriot.Win10.Utils
{
    public static class SettingsHelper
    {
        private static ApplicationDataContainer _roamingSetting = ApplicationData.Current.RoamingSettings;
        private static StorageFolder Store = ApplicationData.Current.RoamingFolder;

        public static T GetRoamingValue<T>(string key, object defaultValue)
        {
            if (_roamingSetting.Values.ContainsKey(key))
                return (T)_roamingSetting.Values[key];
            return (T)defaultValue;
        }

        public static void SetRoamingValue(string key, object obj)
        {
            if (_roamingSetting.Values.ContainsKey(key))
                _roamingSetting.Values[key] = obj;
            else
                _roamingSetting.Values.Add(key, obj);
        }

        public static T GetValue<T>(string key, T defaultValue)
        {
            if (_roamingSetting.Values.ContainsKey(key))
            {
                return (T)_roamingSetting.Values[key];
            }

            return defaultValue;
        }

        public static object GetValue(string key, object defaultValue)
        {
            if (_roamingSetting.Values.ContainsKey(key))
                return _roamingSetting.Values[key];
            return defaultValue;
        }

        public static void SetValue(string key, object obj)
        {
            _roamingSetting.Values[key] = obj;
        }

        private async static Task<StorageFile> GetFileIfExistsAsync(StorageFolder folder, string fileName)
        {
            try
            {
                return await folder.GetFileAsync(fileName);

            }
            catch
            {
                return null;
            }
        }

        public static async Task<T> LoadObjectFromStorage<T>(Windows.Storage.StorageFolder storageFolder)
        {
            T ObjToLoad = default(T);

            try
            {
                StorageFile storageFile = await storageFolder.CreateFileAsync(GetFileName<T>(), CreationCollisionOption.OpenIfExists);

                using (Stream inStream = await storageFile.OpenStreamForReadAsync())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    ObjToLoad = (T)serializer.Deserialize(inStream);
                }
            }
            catch (Exception error)
            {
                throw new NotImplementedException(error.Message);
            }

            return ObjToLoad;
        }

        public static async void SaveObjectToStorage<T>(T ObjectToSave, Windows.Storage.StorageFolder storageFolder)
        {
            string filename = GetFileName<T>();

            using (Stream fs = await storageFolder.OpenStreamForWriteAsync(filename, CreationCollisionOption.ReplaceExisting))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(T));
                    ser.Serialize(sw, ObjectToSave);
                }
            }
        }

        public static string GetFileName<T>()
        {
            return typeof(T).FullName + ".xml";
        }

        public async static Task<bool> IsObjectPersisted<T>(Windows.Storage.StorageFolder storageFolder)
        {
            string file = GetFileName<T>();

            StorageFile storageFile = await GetFileIfExistsAsync(storageFolder, file);

            return (storageFile != null);
        }

        public static T LoadSettingFromStorage<T>(string Key, Windows.Storage.ApplicationDataContainer storageSettings)
        {
            T ObjToLoad = default(T);

            if (storageSettings.Values.ContainsKey(Key))
            {
                using (StringReader sr = new StringReader((string)storageSettings.Values[Key]))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    ObjToLoad = (T)serializer.Deserialize(sr);
                }
            }

            return ObjToLoad;
        }

        public static void SaveSettingToStorage(string Key, object Setting, Windows.Storage.ApplicationDataContainer storageSettings)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                XmlSerializer ser = new XmlSerializer(Setting.GetType());
                ser.Serialize(sw, Setting);
            }

            if (!storageSettings.Values.ContainsKey(Key))
            {
                storageSettings.Values.Add(Key, sb.ToString());
            }
            else
            {
                storageSettings.Values[Key] = sb.ToString();
            }

        }

        public static bool IsSettingPersisted(string Key, Windows.Storage.ApplicationDataContainer storageSettings)
        {
            return storageSettings.Values.ContainsKey(Key);
        }
    }
}
