// Copyright 2021 by Hextant Studios. https://HextantStudios.com
// This work is licensed under CC BY 4.0. http://creativecommons.org/licenses/by/4.0/
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Hextant.Editor
{
    // Specifies the settings' scope (Project or User), path in the UI, and
    // optionally its filename. If the filename is not set, the type's name is used.
    //
    // *Important* Per-user settings are saved in "Assets/Settings/Editor/User/"
    //   This folder needs to be excluded from source control.
    //
    // Note: User settings will be placed in a subdirectory named the same as
    // the current project folder so that shallow cloning (symbolic links to
    // the Assets/ folder) can be used when testing multiplayer games.
    public sealed class EditorSettingsAttribute : Attribute
    {
        public EditorSettingsAttribute( SettingsScope scope, string displayPath,
            string filename = null )
        {
            this.scope = scope;
            this.filename = filename;
            this.displayPath = _displayPathPrefix[ ( int )scope ] + displayPath;
        }

        // The type of settings User (Preferences) or Project.
        public readonly SettingsScope scope;

        // The display name and optional path in the settings dialog.
        public readonly string displayPath;

        // The filename used to store the settings. If null, the type's name is used.
        public readonly string filename;

        // Path prefix required by the settings dialogs.
        static readonly string[] _displayPathPrefix = { "Preferences/", "Project/" };
    }

    // Base class for editor settings. Use the [EditorSettings] attribute to
    // specify its scope, display path, and filename.
    public abstract class EditorSettings<T> : ScriptableObject
        where T : EditorSettings<T>
    {
        // The singleton instance. (Not thread safe but fine for ScriptableObjects.)
        public static T instance => _instance != null ? _instance : Initialize();
        static T _instance;

        // Loads or creates the settings instance and stores it in _instance.
        static T Initialize()
        {
            // Verify there was an [EditorSettings] attribute.
            if( attribute == null )
            {
                Debug.LogError( "[EditorSettings] attribute missing for: " +
                    typeof( T ).Name );
                return null;
            }

            // Attempt to load the settings asset.
            var path = GetAssetPath();
            if( ( _instance = AssetDatabase.LoadAssetAtPath<T>( path ) ) != null )
                return _instance;

            // Move settings if its path changed (type renamed, attribute changed)
            // while the editor was running. This must be done manually if the
            // change was made outside the editor.
            var instances = Resources.FindObjectsOfTypeAll<T>();
            if( instances.Length > 0 )
            {
                var oldPath = AssetDatabase.GetAssetPath( instances[ 0 ] );
                var result = AssetDatabase.MoveAsset( oldPath, path );
                if( string.IsNullOrEmpty( result ) )
                    return _instance = instances[ 0 ];
                else
                    Debug.LogWarning( $"Failed to move previous settings asset " +
                        $"'{oldPath}' to '{path}'. " +
                        $"A new settings asset will be created." );
            }

            // Create a new settings instance if it was not found.
            // Create the directory as Unity does not do this itself.
            Directory.CreateDirectory( Path.Combine(
                Directory.GetCurrentDirectory(),
                Path.GetDirectoryName( path ) ) );

            // Create the settings asset and save it.
            _instance = CreateInstance<T>();
            AssetDatabase.CreateAsset( _instance, path );
            AssetDatabase.SaveAssets();
            return _instance;
        }

        // Returns the full asset path to the settings file.
        static string GetAssetPath()
        {
            // Get the settings filename from its attribute (or type if null).
            var filename = attribute.filename ?? typeof( T ).Name;
            var projectFolder = attribute.scope == SettingsScope.User ?
                ( GetProjectFolderName() + '/' ) : "";
            return $"Assets/Settings/Editor/{attribute.scope}/{projectFolder}" +
                $"{filename}.asset";
        }

        // The derived type's [EditorSettings] attribute.
        public static EditorSettingsAttribute attribute =>
            _attribute != null ? _attribute : _attribute =
                typeof( T ).GetCustomAttribute<EditorSettingsAttribute>( true );
        static EditorSettingsAttribute _attribute;

        // Marks the settings dirty so that it will be saved.
        public new void SetDirty() => EditorUtility.SetDirty( this );

        // Sets the specified setting to the desired value and marks the settings
        // so that it will be saved.
        protected void Set<S>( ref S setting, S value )
        {
            if( EqualityComparer<S>.Default.Equals( setting, value ) ) return;
            setting = value;
            SetDirty();
        }

        // The directory name of the current project folder.
        static string GetProjectFolderName()
        {
            var path = Application.dataPath.Split( '/' );
            return path[ path.Length - 2 ];
        }

        // The SettingsProvider instance used to display settings in Edit/Preferences
        // and Edit/Project Settings.
        public static implicit operator ScriptableObjectSettingsProvider(
            EditorSettings<T> settings ) =>
            settings._provider != null ? settings._provider :
            settings._provider = new ScriptableObjectSettingsProvider( settings,
                attribute.scope, attribute.displayPath );
        ScriptableObjectSettingsProvider _provider;
    }
}