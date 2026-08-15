using UnityEngine;   // Rect、Mathf 等（Unity 引擎的类，RimWorld 的界面系统建立在 Unity 的 IMGUI 之上）
using Verse;         // Mod、ModContentPack、Listing_Standard 等

namespace RITLIM.CulturalRitualLimits
{
	/// <summary>
	/// Mod 主类——RimWorld 加载本 Mod 时会自动创建它。
	///
	/// 【给新人的说明】
	/// 每个带 C# 代码的 Mod 通常都有一个继承 Verse.Mod 的类。
	/// RimWorld 通过反射找到它并实例化，主要作用有两个：
	///   1. 在构造函数里调用 GetSettings&lt;T&gt;() 拿到设置对象；
	///   2. 提供"Mod 设置界面"（游戏主菜单 → 选项 → Mod 设置里点开看到的就是这里画的内容）。
	/// </summary>
	public class CulturalRitualLimitsMod : Mod
	{
		/// <summary>
		/// 设置对象的静态引用。设成 public static 是为了让补丁代码在任何地方都能
		/// 直接通过 CulturalRitualLimitsMod.settings 读到当前设置。
		/// </summary>
		public static CulturalRitualLimitsSettings settings;

		/// <summary>
		/// 构造函数。参数 content 是本 Mod 的内容包（RimWorld 传入，一般用不着它）。
		/// </summary>
		public CulturalRitualLimitsMod(ModContentPack content) : base(content)
		{
			// GetSettings 会自动读取上次保存的设置文件；第一次运行时没有文件，就用字段默认值。
			settings = GetSettings<CulturalRitualLimitsSettings>();
		}

		/// <summary>
		/// 设置界面里本 Mod 显示的分类标题。
		/// </summary>
		public override string SettingsCategory()
		{
			return "Cultural Ritual Limits";
		}

		/// <summary>
		/// 绘制设置窗口的内容。RimWorld 会在玩家打开本 Mod 的设置页时反复调用（每帧）。
		///
		/// 【给新人的说明】
		/// Listing_Standard 是 RimWorld 提供的"列表式界面排版器"：
		/// 每调用一个方法（Label、Slider、CheckboxLabeled……）就往列表里塞一行控件，
		/// 不用自己算坐标。注意控件的取值方法会在"玩家操作时"返回新值，直接赋回字段即可。
		/// </summary>
		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listing = new Listing_Standard();
			listing.Begin(inRect);

			// —— 技能上限滑条 ——
			listing.Label("技能等级上限 / Max skill level: " + settings.maxSkillLevel);
			listing.Label("（仪式招募的小人，所有技能都不会超过这个等级。调成 20 等于关闭本 mod 的效果。）");
			// Slider 返回 float；技能等级是整数，四舍五入一下。范围 0~20 与游戏内技能范围一致。
			settings.maxSkillLevel = Mathf.RoundToInt(listing.Slider(settings.maxSkillLevel, 0f, 20f));

			listing.Gap();   // 空一行，排版好看一点

			// —— 是否移除兴趣的勾选项 ——
			listing.CheckboxLabeled(
				"同时移除所有技能兴趣 / Also strip all skill passions",
				ref settings.removePassions,
				"勾选后，仪式招募的小人将没有任何技能兴趣（成长潜力归零）。");

			listing.End();

			// 调用基类方法是个好习惯（虽然目前基类没画什么额外内容）。
			base.DoSettingsWindowContents(inRect);
		}
	}
}
