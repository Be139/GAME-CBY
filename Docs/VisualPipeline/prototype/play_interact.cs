// Play mode: aim the robot camera at Noah and ask the real interactable whether the prompt would appear, then confirm it.
var sb = new System.Text.StringBuilder();
var ctlType = System.Type.GetType("HearthCompanion17F01ReplayController, Assembly-CSharp");
var intType = System.Type.GetType("HearthCompanionReplayInteractable, Assembly-CSharp");
var ctl = UnityEngine.Object.FindObjectOfType(ctlType);
var it = UnityEngine.Object.FindObjectOfType(intType);
var cam = UnityEngine.GameObject.Find("Player/CompanionRig/ViewCamera").GetComponent<UnityEngine.Camera>();
var rig = UnityEngine.GameObject.Find("Player/CompanionRig").transform;
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var focus = (UnityEngine.Transform)intType.GetField("focusTarget", flags).GetValue(it);
var trigger = (UnityEngine.Collider)intType.GetProperty("InteractionCollider").GetValue(it, null);
sb.AppendLine("step=" + ctlType.GetField("currentStep", flags).GetValue(ctl) + " available=" + intType.GetProperty("IsAvailable").GetValue(it, null) + " focus=" + (focus ? focus.name : "null") + " trigger=" + (trigger ? trigger.name + " on=" + trigger.enabled : "null"));
var before = cam.transform.rotation;
var aim = trigger != null ? trigger.bounds.center : focus.position;
cam.transform.LookAt(aim);
var d = UnityEngine.Vector3.Distance(cam.transform.position, aim);
var can = (bool)intType.GetMethod("CanInteract").Invoke(it, new object[] { rig, cam });
var hits = UnityEngine.Physics.RaycastAll(new UnityEngine.Ray(cam.transform.position, cam.transform.forward), 3.25f, ~0, UnityEngine.QueryTriggerInteraction.Collide);
var names = new System.Text.StringBuilder(); foreach (var h in hits) names.Append(h.collider.name + "@" + h.distance.ToString("0.00") + " ");
cam.transform.rotation = before;
sb.AppendLine("distance=" + d.ToString("0.00") + " canInteract=" + can + " rayHits: " + names);
if (can)
{
    ctlType.GetMethod("CompleteCurrentStep").Invoke(ctl, null);
    sb.AppendLine("confirmed, step=" + ctlType.GetField("currentStep", flags).GetValue(ctl));
}
return sb.ToString();
