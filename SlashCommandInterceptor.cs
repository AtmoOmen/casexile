using System;
using System.Text.RegularExpressions;
using Dalamud;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using XivCommon;

namespace Casexile;

public class SlashCommandInterceptor
{
    [PluginService]
    public static IChatGui Chat { get; private set; } = null!;

    [PluginService]
    public static IClientState Client { get; private set; } = null!;

    private DateTime _lastChatTime = DateTime.MinValue;
    private readonly XivCommonBase _xcb;
    private readonly Regex _regex;

    public SlashCommandInterceptor(DalamudPluginInterface pluginInterface)
    {
        var pattern = Client.ClientLanguage switch
        {
            (ClientLanguage)1 => "The command (\\/.+) does not exist.",         // 英语
            (ClientLanguage)3 => "La commande texte “(\\/.+)” n'existe pas.",   // 法语
            (ClientLanguage)2 => "„(\\/.+)“ existiert nicht als Textkommando.", // 德语
            0 => "そのコマンドはありません。： (\\/.+)",                                      // 日语
            (ClientLanguage)4 => "“(\\/.+)”出现问题：该命令不存在。"                        // 简体中文
        };

        _regex = new Regex(pattern);
        _xcb = new XivCommonBase(pluginInterface);
        Chat.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        Chat.ChatMessage -= OnChatMessage;
        _xcb.Dispose();
    }

    private void OnChatMessage(
        XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (type == XivChatType.ErrorMessage && message.Payloads.Count == 1)
        {
            var value = message.Payloads[0];
            if (value is TextPayload text && !string.IsNullOrEmpty(text.Text))
                isHandled = HandleSlashCommand(text);
        }
    }

    private bool HandleSlashCommand(ITextProvider text)
    {
        var match = _regex.Match(text.Text!);

        if (match is { Success: true, Groups.Count: 2 })
        {
            var capture = match.Groups[1].ValueSpan;

            if (IsValid(capture))
            {
                var end = capture.IndexOf(' ');

                if (end == -1)
                {
                    Span<char> lower = stackalloc char[capture.Length];
                    capture.ToLower(lower, null);
                    _xcb.Functions.Chat.SendMessage(lower.ToString());
                }
                else
                {
                    var command = capture.Slice(0, end);
                    var argument = capture.Slice(end);
                    Span<char> lower = stackalloc char[end];
                    command.ToLower(lower, null);
                    _xcb.Functions.Chat.SendMessage($"{lower}{argument}");
                }

                _lastChatTime = DateTime.Now;
                return true;
            }
        }

        return false;
    }

    private bool IsValid(ReadOnlySpan<char> chars)
    {
        if ((DateTime.Now - _lastChatTime).TotalSeconds < 1f) return false;

        foreach (var @char in chars)
            if (char.IsUpper(@char))
                return true;

        return false;
    }
}
