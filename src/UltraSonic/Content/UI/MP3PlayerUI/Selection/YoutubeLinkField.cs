// Credit to Scalie for TextField - https://github.com/ScalarVector1/DragonLens/blob/master/Content/GUI/FieldEditors/TextField.cs

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MP3Player.Common.UI.Abstract;
using ReLogic.Localization.IME;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;
using Terraria;
using ReLogic.OS;
using MP3Player.Common.UI.Themes;
using MP3Player.Common.UI;
using System;
using System.Runtime.InteropServices;
using MP3Player.Localization;
using Microsoft.Xna.Framework.Input;
using Terraria.Utilities.FileBrowser;
using AngleSharp.Media;
using System.IO;
using MP3Player.Core.UI;

namespace MP3Player.Content.UI.MP3PlayerUI.Selection
{
    public class YoutubeLinkField : SmartUIElement
    {
        public Color BackgroundColor { get; set; } = MP3Player.Cyan;

        public string CurrentValue { get; set; } = "";

        // Composition string is handled at the very beginning of the update.
        // In order to check if there is a composition string before backspace is typed, we need to check the previous state.
        private bool _oldHasCompositionString;

        private bool typing;
        private bool updated;
        private bool reset;

        public void SetTyping()
        {
            typing = true;
            Main.blockInput = true;
        }

        public void SetNotTyping()
        {
            typing = false;
            Main.blockInput = false;
        }

        public override void SafeClick(UIMouseEvent evt)
        {
            SetTyping();
        }

        public override void SafeDoubleClick(UIMouseEvent evt)
        {
            NativeFileDialog dialog = new();

            ExtensionFilter[] filters = new ExtensionFilter[1];
            filters[0] = new ExtensionFilter("MP3 files", "mp3");

            string file = dialog.OpenFilePanel(LocalizationHelper.GetGUIText("AsyncMP3Downloader.FileSelect"), filters);

            // User closed the file dialog without selecting anything.
            if (file == null)
            {
                return;
            }

            Guid uuid = Guid.NewGuid();

            string path = MP3Player.CachePath;

            string originalFileName = Path.GetFileNameWithoutExtension(file);

            string songFile = Path.Combine(path, $"{uuid}.mp3");
            string titleFile = Path.Combine(path, $"{uuid}.txt");

            byte[] bytes = File.ReadAllBytes(file);

            File.WriteAllBytes(songFile, bytes);
            File.WriteAllText(titleFile, $"{originalFileName}{Environment.NewLine}{LocalizationHelper.GetGUIText("AsyncMP3Downloader.AddedByUser")}");

            Main.NewText(LocalizationHelper.GetGUIText("AsyncMP3Downloader.ImportSuccessful", originalFileName), MP3Player.Cyan);

            MP3PlayerMainPanel panel = MP3PlayerUILoader.GetUIState<MP3PlayerState>().MainPanel;

            panel.ChangeWidget(new SongSelectionWidget(panel), true);
        }

        public override void SafeUpdate(GameTime gameTime)
        {
            if (reset)
            {
                updated = false;
                reset = false;
            }

            if (updated)
            {
                reset = true;
            }

            if (Main.mouseLeft && !IsMouseHovering)
            {
                SetNotTyping();
            }
        }

        private void HandleText()
        {
            if (Main.keyState.IsKeyDown(Keys.Escape))
            {
                SetNotTyping();
            }

            PlayerInput.WritingText = true;
            Main.instance.HandleIME();

            string newText = Main.GetInputText(CurrentValue);

            // GetInputText() handles typing operation, but there is a issue that it doesn't handle backspace correctly when the composition string is not empty. It will delete a character both in the text and the composition string instead of only the one in composition string. We'll fix the issue here to provide a better user experience
            if (_oldHasCompositionString && Main.inputText.IsKeyDown(Keys.Back))
            {
                newText = CurrentValue; // force text not to be changed
            }

            if (newText != CurrentValue)
            {
                CurrentValue = newText;
                updated = true;
            }

            _oldHasCompositionString = Platform.Get<IImeService>().CompositionString is { Length: > 0 };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            UIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), BackgroundColor);

            if (typing)
            {
                UIHelper.DrawOutline(spriteBatch, GetDimensions().ToRectangle(), ThemeSystem.ButtonColor.InvertColor());
                HandleText();

                // draw ime panel, note that if there's no composition string then it won't draw anything
                Main.instance.DrawWindowsIMEPanel(GetDimensions().Position());
            }

            RasterizerState state = new()
            {
                ScissorTestEnable = true,
                CullMode = CullMode.None
            };

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, state,
                null, Main.UIScaleMatrix);

            float xScale = Main.UIScaleMatrix.M11;
            float yScale = Main.UIScaleMatrix.M22;

            Rectangle drawBox = GetDimensions().ToRectangle();

            Rectangle rectangle = new(drawBox.X + 8, drawBox.Y, drawBox.Width - 16, drawBox.Height);

            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(
                (int)(rectangle.X * xScale),
                (int)(rectangle.Y * yScale),
                (int)(rectangle.Width * xScale),
                (int)(rectangle.Height * yScale)
            );

            const float scale = 0.85f;

            string displayed = CurrentValue ?? "";

            int stringWidth = (int)(FontAssets.MouseText.Value.MeasureString(displayed).X * scale);

            float positionOffset = Math.Max(stringWidth - (drawBox.Width - 8) + 16, 0);

            Vector2 pos = GetDimensions().Position() + Vector2.One * 8 - new Vector2(positionOffset, 0);

            Utils.DrawBorderString(spriteBatch, displayed, pos, Color.White, scale);

            if (!typing)
            {
                RestartSpriteBatch(spriteBatch);
                return;
            }

            pos.X += stringWidth;

            string compositionString = Platform.Get<IImeService>().CompositionString;

            if (compositionString is { Length: > 0 })
            {
                Utils.DrawBorderString(spriteBatch, compositionString, pos, new Color(255, 240, 20), scale);
                pos.X += FontAssets.MouseText.Value.MeasureString(compositionString).X * scale;
            }

            if (Main.GameUpdateCount % 20 < 10)
                Utils.DrawBorderString(spriteBatch, "|", pos, Color.White, scale);

            RestartSpriteBatch(spriteBatch);

            if (IsMouseHovering)
            {
                Main.instance.MouseText(LocalizationHelper.GetGUIText("MP3PlayerMenu.FileDialogTooltip"));
            }
        }

        private void RestartSpriteBatch(SpriteBatch spriteBatch)
        {
            Main.instance.GraphicsDevice.ScissorRectangle = Main.instance.GraphicsDevice.Viewport.Bounds;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.UIScaleMatrix);
        }
    }
}
