using HarmonyLib;          // [HarmonyPatch]、[HarmonyPostfix] 特性
using RimWorld;            // SkillRecord 等
using RimWorld.QuestGen;   // QuestGen、Slate、QuestNode_Root_WandererJoin_WalkIn 所在的命名空间
using Verse;               // Pawn、PawnGenerationRequest、Passion 等

namespace RITLIM.CulturalRitualLimits
{
	/// <summary>
	/// 核心补丁：压低"文化仪式招募"来的小人的技能等级。
	///
	/// 【原版机制科普——为什么补丁打在这里】
	/// Ideology DLC 的招募仪式（Recruitment）判定成功时，并不是当场生成小人，
	/// 而是由 RitualAttachableOutcomeEffectWorker_RandomRecruit 触发一个
	/// "WandererJoins（流浪者加入）"任务（Quest）。触发前，它会在任务的"石板"
	/// （Slate，一个按名字存参数的容器）里写入一个名为 "overridePawnGenParams" 的
	/// 小人生成参数（PawnGenerationRequest）。
	///
	/// 随后任务节点 QuestNode_Root_WandererJoin_WalkIn.GeneratePawn() 负责真正生成小人，
	/// 它的逻辑是：
	///     如果 Slate 里存在 "overridePawnGenParams" → 用这个参数生成（仪式招募的情况）；
	///     否则 → 用一套默认参数生成（其他来源，比如部分事件触发的流浪者）。
	///
	/// 所以只要在 GeneratePawn 跑完后检查 Slate 里有没有 "overridePawnGenParams"，
	/// 就能精确锁定"文化仪式招募的小人"，绝不误伤其他来源的小人。
	/// （原版代码里只有招募仪式会写这个键——已通过反编译源码逐一核实。）
	///
	/// 另外：游戏开局的"随机流浪者加入"事件（IncidentWorker_WandererJoin）根本不走
	/// 任务系统，而是自己直接生成小人，所以天然不受影响。
	/// </summary>
	[HarmonyPatch(typeof(QuestNode_Root_WandererJoin_WalkIn), nameof(QuestNode_Root_WandererJoin_WalkIn.GeneratePawn))]
	public static class Patch_WandererJoinWalkIn_GeneratePawn
	{
		/// <summary>
		/// Postfix（后置补丁）：在原方法执行完毕后运行。
		///
		/// Harmony 特殊参数说明：
		///   __result —— 原方法的返回值，这里就是刚生成好的小人（Pawn）。
		///              用 __result 接收即可读取；本补丁不需要替换返回值，所以不加 ref。
		/// </summary>
		[HarmonyPostfix]
		public static void Postfix(Pawn __result)
		{
			// 防空指针：万一生成失败（理论上不会），直接不处理
			if (__result == null)
			{
				return;
			}

			// QuestGen.slate 是"当前正在生成的任务"的石板，只在任务生成期间有效。
			// GeneratePawn 恰好是在任务生成过程中被调用的，所以这里一定能拿到。
			// 保险起见还是判一下空。
			Slate slate = QuestGen.slate;
			if (slate == null)
			{
				return;
			}

			// 关键判断：Slate 里存在 "overridePawnGenParams" ⇔ 这个小人是文化仪式招募来的。
			// （原版只有招募仪式会写入这个键；其他途径触发 WandererJoins 任务时不会写。）
			// TryGet 的第二个参数是 out 输出，我们只需要知道"是否存在"，所以用 out _ 丢弃。
			if (!slate.TryGet<PawnGenerationRequest>("overridePawnGenParams", out _))
			{
				return;
			}

			// 走到这里，说明这小人确实是仪式招募来的——按设置压低技能。
			ClampSkills(__result);
		}

		/// <summary>
		/// 把小人的所有技能压到设置的上限以内，并按设置决定是否移除技能兴趣。
		/// </summary>
		private static void ClampSkills(Pawn pawn)
		{
			// 人形小人都有技能追踪器（skills），判空只是防御性写法
			if (pawn.skills == null)
			{
				return;
			}

			int cap = CulturalRitualLimitsMod.settings.maxSkillLevel;

			// pawn.skills.skills 是小人全部技能的列表（射击、格斗、建造……每项一条 SkillRecord）
			foreach (SkillRecord skill in pawn.skills.skills)
			{
				// 注意这里用的是 levelInt 而不是 Level 属性：
				//   - levelInt 是技能的"基础等级"（字段本身）；
				//   - Level 属性在读取时会额外加上基因/特性带来的 Aptitude 加成。
				// 我们要限制的是"招进来时的底子"，所以直接操作基础等级。
				if (skill.levelInt > cap)
				{
					skill.levelInt = cap;

					// 等级被压低了，原本积攒的"距下一级的经验"就没意义了。
					// 不清零的话，小人可能随便干点活就连升好几级，限制形同虚设。
					skill.xpSinceLastLevel = 0f;
				}

				// 可选：移除技能兴趣（对所有技能生效，而不只是被压低的技能——
				// 兴趣代表成长速度，想让小人彻底平庸就得一个不留）。
				if (CulturalRitualLimitsMod.settings.removePassions)
				{
					skill.passion = Passion.None;
				}
			}
		}
	}
}
