using System.Drawing;
using Pastel;

namespace VideoConverter.Cmd;

internal static class ColorWriter
{
    public static readonly Color WelcomeColor = Color.LightSeaGreen;
    public static readonly Color ActionPromptColor = Color.LightSeaGreen;
    public static readonly Color ValuePromptColor = Color.Green;
    public static readonly Color InfoColor = Color.Yellow;
    public static readonly Color InputErrorColor = Color.OrangeRed;
    public static readonly Color ConvertingColor = Color.Green;

    public static void WriteWelcome(string text) => Console.WriteLine(text.Pastel(WelcomeColor));

    public static void WriteInfo(string text) => Console.WriteLine($"ℹ️ {text}".Pastel(InfoColor));

    public static void WriteActionPrompt(string text) => Console.WriteLine($"📋 {text}".Pastel(ActionPromptColor));

    public static void WriteValuePrompt(string text) => Console.WriteLine($"➡️ {text}".Pastel(ValuePromptColor));

    public static void WriteInputError(string text) => Console.WriteLine($"❗ {text}".Pastel(InputErrorColor));

    public static void WriteConversion(string text) => Console.WriteLine($"🥁 {text}".Pastel(ConvertingColor));
}
