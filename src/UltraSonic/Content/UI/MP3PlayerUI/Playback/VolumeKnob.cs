using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MP3Player.Common.UI.Abstract;
using MP3Player.Common.UI.Themes;
using System;
using Terraria;
using Terraria.UI;

namespace MP3Player.Content.UI.MP3PlayerUI.Playback
{
    public class VolumeKnob : SmartUIElement
    {
        private const float MinAngle = MathHelper.Pi + MathHelper.PiOver4;

        private const float MaxAngle = -MathHelper.PiOver4;

        private float angle;

        private bool dragging;

        private float startOffset;

        private float startAngle;

        private float CursorOffsetFromCenterX => Main.MouseScreen.X - (GetDimensions().Position().X + GetDimensions().Width / 2);

        public float Volume => (float)Math.Abs(InverseLerp(MinAngle, MaxAngle, angle));

        public VolumeKnob()
        {
            // Start at 50% volume.
            angle = MathHelper.PiOver2;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D volumeKnob = ThemeSystem.GetIcon("VolumeKnob");
            Texture2D volumeIndicator = ThemeSystem.GetIcon("VolumeIndicator");

            Vector2 knobPosition = GetDimensions().Position();

            Vector2 knobCenter = knobPosition + new Vector2(64, 60);
            Vector2 knobOrigin = new(volumeIndicator.Width / 2, volumeIndicator.Height / 2);
            Vector2 knobOffset = Vector2.UnitX.RotatedBy(-angle) * 26;

            spriteBatch.Draw(volumeKnob, knobPosition, Color.White);
            spriteBatch.Draw(volumeIndicator, knobOffset + knobCenter, null, Color.White, knobOffset.ToRotation(), knobOrigin, 1, SpriteEffects.None, 0);

            base.Draw(spriteBatch);
        }

        public override void SafeUpdate(GameTime gameTime)
        {
            if (!Main.mouseLeft && dragging)
            {
                dragging = false;
                startOffset = 0;
            }

            if (dragging)
            {
                float currentOffset = CursorOffsetFromCenterX - startOffset;

                float radiansOffset = -MathHelper.ToRadians(currentOffset / 1.5f);

                angle = startAngle + radiansOffset;
            }

            angle = MathHelper.Clamp(angle, MaxAngle, MinAngle);

            base.SafeUpdate(gameTime);
        }

        public override void SafeMouseDown(UIMouseEvent evt)
        {
            Rectangle drawBox = GetDimensions().ToRectangle();

            // Hitbox of the knob itself.
            Rectangle smallerBox = new(drawBox.X + 30, drawBox.Y + 26, 68, 72);

            if (smallerBox.Contains((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y))
            {
                dragging = true;

                startOffset = CursorOffsetFromCenterX;
                startAngle = angle;
            }
        }

        private float InverseLerp(float a, float b, float value) => (value - a) / (b - a);
    }
}
