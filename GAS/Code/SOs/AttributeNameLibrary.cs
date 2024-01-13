using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using EasyButtons;
using System.IO;
using System.Threading.Tasks;
#pragma warning disable 0618

namespace GAS {

    [CreateAssetMenu(menuName = "GAS/AttributeNameLibrary", fileName = "AttributeNameLibrary")]
    [Serializable]
    public class AttributeNameLibrary : SingletonScriptableObjectLibrary<AttributeNameLibrary, AttributeName> {

    }
}

#pragma warning restore 0618