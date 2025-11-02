using UnityEngine;

namespace PlayingCard.Utilities
{
    public static class ClientPrefs
    {
        const string ClientGUIDKey = "Client_GUID";

        public static string GetGuid()
        {
            if (PlayerPrefs.HasKey(ClientGUIDKey))
            {
                return PlayerPrefs.GetString(ClientGUIDKey);
            }

            var guid = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(ClientGUIDKey, guid);

            return guid;
        }

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(GetKey(key));
        }

        public static void DeleteKey(string key)
        {
            if (HasKey(key))
                PlayerPrefs.DeleteKey(GetKey(key));
        }

        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(GetKey(key), value);
        }

        public static int GetInt(string key)
        {
            if (HasKey(key))
                return PlayerPrefs.GetInt(GetKey(key));
            else
            {
                SetInt(key, 0);
                return 0;
            }
        }

        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(GetKey(key), value);
        }

        public static float GetFloat(string key)
        {
            if (HasKey(key))
                return PlayerPrefs.GetFloat(GetKey(key));
            else
            {
                SetFloat(key, 0);
                return 0;
            }
        }

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(GetKey(key), value);
        }

        public static string GetString(string key)
        {
            if (HasKey(key))
                return PlayerPrefs.GetString(GetKey(key));
            else
            {
                SetString(key, string.Empty);
                return string.Empty;
            }
        }

        static string GetKey(string key)
        {
            return $"{GetGuid()}|{key}";
        }
    }
}
