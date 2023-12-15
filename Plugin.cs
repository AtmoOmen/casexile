using Dalamud.IoC;
using Dalamud.Plugin;

namespace Casexile
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Casexile";

        private SlashCommandInterceptor? _slashCommandInterceptor { get; init; }

        public Plugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            _slashCommandInterceptor = pluginInterface.Create<SlashCommandInterceptor>();
        }

        public void Dispose()
        {
            _slashCommandInterceptor?.Dispose();
        }
    }
}
