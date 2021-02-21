// Copyright 2021 by Hextant Studios. https://HextantStudios.com
// This work is licensed under CC BY 4.0. http://creativecommons.org/licenses/by/4.0/
using UnityEngine;

// A ScriptableObject-based singleton that can be used for GUI-less editor plug-ins.
// Note: OnEnable() / OnDisable() should be used to register with any global events
// to properly support domain reloads.
public abstract class EditorSingleton<T> : ScriptableObject
    where T : EditorSingleton<T>
{
    // The singleton instance. (Not thread safe but fine for ScriptableObjects.)
    public static T instance => _instance != null ? _instance : Initialize();
    static T _instance;

    // Finds or creates the singleton instance and stores it in _instance. This can
    // be called from a derived type to ensure creation of the singleton using the 
    // [InitializeOnLoadMethod] attribute on a static method.
    protected static T Initialize()
    {
        // If the instance is already valid, return it. Needed if called from a 
        // derived class that wishes to ensure the instance is initialized.
        if( _instance != null ) return _instance;

        // Find the existing instance (across domain reloads) or create a new one.
        var instances = Resources.FindObjectsOfTypeAll<T>();
        return instances.Length > 0 ? _instance = instances[ 0 ] :
            CreateInstance<T>();
    }

    // Called once during creation of this instance. Derived classes should call
    // this base method first if overridden.
    protected virtual void Awake()
    {
        // Verify there is only a single instance; catches accidental creation
        // from other CreateInstance() calls.
        Debug.Assert( _instance == null );

        // Ensure _instance is assigned here to prevent possible double-creation
        // should the instance property be called by a derived class handler.
        _instance = ( T )this;

        // HideAndDontSave prevents Resources.UnloadUnusedAssets() from destroying
        // the singleton instance if called or when new scenes are loaded.
        _instance.hideFlags = HideFlags.HideAndDontSave;
    }
}
