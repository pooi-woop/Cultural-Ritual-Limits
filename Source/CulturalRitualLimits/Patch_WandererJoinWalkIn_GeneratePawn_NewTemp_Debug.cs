using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System.Linq;

namespace RITLIM.CulturalRitualLimits
{
	/// <summary>
	/// 调试补丁：在触发正确的生成入口 GeneratePawn_NewTemp 后打印小人技能，
	/// 供对比"压缩前/压缩后"。定位完根因后可删。
	///
	/// 注意：Postfix 优先级设为 First，保证先于核心补丁的 Postfix（默认优先级 400）执行，
	/// 这样这里看到的技能等级是"压低前"的原始值。
	/// </summary>
	[HarmonyPatch(typeof(QuestNode_Root_WandererJoin_WalkIn), nameof(QuestNode_Root_WandererJoin_WalkIn.GeneratePawn_NewTemp))]
	public static class Patch_WandererJoinWalkIn_GeneratePawn_NewTemp_Debug
	{
		[HarmonyPrefix]
		public static void Prefix()
		{
			Log.Warning("[Cultural Ritual Limits DEBUG] GeneratePawn_NewTemp 被调用（Prefix）");

			Slate slate = QuestGen.slate;
			if (slate == null)
			{
				Log.Warning("[Cultural Ritual Limits DEBUG] Slate 是 null");
				return;
			}

			// 检查 overridePawnGenParams 是否存在
			bool hasOverride = slate.TryGet<PawnGenerationRequest>("overridePawnGenParams", out _);
			Log.Warning($"[Cultural Ritual Limits DEBUG] 是否有 'overridePawnGenParams'：{hasOverride}");
		}

		[HarmonyPostfix, HarmonyPriority(Priority.First)]
		public static void Postfix_Debug(Pawn __result)
		{
			if (__result == null)
			{
				Log.Warning("[Cultural Ritual Limits DEBUG] GeneratePawn_NewTemp 返回 null");
				return;
			}

			Log.Warning($"[Cultural Ritual Limits DEBUG] GeneratePawn_NewTemp 返回了小人：{__result.Name}");

			Slate slate = QuestGen.slate;
			if (slate == null)
			{
				Log.Warning("[Cultural Ritual Limits DEBUG] Postfix 中 Slate 是 null");
				return;
			}

			bool hasOverride = slate.TryGet<PawnGenerationRequest>("overridePawnGenParams", out _);
			Log.Warning($"[Cultural Ritual Limits DEBUG] Postfix 中是否有 'overridePawnGenParams'：{hasOverride}");

			// 显示小人当前的技能等级（优先级 First，此时核心补丁尚未压缩，看到的是原始值）
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