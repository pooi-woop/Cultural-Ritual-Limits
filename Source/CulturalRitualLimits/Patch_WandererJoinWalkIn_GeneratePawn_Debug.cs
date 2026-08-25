using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System.Linq;

namespace RITLIM.CulturalRitualLimits
{
	/// <summary>
	/// 调试补丁：帮助诊断为什么主补丁不起效果
	/// </summary>
	[HarmonyPatch(typeof(QuestNode_Root_WandererJoin_WalkIn), nameof(QuestNode_Root_WandererJoin_WalkIn.GeneratePawn))]
	public static class Patch_WandererJoinWalkIn_GeneratePawn_Debug
	{
		[HarmonyPrefix]
		public static void Prefix()
		{
			Log.Warning("[Cultural Ritual Limits DEBUG] GeneratePawn 被调用（Prefix）");

			Slate slate = QuestGen.slate;
			if (slate == null)
			{
				Log.Warning("[Cultural Ritual Limits DEBUG] Slate 是 null");
				return;
			}

			// 检查 overridePawnGenParams 是否存在
			bool hasOverride = slate.TryGet<PawnGenerationRequest>("overridePawnGenParams", out var genParams);
			Log.Warning($"[Cultural Ritual Limits DEBUG] 是否有 'overridePawnGenParams'：{hasOverride}");

			if (hasOverride)
			{
				Log.Warning($"[Cultural Ritual Limits DEBUG] PawnGenerationRequest 找到了！");
			}
		}

		[HarmonyPostfix]
		public static void Postfix_Debug(Pawn __result)
		{
			if (__result == null)
			{
				Log.Warning("[Cultural Ritual Limits DEBUG] GeneratePawn 返回 null");
				return;
			}

			Log.Warning($"[Cultural Ritual Limits DEBUG] GeneratePawn 返回了小人：{__result.Name}");

			Slate slate = QuestGen.slate;
			if (slate == null)
			{
				Log.Warning("[Cultural Ritual Limits DEBUG] Postfix 中 Slate 是 null");
				return;
			}

			bool hasOverride = slate.TryGet<PawnGenerationRequest>("overridePawnGenParams", out _);
			Log.Warning($"[Cultural Ritual Limits DEBUG] Postfix 中是否有 'overridePawnGenParams'：{hasOverride}");

			// 显示小人当前的技能等级
			if (__result.skills != null)
			{
				Log.Warning("[Cultural Ritual Limits DEBUG] 小人技能（压低前）：");
				foreach (var skill in __result.skills.skills.Where(s => s.levelInt > 0))
				{
					Log.Warning($"  - {skill.def.defName}: Lv{skill.levelInt}, 兴趣={skill.passion}");
				}
			}
		}
	}
}
