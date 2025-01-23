// Credit to Scalie for DraggableUIState - https://github.com/ScalarVector1/DragonLens/blob/407a54e45d7a4828f660b46988feaf86092249b3/Content/GUI/DraggableUIState.cs#L10

using Microsoft.Xna.Framework;
using Terraria;

namespace MP3Player.Common.UI.Abstract
{
    public abstract class DraggableUIElement : SmartUIElement
    {
        /// <summary>
        /// The top-left of the main window
        /// </summary>
        private Vector2? basePos;

        private bool dragging;
        private Vector2 dragOff;

        /// <summary>
        /// The area where the user can click and drag to move the main window
        /// </summary>
        public abstract Rectangle DragBox { get; }

        /// <summary>
        /// Where the main window will be placed initially
        /// </summary>
        public abstract Vector2 DefaultPosition { get; }

        public virtual void SafeOnInitialize() { }

        public virtual void DraggableUpdate(GameTime gameTime) { }

        public sealed override void OnInitialize()
        {
            SafeOnInitialize();

            base.OnInitialize();
        }

        public sealed override void SafeUpdate(GameTime gameTime)
        {
            Rectangle size = GetDimensions().ToRectangle();

            basePos ??= new Vector2(DefaultPosition.X * Main.screenWidth, DefaultPosition.Y * Main.screenHeight) - new Vector2(size.Width / 2, size.Height / 2);

            Recalculate();

            if (!Main.mouseLeft && dragging)
            {
                dragging = false;
            }

            if (DragBox.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft || dragging)
            {
                dragging = true;

                if (dragOff == Vector2.Zero)
                {
                    dragOff = Main.MouseScreen - basePos.Value;
                }

                basePos = Main.MouseScreen - dragOff;
            }
            else
            {
                dragOff = Vector2.Zero;
            }

            AdjustPositions(basePos.Value);
            Recalculate();

            if (DragBox.Contains(Main.MouseScreen.ToPoint()))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            // If the box was somehow dragged offscreen reset its position.
            if (!dragging && !DragBox.Intersects(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight)))
            {
                basePos = new Vector2(DefaultPosition.X * Main.screenWidth, DefaultPosition.Y * Main.screenHeight) - new Vector2(size.Width / 2, size.Height / 2);
            }

            DraggableUpdate(gameTime);
        }

        /// <summary>
        /// You should adjust the position of all child elements of your UIState here so they move when the window is being dragged.
        /// </summary>
        /// <param name="newPos">The new position of the base window</param>
        public virtual void AdjustPositions(Vector2 newPos)
        {
            Left.Set(newPos.X, 0);
            Top.Set(newPos.Y, 0);
        }
    }
}
