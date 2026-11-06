// Play mode only: enter the 17F01 replay through the real game entry.
if (!UnityEngine.Application.isPlaying) return "not playing";
var t = System.Type.GetType("MinLoopFlowController, Assembly-CSharp");
var flow = UnityEngine.Object.FindObjectOfType(t);
if (flow == null) return "no flow controller";
t.GetMethod("SetActiveReplayResident", new[] { typeof(string) }).Invoke(flow, new object[] { "17F01" });
t.GetMethod("RequestReplayFromTerminal").Invoke(flow, null);
return "requested replay";
