using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if UNITY_EDITOR
using System.Security.Cryptography;
using System.Text;
#endif

using UnityEngine;

namespace PlayingCard.Utilities
{
    public class ProfileManager
    {
        public const string AuthProfileCommandLineArg = "-AuthProfile";
        const string AvailableProfilesKey = "AvailableProfiles";

        string profile;

        public string Profile
        {
            get
            {
                if (profile == null)
                {
                    profile = GetProfile();
                }

                return profile;
            }
            set
            {
                profile = value;
                onProfileChanged?.Invoke(profile);
            }
        }

        public event Action<string> onProfileChanged;

        List<string> availableProfiles;

        public ReadOnlyCollection<string> AvailableProfiles
        {
            get
            {
                if (availableProfiles == null)
                {
                    LoadProfiles();
                }

                return availableProfiles.AsReadOnly();
            }
        }

        public void CreateProfile(string profile)
        {
            availableProfiles.Add(profile);
            SaveProfiles();
        }

        public void DeleteProfile(string profile)
        {
            if (availableProfiles.Contains(profile))
                availableProfiles.Remove(profile);
            SaveProfiles();
        }

        public string GetProfile()
        {
            var arguments = Environment.GetCommandLineArgs();
            for (var i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == AuthProfileCommandLineArg)
                {
                    var profileId = arguments[i + 1];
                    return profileId;
                }
            }

#if UNITY_EDITOR
            // 에디터에서 실행 중일 때, Application.dataPath를 기반으로 고유 ID를 생성.
            // 이 방식은 프로젝트를 수동으로 복제하거나 Virtual Projects를 사용할 때도 작동.
            // 특정 dataPath에 대해 에디터 인스턴스는 하나만 열 수 있으므로, 고유성이 보장.
            var hashedBytes = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(Application.dataPath));
            Array.Resize(ref hashedBytes, 16);
            // GUID 문자열의 처음 30자를 잘라내는 것으로도 고유성을 확보할 수 있다.
            return new Guid(hashedBytes).ToString("N")[..30];
#else
            return string.Empty;
#endif
        }

        void LoadProfiles()
        {
            var loadedProfilesJson = ClientPrefs.GetString(AvailableProfilesKey);
            availableProfiles = JsonUtility.FromJson<List<string>>(loadedProfilesJson).FindAll(p => p.Length > 0);
        }

        void SaveProfiles()
        {
            var saveProfilesJson = JsonUtility.ToJson(availableProfiles);
            ClientPrefs.SetString(AvailableProfilesKey, saveProfilesJson);
        }
    }
}
