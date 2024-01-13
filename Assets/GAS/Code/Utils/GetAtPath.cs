
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.IO;
using EasyButtons;

namespace GAS {
#if UNITY_EDITOR
    public static class Assets {
        public static T[] GetAtPath<T>(string path) {
            ArrayList al = new ArrayList();
            string[] fileEntries = Directory.GetFiles(/* Application.dataPath + "/" + */ path);

            foreach (string fileName in fileEntries) {
                string temp = fileName.Replace("\\", "/");
                int index = temp.LastIndexOf("/");
                string localPath = /* "Assets/" + */ path;

                if (index > 0)
                    localPath += temp.Substring(index);

                UnityEngine.Object t = UnityEditor.AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

                if (t != null)
                    al.Add(t);
            }

            T[] result = new T[al.Count];

            for (int i = 0; i < al.Count; i++)
                result[i] = (T)al[i];

            return result;
        }
    }
#endif
}