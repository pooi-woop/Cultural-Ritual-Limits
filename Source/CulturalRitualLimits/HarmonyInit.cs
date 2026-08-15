using HarmonyLib;   // Harmony 补丁库（编译时引用 NuGet 的 Lib.Harmony，运行时用的是游戏自带的 0Harmony.dll）
using Verse;        // RimWorld 核心命名空间（Log、StaticConstructorOnStartup 等）

namespace RITLIM.CulturalRitualLimits
{
	/// <summary>
	/// Harmony 补丁的初始化入口。
	///
	/// 【给新人的说明】
	/// [StaticConstructorOnStartup] 是 RimWorld（Verse 库）提供的特性。
	/// 游戏启动、所有 Mod 的 Def 加载完成之后、主菜单显示之前，
	/// RimWorld 会自动调用所有带这个特性的类的"静态构造函数"。
	/// 这是安装 Harmony 补丁的标准时机——此时游戏的所有方法都已就绪，可以安全挂钩。
	/// </summary>
	[StaticConstructorOnStartup]
	public static class HarmonyInit
	{
		/// <summary>
		/// 静态构造函数：整个游戏进程里只会被执行一次，就是上面说的启动时机。
		/// </summary>
		static HarmonyInit()
		{
			// 创建 Harmony 实例。参数是一个全局唯一的 ID，用来区分是哪个 mod 打的补丁。
			// 社区惯例：和 About.xml 里的 packageId 保持一致。
			Harmony harmony = new Harmony("pooi.culturalrituallimits");

			// PatchAll() 会扫描本 DLL（当前程序集）中所有带 [HarmonyPatch] 特性的类，
			// 把里面定义的 Prefix / Postfix 补丁自动挂到目标方法上。
			harmony.PatchAll();

			// 在游戏的日志里留一条消息，方便排查"补丁到底装没装上"。
			// 游戏里按 ~ 键（需开启开发者模式）或查看 Player.log 可以看到。
			Log.Message("[Cultural Ritual Limits] Harmony 补丁已加载 / Harmony patches applied.");
		}
	}
}
