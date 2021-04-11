using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArchCollider))]
public class ArchColliderEditor : Editor
{
    private void OnSceneGUI()
    {
        Handles.color = Color.cyan;

        ArchCollider myObj = target as ArchCollider;
        float rotationValue = (myObj.Rotation + myObj.RotationZ * 2) * Mathf.Deg2Rad;
        // DrawArc
        {
            Vector3 drawStart = new Vector3(Mathf.Cos(rotationValue * 0.5f), Mathf.Sin(rotationValue * 0.5f)) * 0.5f;

            Handles.DrawWireArc(myObj.transform.position, Vector3.forward, drawStart, myObj.Degree * 0.5f, myObj.InsideRadius);
            Handles.DrawWireArc(myObj.transform.position, Vector3.forward, drawStart, myObj.Degree * 0.5f, myObj.OutsideRadius);

            Handles.DrawWireArc(myObj.transform.position, Vector3.forward, drawStart, myObj.Degree * -0.5f, myObj.InsideRadius);
            Handles.DrawWireArc(myObj.transform.position, Vector3.forward, drawStart, myObj.Degree * -0.5f, myObj.OutsideRadius);
        }
        // DrawLine
        {
            var rot = (myObj.Degree * +0.5f) * Mathf.Deg2Rad + rotationValue * 0.5f;
            var direction = new Vector3(Mathf.Cos(rot), Mathf.Sin(rot));
            Handles.DrawLine(myObj.transform.position + myObj.InsideRadius * direction, myObj.transform.position + myObj.OutsideRadius * direction);

                rot = (myObj.Degree * -0.5f) * Mathf.Deg2Rad + rotationValue * 0.5f;
            direction = new Vector3(Mathf.Cos(rot), Mathf.Sin(rot));
            Handles.DrawLine(myObj.transform.position + myObj.InsideRadius * direction, myObj.transform.position + myObj.OutsideRadius * direction);
        }
    }
}
