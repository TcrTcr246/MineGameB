using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineGameB.Items;

public class InventorySlot {
    public Texture2D ItemTexture { get; set; }
    public Rectangle SourceRect { get; set; }
    public string ItemName { get; set; }
    public int Count { get; set; }
    public int MaxStack { get; set; }

    public InventorySlot() {
        MaxStack = 99;
        Count = 0;
        SourceRect = Rectangle.Empty;
    }

    public bool IsEmpty => ItemTexture == null || Count == 0;

    public void Clear() {
        ItemTexture = null;
        ItemName = null;
        Count = 0;
        SourceRect = Rectangle.Empty;
    }

    public void CopyFrom(InventorySlot other) {
        ItemTexture = other.ItemTexture;
        SourceRect = other.SourceRect;
        ItemName = other.ItemName;
        Count = other.Count;
        MaxStack = other.MaxStack;
    }
}

public class Inventory {
    private const int HOTBAR_SLOTS = 5;
    private const int TOTAL_ROWS = 4;
    private const int SLOTS_PER_ROW = 5;
    private const int TOTAL_SLOTS = HOTBAR_SLOTS + (TOTAL_ROWS - 1) * SLOTS_PER_ROW;

    private const int SLOT_SIZE = 56;
    private const int SLOT_PADDING = 2;
    private const int HOTBAR_Y_OFFSET = 20;

    private InventorySlot[] slots;
    private Texture2D inventoryTexture;
    private SpriteFont font;

    // Source rectangles for the inventory sprite sheet
    private Rectangle slotRect;
    private Rectangle selectedSlotRect;
    private Rectangle backgroundRect;

    private bool isFullInventoryOpen = false;
    private int selectedHotbarSlot = 0;

    private KeyboardState prevKeyState;
    private KeyboardState currKeyState;
    private MouseState prevMouseState;
    private MouseState currMouseState;

    private int screenWidth;
    private int screenHeight;

    // Drag and drop
    private InventorySlot draggedSlot = null;
    private int draggedFromIndex = -1;

    public Inventory(GraphicsDevice graphicsDevice, SpriteFont spriteFont, Texture2D inventoryTex) {
        slots = new InventorySlot[TOTAL_SLOTS];
        for (int i = 0; i < TOTAL_SLOTS; i++) {
            slots[i] = new InventorySlot();
        }
        prevKeyState = Keyboard.GetState();
        prevMouseState = Mouse.GetState();
        LoadContent(graphicsDevice, spriteFont, inventoryTex);
    }

    public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont spriteFont, Texture2D inventoryTex) {
        font = spriteFont;
        inventoryTexture = inventoryTex;

        // Define source rectangles from the inventory sprite sheet
        slotRect = new Rectangle(0, 0, 32, 32);
        selectedSlotRect = new Rectangle(32, 0, 32, 32);
        backgroundRect = new Rectangle(0, 32, 32, 32);

        screenWidth = Game1.Instance.Camera.Screen.Width;
        screenHeight = Game1.Instance.Camera.Screen.Height;
    }

    public void Update(GameTime gameTime) {
        currKeyState = Keyboard.GetState();
        currMouseState = Mouse.GetState();

        // Toggle full inventory with Tab
        if (currKeyState.IsKeyDown(Keys.Tab) && prevKeyState.IsKeyUp(Keys.Tab)) {
            isFullInventoryOpen = !isFullInventoryOpen;

            // Drop any dragged item back if inventory closes
            if (!isFullInventoryOpen && draggedSlot != null) {
                if (draggedFromIndex >= 0) {
                    slots[draggedFromIndex].CopyFrom(draggedSlot);
                }
                draggedSlot = null;
                draggedFromIndex = -1;
            }
        }

        // Handle inventory interactions
        if (isFullInventoryOpen) {
            HandleInventoryMouseInput();
        } else {
            HandleHotbarMouseInput();
        }

        prevKeyState = currKeyState;
        prevMouseState = currMouseState;
    }

    private void HandleHotbarMouseInput() {
        int totalWidth = HOTBAR_SLOTS * SLOT_SIZE + (HOTBAR_SLOTS - 1) * SLOT_PADDING;
        int startX = (screenWidth - totalWidth) / 2;
        int startY = screenHeight - SLOT_SIZE - HOTBAR_Y_OFFSET;

        Vector2 mousePos = Game1.Instance.Camera.MouseScreen;  // Changed to MouseScreen

        // Check if clicking on hotbar slots
        if (currMouseState.LeftButton == ButtonState.Pressed &&
            prevMouseState.LeftButton == ButtonState.Released) {

            for (int i = 0; i < HOTBAR_SLOTS; i++) {
                int x = startX + i * (SLOT_SIZE + SLOT_PADDING);
                int y = startY;
                Rectangle slotBounds = new Rectangle(x, y, SLOT_SIZE, SLOT_SIZE);

                if (slotBounds.Contains(mousePos)) {
                    selectedHotbarSlot = i;
                    break;
                }
            }
        }
    }

    private void HandleInventoryMouseInput() {
        int totalWidth = SLOTS_PER_ROW * SLOT_SIZE + (SLOTS_PER_ROW - 1) * SLOT_PADDING;
        int totalHeight = TOTAL_ROWS * SLOT_SIZE + (TOTAL_ROWS - 1) * SLOT_PADDING;
        int startX = (screenWidth - totalWidth) / 2;
        int startY = (screenHeight - totalHeight) / 2;

        Vector2 mousePos = Game1.Instance.Camera.MouseScreen;  // Changed to MouseScreen

        // Start dragging
        if (currMouseState.LeftButton == ButtonState.Pressed &&
            prevMouseState.LeftButton == ButtonState.Released &&
            draggedSlot == null) {

            for (int row = 0; row < TOTAL_ROWS; row++) {
                for (int col = 0; col < SLOTS_PER_ROW; col++) {
                    int slotIndex = row * SLOTS_PER_ROW + col;
                    int x = startX + col * (SLOT_SIZE + SLOT_PADDING);
                    int y = startY + row * (SLOT_SIZE + SLOT_PADDING);
                    Rectangle slotBounds = new Rectangle(x, y, SLOT_SIZE, SLOT_SIZE);

                    if (slotBounds.Contains(mousePos) && !slots[slotIndex].IsEmpty) {
                        // Pick up item
                        draggedSlot = new InventorySlot();
                        draggedSlot.CopyFrom(slots[slotIndex]);
                        draggedFromIndex = slotIndex;
                        slots[slotIndex].Clear();
                        break;
                    }
                }
            }
        }

        // Drop item
        if (currMouseState.LeftButton == ButtonState.Released &&
            prevMouseState.LeftButton == ButtonState.Pressed &&
            draggedSlot != null) {

            bool dropped = false;

            for (int row = 0; row < TOTAL_ROWS; row++) {
                for (int col = 0; col < SLOTS_PER_ROW; col++) {
                    int slotIndex = row * SLOTS_PER_ROW + col;
                    int x = startX + col * (SLOT_SIZE + SLOT_PADDING);
                    int y = startY + row * (SLOT_SIZE + SLOT_PADDING);
                    Rectangle slotBounds = new Rectangle(x, y, SLOT_SIZE, SLOT_SIZE);

                    if (slotBounds.Contains(mousePos)) {
                        // Try to stack with existing item
                        if (!slots[slotIndex].IsEmpty &&
                            slots[slotIndex].ItemName == draggedSlot.ItemName &&
                            slots[slotIndex].Count < slots[slotIndex].MaxStack) {

                            int addAmount = Math.Min(draggedSlot.Count, slots[slotIndex].MaxStack - slots[slotIndex].Count);
                            slots[slotIndex].Count += addAmount;
                            draggedSlot.Count -= addAmount;

                            if (draggedSlot.Count <= 0) {
                                draggedSlot = null;
                                draggedFromIndex = -1;
                            } else {
                                // Still have items left, put back in original slot
                                slots[draggedFromIndex].CopyFrom(draggedSlot);
                                draggedSlot = null;
                                draggedFromIndex = -1;
                            }
                        } else {
                            // Swap items
                            InventorySlot temp = new InventorySlot();
                            temp.CopyFrom(slots[slotIndex]);
                            slots[slotIndex].CopyFrom(draggedSlot);

                            if (!temp.IsEmpty) {
                                slots[draggedFromIndex].CopyFrom(temp);
                            }

                            draggedSlot = null;
                            draggedFromIndex = -1;
                        }

                        dropped = true;
                        break;
                    }
                }
                if (dropped)
                    break;
            }

            // If not dropped on a slot, return to original position
            if (!dropped && draggedSlot != null) {
                slots[draggedFromIndex].CopyFrom(draggedSlot);
                draggedSlot = null;
                draggedFromIndex = -1;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        if (isFullInventoryOpen) {
            DrawFullInventory(spriteBatch);
        } else {
            DrawHotbar(spriteBatch);
        }

        // Draw dragged item following mouse
        if (draggedSlot != null && isFullInventoryOpen) {
            Vector2 mousePos = Game1.Instance.Camera.MouseScreen;
            DrawItemInSlot(spriteBatch, draggedSlot, (int)mousePos.X - SLOT_SIZE / 2, (int)mousePos.Y - SLOT_SIZE / 2);
        }
    }

    private void DrawHotbar(SpriteBatch sb) {
        int totalWidth = HOTBAR_SLOTS * SLOT_SIZE + (HOTBAR_SLOTS - 1) * SLOT_PADDING;
        int startX = (screenWidth - totalWidth) / 2;
        int startY = screenHeight - SLOT_SIZE - HOTBAR_Y_OFFSET;

        for (int i = 0; i < HOTBAR_SLOTS; i++) {
            int x = startX + i * (SLOT_SIZE + SLOT_PADDING);
            int y = startY;

            // Draw slot from sprite sheet
            Rectangle srcRect = (i == selectedHotbarSlot) ? selectedSlotRect : slotRect;
            sb.Draw(inventoryTexture, new Rectangle(x, y, SLOT_SIZE, SLOT_SIZE), srcRect, Color.White);

            // Draw item
            if (!slots[i].IsEmpty) {
                DrawItemInSlot(sb, slots[i], x, y);
            }
        }
    }

    private void DrawFullInventory(SpriteBatch sb) {
        int totalWidth = SLOTS_PER_ROW * SLOT_SIZE + (SLOTS_PER_ROW - 1) * SLOT_PADDING;
        int totalHeight = TOTAL_ROWS * SLOT_SIZE + (TOTAL_ROWS - 1) * SLOT_PADDING;
        int startX = (screenWidth - totalWidth) / 2;
        int startY = (screenHeight - totalHeight) / 2;

        // Draw background panel (tiled)
        DrawTiledBackground(sb, startX - 20, startY - 20, totalWidth + 40, totalHeight + 40);

        // Draw all slots
        for (int row = 0; row < TOTAL_ROWS; row++) {
            for (int col = 0; col < SLOTS_PER_ROW; col++) {
                int slotIndex = row * SLOTS_PER_ROW + col;
                int x = startX + col * (SLOT_SIZE + SLOT_PADDING);
                int y = startY + row * (SLOT_SIZE + SLOT_PADDING);

                // Draw slot from sprite sheet
                Rectangle srcRect = (row == 0 && col == selectedHotbarSlot) ? selectedSlotRect : slotRect;
                sb.Draw(inventoryTexture, new Rectangle(x, y, SLOT_SIZE, SLOT_SIZE), srcRect, Color.White);

                // Draw item (skip if being dragged)
                if (!slots[slotIndex].IsEmpty && slotIndex != draggedFromIndex) {
                    DrawItemInSlot(sb, slots[slotIndex], x, y);
                }
            }
        }
    }

    private void DrawTiledBackground(SpriteBatch sb, int x, int y, int width, int height) {
        int tileSize = backgroundRect.Width;

        for (int ty = y; ty < y + height; ty += tileSize) {
            for (int tx = x; tx < x + width; tx += tileSize) {
                int drawWidth = Math.Min(tileSize, x + width - tx);
                int drawHeight = Math.Min(tileSize, y + height - ty);

                sb.Draw(inventoryTexture,
                    new Rectangle(tx, ty, drawWidth, drawHeight),
                    new Rectangle(backgroundRect.X, backgroundRect.Y, drawWidth, drawHeight),
                    new Color(255, 255, 255, 230));
            }
        }
    }

    private void DrawItemInSlot(SpriteBatch sb, InventorySlot slot, int x, int y) {
        if (slot.ItemTexture != null) {
            // Draw item texture centered in slot
            int itemSize = SLOT_SIZE - 6;
            sb.Draw(slot.ItemTexture,
                new Rectangle(x + 3, y + 3, itemSize, itemSize),
                slot.SourceRect,
                Color.White);

            // Draw item count
            if (slot.Count > 1 && font != null) {
                string countText = slot.Count.ToString();
                Vector2 textSize = font.MeasureString(countText);
                Vector2 textPos = new Vector2(x + SLOT_SIZE - textSize.X - 2, y + SLOT_SIZE - textSize.Y - 1);

                // Draw shadow
                sb.DrawString(font, countText, textPos + new Vector2(1, 1), Color.Black);
                sb.DrawString(font, countText, textPos, Color.White);
            }
        }
    }

    // Public methods for managing inventory
    public bool AddItem(Texture2D texture, Rectangle source, string name, int count = 1) {
        // Try to stack with existing items
        for (int i = 0; i < TOTAL_SLOTS; i++) {
            if (!slots[i].IsEmpty && slots[i].ItemName == name && slots[i].Count < slots[i].MaxStack) {
                int addAmount = Math.Min(count, slots[i].MaxStack - slots[i].Count);
                slots[i].Count += addAmount;
                count -= addAmount;

                if (count <= 0)
                    return true;
            }
        }

        // Add to empty slots
        for (int i = 0; i < TOTAL_SLOTS; i++) {
            if (slots[i].IsEmpty) {
                slots[i].ItemTexture = texture;
                slots[i].SourceRect = source;
                slots[i].ItemName = name;
                slots[i].Count = Math.Min(count, slots[i].MaxStack);
                count -= slots[i].Count;

                if (count <= 0)
                    return true;
            }
        }

        return count <= 0;
    }

    public InventorySlot GetSelectedHotbarSlot() {
        return slots[selectedHotbarSlot];
    }

    public InventorySlot GetSlot(int loc) {
        if (loc < 0 || loc >= TOTAL_SLOTS)
            throw new ArgumentOutOfRangeException(nameof(loc), "Invalid inventory slot index.");
        return slots[loc];
    }
}