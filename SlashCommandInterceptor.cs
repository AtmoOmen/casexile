using Dalamud.Game.Gui;
using Dalamud.IoC;
using System;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using XivCommon;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState;

namespace Casexile
{
    public class SlashCommandInterceptor
    {
        [PluginService, RequiredVersion("1.0")]
        public static ChatGui Chat { get; private set; } = null!;
        [PluginService, RequiredVersion("1.0")]
        public static ClientState Client { get; private set; } = null!;

        private DateTime lastChatTime = DateTime.MinValue;
        private XivCommonBase xcb;
        private Regex regex;

        public SlashCommandInterceptor()
        {
            regex = new Regex(Client.ClientLanguage switch
            {
                Dalamud.ClientLanguage.English => "The command (\\/.+) does not exist.",
                Dalamud.ClientLanguage.French => "La commande texte “(\\/.+)” n'existe pas.",
                Dalamud.ClientLanguage.German => "„(\\/.+)“ existiert nicht als Textkommando.",
                Dalamud.ClientLanguage.Japanese => "そのコマンドはありません。： (\\/.+)",
                Dalamud.ClientLanguage.ChineseSimplified => "“(\\/.+)”出现问题：该命令不存在。",
                _ => "The command (\\/.+) does not exist."
            });
            xcb = new XivCommonBase();
            Chat.ChatMessage += OnChatMessage;
        }

        public void Dispose()
        {
            Chat.ChatMessage -= OnChatMessage;
            xcb.Dispose();
        }

        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (type == XivChatType.ErrorMessage && message?.Payloads?.Count == 1)
            {
                if (message.Payloads[0] is TextPayload text && !string.IsNullOrEmpty(text.Text))
                {
                    isHandled = HandleSlashCommand(text);
                }
            }
        }

        private bool HandleSlashCommand(TextPayload text)
        {
            var match = regex.Match(text.Text!);

            if (match?.Success == true && match.Groups.Count == 2)
            {
                var capture = match.Groups[1].ValueSpan;

                if (IsValid(capture))
                {
                    var end = capture.IndexOf(' ');

                    if (end == -1)
                    {
                        Span<char> lower = stackalloc char[capture.Length];
                        capture.ToLower(lower, null);
                        xcb.Functions.Chat.SendMessage(lower.ToString());
                    }
                    else
                    {
                        var command = capture.Slice(0, end);
                        var argument = capture.Slice(end);
                        Span<char> lower = stackalloc char[end];
                        command.ToLower(lower, null);
                        xcb.Functions.Chat.SendMessage($"{lower}{argument}");
                    }

                    lastChatTime = DateTime.Now;
                    return true;
                }
            }

            return false;
        }

        private bool IsValid(ReadOnlySpan<char> chars)
        {
            if ((DateTime.Now - lastChatTime).TotalSeconds < 0.2f)
            {
                return false;
            }

            foreach (var @char in chars)
            {
                if (char.IsUpper(@char))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
