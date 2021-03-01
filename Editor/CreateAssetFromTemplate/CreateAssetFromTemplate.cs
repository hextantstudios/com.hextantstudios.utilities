using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

#if false
namespace Hextant.Editor
{
    public static class CreateAssetFromTemplate
    {
        // Starts the creation of a new asset file by prompting for a filename.
        public static void Create( string defaultAssetName, string assetTemplate,
            string assetTemplateIcon = null,
            System.Action<string> postCopyAction = null )
        {
            // Prefix with templates folder.
            assetTemplate = _templates + assetTemplate;

            if( File.Exists( assetTemplate ) )
            {
                // Create the post-name action and assign the asset template.
                var action = ScriptableObject.CreateInstance<DoCreateAsset>();
                action.assetTemplate = assetTemplate;
                action.postCopyAction = postCopyAction;

                // Prompt the user for a new filename for the asset.
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                    0, action, defaultAssetName,
                    AssetDatabase.GetCachedIcon( assetTemplateIcon ?? assetTemplate )
                        as Texture2D, null );
            }
            else
                Debug.LogError( "Missing asset template file: " + assetTemplate );
        }

        // Completes the creation of the external asset by copying the asset's template
        // file to the new file and opening it in its own application.
        class DoCreateAsset : EndNameEditAction
        {
            // The external asset template assigned above.
            public string assetTemplate;

            // Called after the copy succeeded, but before the asset is selected/opened.
            public System.Action<string> postCopyAction;

            // Called by the StartNameEditingIfProjectWindowExists method above to
            // complete the action once the name has been entered.
            public override void Action( int instanceId, string pathName,
                string resourceFile )
            {
                if( AssetDatabase.CopyAsset( assetTemplate, pathName ) )
                {
                    postCopyAction?.Invoke( pathName );
                    Selection.activeObject =
                        AssetDatabase.LoadAssetAtPath<Object>( pathName );
                    AssetDatabase.OpenAsset( Selection.activeObject );
                }
            }
        }

    }
}
#endif