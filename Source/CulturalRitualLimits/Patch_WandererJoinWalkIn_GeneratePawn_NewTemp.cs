using HarmonyLib;          // [HarmonyPatch]、[HarmonyPostfix] 特性
using RimWorld;            // SkillRecord 等
using RimWorld.QuestGen;   // QuestGen、Slate、QuestNode_Root_WandererJoin_WalkIn 所在的命名空间
using Verse;               // Pawn、PawnGenerationRequest、Passion 等

namespace RITLIM.CulturalRitualLimits
{
	/// <summary>
	/// 核心补丁：压低"文化仪式招募"来的小人的技能等级。
	///
	/// 【为什么挂 GeneratePawn_NewTemp 而不是 GeneratePawn（血泪教训）】
	/// Ideology DLC 的招募仪式（Recruitment）判定成功时，由
	/// RitualAttachableOutcomeEffectWorker_RandomRecruit.Apply 触发一个 "WandererJoins"
	/// 任务（Quest），并在任务石板（Slate）里写入 "overridePawnGenParams" 小人生成参数。
	///
	/// 任务真正生成小人时，走的是基类 QuestNode_Root_WandererJoin.RunInt()：
	///     Pawn pawn = GeneratePawn_NewTemp(map);   ← 真正的生成入口（带地图参数）
	/// 而无参的 GeneratePawn() 只是另一个便捷方法（内部转调 GeneratePawn_NewTemp(null)），
	/// 任务链路根本不会调用它！
	///
	/// v1 补丁挂在 GeneratePawn() 上 → 永远不触发 → 技能压不下来。这就是本 mod 之前"不生效"的根因。
	/// 本补丁改挂实际入口 GeneratePawn_NewTemp。
	///
	/// 【为什么用 Slate 里的 "overridePawnGenParams" 键做判断】
	/// 原版代码里只有招募仪式（RandomRecruit）写入这个键（已通过反编译 1.6 源码逐条核实），
	/// 其它来源触发 WandererJoins 任务时不会写。所以检查它就能精确锁定"仪式招募的小人"。
	/// 随机"流浪者加入"事件（IncidentWorker_WandererJoin）根本不走任务系统，
	/// 而是自己直接生成小人，所以天然不受本补丁影响（符合设计意图）。
	/// </summary>
	[HarmonyPatch(typeof(QuestNode_Root_WandererJoin_WalkIn), nameof(QuestNode_Root_WandererJoin_WalkIn.GeneratePawn_NewTemp))]
	public static class Patch_WandererJoinWalkIn_GeneratePawn_NewTemp
	{
		/// <summary>
		/// Postfix（后置补丁）：在原方法执行完毕后运行。
		///
		/// Harmony 特殊参数说明：
		///   __result —— 原方法的返回值，这里就是刚生成好的小人（Pawn）。
		///               用 __result 接收即可读取；本补丁不需要替换返回值，所以不加 ref。
		///
		/// 原方法和本 Postfix 在同一调用栈里连续执行，QuestGen.slate 在两者之间不会被改动，
		/// 所以在 Postfix 里读石板、判定 "overridePawnGenParams" 是可靠、准确的。
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
			// 保险起见还是判一下空。
			Slate slate = QuestGen.slate;
			if (slate == null)
			{
				return;
			}

			// 关键判断：Slate 里存在 "overridePawnGenParams" ⇔ 这个小人是文化仪式招募来的。
			// TryGet 的第二个参数是 out 输出，我们只需要知道"是否存在"，所以用 out _ 丢弃。
			if (!slate.TryGet<PawnGenerationRequest>("overridePawnGenParams", out _))
			{
				return;
			}

			// 走到这里，说明这小人确实是仪式招募来的——按设置压低技能。
			ClampSkills(__result);

			// 留一句日志，方便确认补丁真的生效了（游戏里 ~ 键开发者控制台 / Player.log）。
			Log.Message("[Cultural Ritual Limits] 仪式招募的小人技能已压低（上限 " + CulturalRitualLimitsMod.settings.maxSkillLevel + "）");
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