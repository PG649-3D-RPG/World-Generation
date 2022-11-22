using System.Collections.Generic;
using UnityEngine;

public class BoneDefinition {
    public BoneDefinition ParentBone;

    public HashSet<BoneDefinition> ChildBones;

    public float Length;

    /// Direction pointing along the bone (towards the child bone / away from the torso).
    public Vector3 ProximalAxis;

    /// Direction pointing towards the front of the body part.
    public Vector3 VentralAxis;

    public BoneCategory Category;

    public AttachmentHint AttachmentHint;

    public float Thickness;

    public BoneDefinition() {
        ChildBones = new();
        AttachmentHint = new();
    }

    public void LinkChild(BoneDefinition child) {
        this.ChildBones.Add(child);
        child.ParentBone = this;
    }

    public void PropagateAttachmentRotation(float angle) {
        VentralAxis = Quaternion.AngleAxis(angle, ProximalAxis) * VentralAxis;
        foreach (var child in ChildBones) {
            child.PropagateAttachmentRotation(angle);
        }
    }
}

public static class AttachmentPoints {
    public const float ProximalPoint = 0.0f;
    public const float MidPoint = 0.5f;
    public const float DistalPoint = 1.0f;
}
/// Provides a hint for how a bone should be attached to its parent.
/// The world-space VentralAxis of the bone should point towards the VentralDirection
/// after attachment.
public class AttachmentHint {
    public float AttachmentPoint;
    public Vector3? VentralDirection;
    public Vector3? Offset;
    public Quaternion? Rotation;

    public AttachmentHint() {
        AttachmentPoint =  AttachmentPoints.DistalPoint;
        VentralDirection = null;
        Offset = null;
        Rotation = null;
    }
}