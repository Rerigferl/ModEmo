global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.Linq;
global using System.Runtime.CompilerServices;
global using UnityEngine;

#if NDMF
    global using nadena.dev.ndmf;
#endif

#if UNITY_EDITOR
    global using UnityEditor;
    global using UnityEditor.Animations;

    #if NDMF
        global using nadena.dev.ndmf.animator;
    #endif

#endif

global using Object = UnityEngine.Object;