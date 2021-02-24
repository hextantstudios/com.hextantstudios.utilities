using UnityEditor;
using UnityEngine;

namespace Hextant.Editor
{
    // SettingsProvider helper used to display settings for a ScriptableObject
    // derived class.
    public class ScriptableObjectSettingsProvider : SettingsProvider
    {
        public ScriptableObjectSettingsProvider( ScriptableObject settings,
            SettingsScope scope, string displayPath ) :
            base( displayPath, scope ) => this.settings = settings;

        // The settings instance being edited.
        public readonly ScriptableObject settings;

        // The SerializedObject settings instance.
        public SerializedObject serializedSettings =>
            _serializedSettings != null ? _serializedSettings :
            _serializedSettings = new SerializedObject( settings );
        SerializedObject _serializedSettings;

        // Displays the settings.
        public override void OnGUI( string searchContext ) =>
            DrawDefaultInspector( serializedSettings );

        // Draws the UI for exposed properties.
        protected static bool DrawDefaultInspector( SerializedObject obj )
        {
            if( obj.targetObject == null ) return false;

            EditorGUI.BeginChangeCheck();
            obj.UpdateIfRequiredOrScript();

            // Set label width and indentation.
            EditorGUIUtility.labelWidth = 250;
            EditorGUI.indentLevel = 1;

            // Iterate over the object's fields.
            var property = obj.GetIterator();

            // Skip "m_Script" property.
            property.NextVisible( true );

            // Iterate remaining properties.
            while( property.NextVisible( false ) )
                EditorGUILayout.PropertyField( property, true );

            // Reset label width.
            EditorGUIUtility.labelWidth = 0;

            obj.ApplyModifiedProperties();
            return EditorGUI.EndChangeCheck();
        }

        // Build the set of keywords on demand from the settings fields.
        public override bool HasSearchInterest( string searchContext )
        {
            if( !_keywordsBuilt )
            {
                keywords = GetSearchKeywordsFromSerializedObject(
                    serializedSettings );
                _keywordsBuilt = true;
            }
            return base.HasSearchInterest( searchContext );
        }

        bool _keywordsBuilt;
    }
}