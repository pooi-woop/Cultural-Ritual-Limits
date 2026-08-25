using HarmonyLib;
using RimWorld;
using Verse;

namespace RITLIM.CulturalRitualLimits
{
	/// <summary>
	/// 调试补丁2：监听招募仪式的结果触发
	/// </summary>
	[HarmonyPatch(typeof(RitualAttachableOutcomeEffectWorker_RandomRecruit), "Apply")]
	public static class Patch_RitualOutcome
	{
		[HarmonyPrefix]
		public static void Prefix()
		{
			Log.Warning("[Cultural Ritual Limits DEBUG] 招募仪式结果正在应用（RandomRecruit.Apply）");
		}

		[HarmonyPostfix]
		public static void Postfix()
		{
			Log.Warning("[Cultural Ritual Limits DEBUG] 招募仪式 Apply 方法执行完毕");
		}
	}
}
