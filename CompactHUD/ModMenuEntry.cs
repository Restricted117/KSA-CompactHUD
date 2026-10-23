/// A copy of MrJeranimo's <c>ModMenuEntryAttribute so ModMenu lists this mod under
/// its shared <c>Mods</c> menu when a player has it installed.
///
/// This is a copy on purpose and it is not a dependency. ModMenu scans every loaded
/// assembly and matches the attribute by GetType().Name alone, never by assembly identity —
/// so declaring it here is enough to be found, and it is inert when ModMenu is absent. Its own
/// README says to copy it rather than reference anything.
/// 
/// Credit to Laurens/LaurensDev for this https://github.com/LaurensDeV/KSArmory

namespace CompactHUD
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class ModMenuEntryAttribute(string menuName, string? isModMenuActivePropertyName = null) : Attribute
    {
        public string MenuName { get; } = menuName;
        public string? IsModMenuActivePropertyName { get; } = isModMenuActivePropertyName;
    }


    internal static class ModMenuPresence
    {
        private static bool? _present;
        public static bool Installed => _present ??= Detect();

        private static bool Detect()
        {
            try
            {
                foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "ModMenu") return true;
                }
            }
            catch { }

            return false;
        }
    }
}
