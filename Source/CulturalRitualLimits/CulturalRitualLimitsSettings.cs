using Verse;   // ModSettings、Scribe_Values 所在的核心命名空间

namespace RITLIM.CulturalRitualLimits
{
	/// <summary>
	/// Mod 设置数据。
	///
	/// 【给新人的说明】
	/// 继承 Verse.ModSettings 之后，RimWorld 会负责把这个类的字段
	/// 保存到 Mod 配置文件（Windows 下在 AppData\...\Ludeon Studios\RimWorld by Ludeon Studios\Config），
	/// 属于"全局设置"，和具体存档无关——所有存档共用同一份设置。
	/// </summary>
	public class CulturalRitualLimitsSettings : ModSettings
	{
		/// <summary>
		/// 技能等级上限：仪式招募的小人，每项技能的等级都不会超过这个值。
		/// 默认 3——也就是"最高技能点为 3 的臭鱼烂虾"。
		/// 游戏内技能本身的范围是 0~20，所以这个设置也限制在 0~20。
		/// </summary>
		public int maxSkillLevel = 3;

		/// <summary>
		/// 是否移除仪式招募的小人的所有技能兴趣（Passion）。
		/// 默认关闭。开启后招募的小人没有任何技能兴趣，成长速度垫底，臭鱼烂虾得更彻底。
		/// </summary>
		public bool removePassions = false;

		/// <summary>
		/// 设置的"存档读写"方法。
		///
		/// RimWorld 的 Scribe 序列化系统会在保存设置时调用本方法把字段写出去，
		/// 读取设置时调用本方法把值读回来。每个需要保存的字段都必须在这里登记一次：
		///   - 第一个参数：字段本身（ref 传引用，读写两用）
		///   - 第二个参数：配置文件里的 key 字符串——定下来以后不要再改，改了老用户的设置会丢失
		///   - 第三个参数：默认值——必须和上面字段的初始值保持一致
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref maxSkillLevel, "maxSkillLevel", 3);
			Scribe_Values.Look(ref removePassions, "removePassions", false);
		}
	}
}
