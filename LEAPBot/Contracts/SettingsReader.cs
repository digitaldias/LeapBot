using LEAPBot.Domain.Contracts;
using Microsoft.Azure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace LEAPBot.Contracts
{
    public class SettingsReader : ISettingsReader
    {
        private JObject _json;


        public string this[string index]
        {
            get
            {
                var settingValue = CloudConfigurationManager.GetSetting(index);

                if (string.IsNullOrEmpty(settingValue))
                    settingValue = GetByLocalJsonFile(index);

                return settingValue;
            }
        }


        private string GetByLocalJsonFile(string index)
        {
            if(_json == null)
                LoadJsonFromUserProfileFolder();

            if (!_json.HasValues)
                return string.Empty;

            return _json[index].Value<string>();
        }


        private void LoadJsonFromUserProfileFolder()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "LeapBotSettings.Json");
            if (!File.Exists(path))
                return;

            var fileContent = File.ReadAllText(path);
            if (string.IsNullOrEmpty(fileContent))
                return;

            _json = JObject.Parse(fileContent);
        }
    }
}