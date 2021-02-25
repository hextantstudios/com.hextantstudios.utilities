// Copyright 2021 by Hextant Studios. https://HextantStudios.com
// This work is licensed under CC BY 4.0. http://creativecommons.org/licenses/by/4.0/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hextant.Editor
{
    // Maintains a small history of recently selected objects that can be moved
    // backward or forward through similar to a web browser.
    // F1: Move back through history.
    // Shift + F1: Move forward through history.
    public sealed class SelectionHistory : EditorSingleton<SelectionHistory>
    {
        // Initialize the singleton on editor load.
        [InitializeOnLoadMethod]
        static void OnLoad() => Initialize();

        [MenuItem( "Edit/Selection/History Back _F1" )] // F1
        static void OnBack() => instance.Back();

        [MenuItem( "Edit/Selection/History Forward #F1" )] // Shift + F1
        static void OnForward() => instance.Forward();

        // Move backwards one entry in the history.
        void Back()
        {
            // Return if there are no previous entries.
            if( _currentIndex <= 0 ) return;

            // Move backwards and remove any entries that are now null or the same.
            var selected = _current;
            while( _currentIndex > 0 &&
                IsNullOrEqual( _history[ --_currentIndex ], selected ) )
                _history.RemoveAt( _currentIndex );

            // Select the current entry if it is valid.
            if( !IsNull( _current ) )
                Selection.objects = _current;
        }

        // Move forwards one entry in the history.
        void Forward()
        {
            // Return if there are no newer entries.
            if( _currentIndex == _history.Count - 1 ) return;

            // Move forwards and remove any entries that are now null or the same.
            var selected = _current;
            while( _currentIndex < _history.Count - 1 &&
                IsNullOrEqual( _history[ ++_currentIndex ], selected ) )
                _history.RemoveAt( _currentIndex-- );

            // Select the current entry if it is valid.
            if( !IsNull( _current ) )
                Selection.objects = _current;
        }

        // Adds the specified objects entry to the history.
        void Add( Object[] objects )
        {
            // Return if the new entry is null or is the same as the current entry.
            if( IsNullOrEqual( objects, _current ) ) return;

            // Remove oldest entry if full.
            if( _history.Count == _maxHistory )
            {
                _history.RemoveAt( 0 );
                --_currentIndex;
            }

            // Remove "newer" entries.
            ++_currentIndex;
            _history.RemoveRange( _currentIndex, _history.Count - _currentIndex );

            // Add the new entry to the history.
            _history.Add( objects );
        }

        // Called initially or when the selection changes to update the history.
        void UpdateSelection() => Add( Selection.objects );

        // Called when the instance is created or after a domain reload.
        void OnEnable()
        {
            UpdateSelection();
            Selection.selectionChanged += UpdateSelection;
        }

        // Called when the instance is destroyed or before a domain reload.
        void OnDisable()
        {
            Selection.selectionChanged -= UpdateSelection;
        }

        // True if the Object array 'a' is null or "equal" to 'b'.
        static bool IsNullOrEqual( Object[] a, Object[] b ) =>
            IsNull( a ) || AreEqual( a, b );

        // True if the Object array is null or if all entries are null.
        static bool IsNull( Object[] objects )
        {
            if( objects != null )
                foreach( var obj in objects )
                    if( obj != null ) return false;
            return true;
        }

        // True if there are the same number of non-null entries that are equal.
        static bool AreEqual( Object[] a, Object[] b )
        {
            if( a == b ) return true;
            var aLength = a != null ? a.Length : 0;
            var bLength = b != null ? b.Length : 0;
            for( int ia = 0, ib = 0; ia < aLength || ib < bLength; )
            {
                if( ia < aLength && a[ ia ] == null ) ++ia;
                else if( ib < bLength && b[ ib ] == null ) ++ib;
                else if( ia >= aLength || ib >= bLength || a[ ia++ ] != b[ ib++ ] )
                    return false;
            }
            return true;
        }

        // The currently selected objects in the history.
        Objects _current => _currentIndex >= 0 ? _history[ _currentIndex ] : default;

        // The history of selected objects.
        List<Objects> _history = new List<Objects>( _maxHistory );

        // The index of the current history entry.
        int _currentIndex = -1;

        // The maximum number of items in the history.
        const int _maxHistory = 15;

        // A serializable wrapper for Object[]'s for use inside a List<>.
        [System.Serializable]
        struct Objects
        {
            // Convert from Object[] to Objects.
            public static implicit operator Objects( Object[] objects ) =>
                new Objects { _objects = objects };

            // Convert from Objects to Object[].
            public static implicit operator Object[]( Objects objects ) =>
                objects._objects;

            Object[] _objects;
        }
    }
}