#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

#pragma warning disable 618

[CustomEditor(typeof(IsoSpriteSorting))]
public class IsoSpriteSortingEditor : Editor
{
    public void OnSceneGUI()
    {
        IsoSpriteSorting myTarget = (IsoSpriteSorting)target;

        //myTarget.SorterPositionOffset = Handles.FreeMoveHandle(myTarget.transform.position + myTarget.SorterPositionOffset, Quaternion.identity, 0.08f * HandleUtility.GetHandleSize(myTarget.transform.position), Vector3.zero, Handles.DotHandleCap) - myTarget.transform.position;
        //changed by Seb to respect scale...
        Vector3 sorter_position_world = myTarget.TransformSortingPoint( myTarget.SorterPositionOffset );
        Vector3 new_sorter_position = Handles.FreeMoveHandle(sorter_position_world, Quaternion.identity, 0.08f * HandleUtility.GetHandleSize(myTarget.transform.position), Vector3.zero, Handles.DotHandleCap);
        if( new_sorter_position != sorter_position_world )  //prevent any floating point drift
            myTarget.SorterPositionOffset = myTarget.UntransformSortingPoint( new_sorter_position );


        if (myTarget.sortType == IsoSpriteSorting.SortType.Line)
        {
            //myTarget.SorterPositionOffset2 = Handles.FreeMoveHandle(myTarget.transform.position + myTarget.SorterPositionOffset2, Quaternion.identity, 0.08f * HandleUtility.GetHandleSize(myTarget.transform.position), Vector3.zero, Handles.DotHandleCap) - myTarget.transform.position;
            //Changed by Seb to respect scale...
            Vector3 sorter_position_world2 = myTarget.TransformSortingPoint( myTarget.SorterPositionOffset2 );
            Vector3 new_sorter_position2 = Handles.FreeMoveHandle(sorter_position_world2, Quaternion.identity, 0.08f * HandleUtility.GetHandleSize(myTarget.transform.position), Vector3.zero, Handles.DotHandleCap);
            if( new_sorter_position2 != sorter_position_world2 )  //prevent any floating point drift
                myTarget.SorterPositionOffset2 = myTarget.UntransformSortingPoint( new_sorter_position2 );

            Handles.DrawLine(myTarget.TransformSortingPoint( myTarget.SorterPositionOffset ), myTarget.TransformSortingPoint( myTarget.SorterPositionOffset2 ) );
        }
        if (GUI.changed)
        {
            Undo.RecordObject(target, "Updated Sorting Offset");
            EditorUtility.SetDirty(target);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        IsoSpriteSorting myScript = (IsoSpriteSorting)target;
        if (GUILayout.Button("Sort Visible Scene"))
        {
            myScript.SortScene();
        }
    }
}
#endif
