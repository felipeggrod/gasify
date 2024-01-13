
using UnityEngine.Events;
using System.Reflection;

namespace GAS {

    // public static class GetListenerNumberExtensionMethod {
    //     public static int GetListenerNumber(this UnityEventBase unityEvent) {
    //         var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
    //         var invokeCallList = field.GetValue(unityEvent);
    //         var property = invokeCallList.GetType().GetProperty("Count");
    //         return (int)property.GetValue(invokeCallList);
    //     }
    // }
    public static class GetListener {
        public static int GetListenerNumber(this UnityEventBase unityEvent) {
            var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var invokeCallList = field.GetValue(unityEvent);
            var property = invokeCallList.GetType().GetProperty("Count");
            return (int)property.GetValue(invokeCallList);
        }
    }
}