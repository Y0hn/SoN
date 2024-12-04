using UnityEditor;

namespace Pathfinding.Legacy {
	[System.Obsolete]
	[CustomEditor(typeof(LegacyAIPath))]
	[CanEditMultipleObjects]
	public class LegacyAIPathEditor : BaseAIEditor {
        [System.Obsolete]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

        protected override void Inspector () {
			base.Inspector();
			var gravity = FindProperty("gravity");
			if (!gravity.hasMultipleDifferentValues && !float.IsNaN(gravity.vector3Value.x)) {
				gravity.vector3Value = new UnityEngine.Vector3(float.NaN, float.NaN, float.NaN);
				serializedObject.ApplyModifiedPropertiesWithoutUndo();
			}
			LegacyEditorHelper.UpgradeDialog(targets, typeof(AIPath));
		}
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
    }
}
