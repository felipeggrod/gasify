using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

namespace GAS {
    [Serializable]
    public class JsonListWrapper<T> {//We need this because unity JSON utility doesnt serialize lists, but it does serialize an object with a list variable... Yeah yeah, makes a lot of sense, I know... They don't even warn you about that when you spend a day trying to serialize a list directly...
        public List<T> list;
        public JsonListWrapper(List<T> list) => this.list = list;

        public string ToJson(bool pretty = true) {
            return JsonUtility.ToJson(this, pretty);
        }
        public static T FromJson(string json) {
            return JsonUtility.FromJson<T>(json);
        }
    }
}