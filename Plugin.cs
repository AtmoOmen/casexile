using Dalamud.Plugin;

namespace Casexile;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Casexile";

    private SlashCommandInterceptor _slashCommandInterceptor { get; init; }

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        _slashCommandInterceptor = pluginInterface.Create<SlashCommandInterceptor>(pluginInterface)!;
    }

    public void Dispose()
    {
        _slashCommandInterceptor?.Dispose();
    }
}
