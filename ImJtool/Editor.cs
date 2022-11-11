using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ImJtool
{
    /// <summary>
    /// Control map editing. 
    /// Including placing, deleting map objects, undoing and redoing, etc.
    /// </summary>
    public class Editor
    {
        public static Editor Instance => Jtool.Instance.Editor;
        public bool MouseInTitle { get; set; }
        public Type SelectType { get; set; } = typeof(Block);
        public int Snap { get; set; } = 32;
        public bool ShowGrid { get; set; } = true;
        public RenderTarget2D GridTexture { get; set; }
        public int GridSize { get; set; } = 32;

        MapObject handedObject;
        Vector2 handedOldPos;

        bool leftHoldLast = false;
        bool rightHoldLast = false;
        Vector2 mouseLastPos;

        public bool NeedDrawPreview { get; private set; } = false;
        public Sprite PreviewSprite { get; private set; }
        public Vector2 PreviewPosition { get; private set; }

        Stack<UndoEvent> undoStack = new();
        Stack<UndoEvent> redoStack = new();
        UndoEvent curEvent;
        public void RedrawGrid()
        {
            var gd = Jtool.Instance.GraphicsDevice;
            var sb = Jtool.Instance.SpriteBatch;

            var oldtarget = gd.GetRenderTargets();
            var tex = ResourceManager.Instance.GetTexture("white_pixel");

            var col = Color.White;
            gd.SetRenderTargets(GridTexture);
            gd.Clear(Color.Transparent);

            sb.Begin();
            for (int xx = 0; xx < 800; xx += GridSize)
            {
                sb.Draw(tex, new Rectangle(xx, 0, 1, 608), col);
            }
            for (int yy = 0; yy < 608; yy+=GridSize)
            {
                sb.Draw(tex, new Rectangle(0, yy, 800, 1), col);
            }
            sb.End();

            gd.SetRenderTargets(oldtarget);
        }
        public void Update()
        {
            var mousePos = ImGui.GetMousePos(); // Mouse position in window
            var windowPos = ImGui.GetWindowPos(); // Map window position
            var windowSize = ImGui.GetWindowSize();
            var contentStartPos = windowPos + new Vector2(0, Gui.TitleBarHeight);

            MouseInTitle = new Rectangle((int)windowPos.X, (int)windowPos.Y, (int)windowSize.X, (int)Gui.TitleBarHeight).Contains(mousePos.X, mousePos.Y);
            var mouseInPos = (mousePos - contentStartPos) / Gui.Instance.MapWindowScale;
            var cursorInArea = new Rectangle(0, 0, 799, 607).Contains(mouseInPos);

            var leftPress = ImGui.IsMouseClicked(ImGuiMouseButton.Left);
            var leftHold = ImGui.IsMouseDown(ImGuiMouseButton.Left);
            var leftRelease = ImGui.IsMouseReleased(ImGuiMouseButton.Left);

            var rightPress = ImGui.IsMouseClicked(ImGuiMouseButton.Right);
            var rightHold = ImGui.IsMouseDown(ImGuiMouseButton.Right);
            var rightRelease = ImGui.IsMouseReleased(ImGuiMouseButton.Right);

            var ks = Keyboard.GetState();
            var dragHold = ks.IsKeyDown(Keys.Space);
            var pickerHold = ks.IsKeyDown(Keys.LeftControl);

            var snappedPos = new Vector2(MathF.Floor(mouseInPos.X / Snap) * Snap, MathF.Floor(mouseInPos.Y / Snap) * Snap);
            var focused = ImGui.IsWindowFocused();

            ImGui.SetMouseCursor(ImGuiMouseCursor.Arrow);
            if (cursorInArea)
            {
                if (dragHold && focused)
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if (leftHold)
                    {
                        if (handedObject == null)
                        {
                            var col = MapObjectManager.Instance.CollisionPoint(mouseInPos.X, mouseInPos.Y);
                            if (col != null)
                            {
                                handedObject = col;
                                handedOldPos = new Vector2(col.X, col.Y);
                            }
                        }
                        else
                        {
                            handedObject.X = snappedPos.X;
                            handedObject.Y = snappedPos.Y;
                        }
                    }
                    if (leftRelease)
                    {
                        FinishMoveObject();
                    }
                }
                else if (pickerHold && focused)
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Arrow);
                    if (leftPress)
                    {
                        var col = MapObjectManager.Instance.CollisionPointList(mouseInPos.X, mouseInPos.Y);
                        foreach (var i in col)
                        {
                            SelectType = i.GetType();
                        }
                    }
                }
                else
                {
                    if (leftHold && !ImGui.IsPopupOpen(null, ImGuiPopupFlags.AnyPopupId))
                    {
                        if (leftPress)
                        {
                            FinishCreateObject(snappedPos.X, snappedPos.Y);
                        }
                        if (leftHoldLast && mouseInPos != mouseLastPos)
                        {
                            // Call "FinishCreateObject" for each pixel in a line
                            // to prevent the mouse from moving too fast
                            float k, b;

                            if (MathF.Abs(mouseInPos.Y - mouseLastPos.Y) < MathF.Abs(mouseInPos.X - mouseLastPos.X))
                            {
                                // Δy<Δx, do y=kx+b check
                                k = (mouseInPos.Y - mouseLastPos.Y) / (mouseInPos.X - mouseLastPos.X);
                                b = mouseLastPos.Y - k * mouseLastPos.X;

                                var xmin = MathF.Min(mouseInPos.X, mouseLastPos.X);
                                var xmax = MathF.Max(mouseInPos.X, mouseLastPos.X);

                                for (var xx = xmin; xx <= xmax; xx++)
                                {
                                    var yy = k * xx + b;
                                    var xxn = MathF.Floor(xx / Snap) * Snap;
                                    var yyn = MathF.Floor(yy / Snap) * Snap;
                                    FinishCreateObject(xxn, yyn);
                                }
                            }
                            else
                            {
                                // Δy≥Δx, do x=ky+b check
                                k = (mouseInPos.X - mouseLastPos.X) / (mouseInPos.Y - mouseLastPos.Y);
                                b = mouseLastPos.X - k * mouseLastPos.Y;

                                var ymin = MathF.Min(mouseInPos.Y, mouseLastPos.Y);
                                var ymax = MathF.Max(mouseInPos.Y, mouseLastPos.Y);

                                for (var yy = ymin; yy <= ymax; yy++)
                                {
                                    var xx = k * yy + b;
                                    var xxn = MathF.Floor(xx / Snap) * Snap;
                                    var yyn = MathF.Floor(yy / Snap) * Snap;
                                    FinishCreateObject(xxn, yyn);
                                }
                            }
                        }
                    }
                    else if (rightHold)
                    {
                        List<MapObject> col;
                        if (rightHoldLast)
                        {
                            col = MapObjectManager.Instance.CollisionLineList(mouseLastPos.X, mouseLastPos.Y, mouseInPos.X, mouseInPos.Y);
                        }
                        else
                        {
                            col = MapObjectManager.Instance.CollisionPointList(mouseInPos.X, mouseInPos.Y);
                        }
                        foreach (var i in col)
                        {
                            if (i.GetType() == typeof(PlayerStart) || !i.IsInPalette)
                            {
                                continue;
                            }
                            AddRemoveEvent(i.X, i.Y, i.GetType());
                            i.Destroy();
                        }
                    }
                }
                if (!dragHold)
                {
                    FinishMoveObject();
                }
            }

            if (leftRelease || rightRelease)
            {
                FinishEvent();
            }

            mouseLastPos = mouseInPos;
            leftHoldLast = leftHold;
            rightHoldLast = rightHold;

            if (cursorInArea && !dragHold && !pickerHold)
            {
                // Preview rendering is implemented in "Jtool.cs"
                NeedDrawPreview = true;
                PreviewSprite = SkinManager.Instance.GetCurrentSpriteOfType(SelectType);
                PreviewPosition = snappedPos;
            }
            else
            {
                NeedDrawPreview = false;
            }
        }
        /// <summary>
        /// Call this method after creating an object to record the undo action.
        /// </summary>
        public void AddCreateEvent(float x, float y, Type type)
        {
            if (curEvent == null)
                curEvent = new(UndoEvent.EventType.Create);

            curEvent.events.Add(new SubEvent(x, y, type));
        }
        /// <summary>
        /// Call this method after removing an object to record the undo action.
        /// </summary>
        public void AddRemoveEvent(float x, float y, Type type)
        {
            if (curEvent == null)
                curEvent = new(UndoEvent.EventType.Remove);

            curEvent.events.Add(new SubEvent(x, y, type));
        }
        /// <summary>
        /// Call this method after moving an object to record the undo action.
        /// </summary>
        public void AddMoveEvent(Type type, float oldX, float oldY, float newX, float newY)
        {
            if (curEvent == null)
                curEvent = new(UndoEvent.EventType.Move);

            curEvent.events.Add(new SubEvent(type, oldX, oldY, newX, newY));
        }
        /// <summary>
        /// Call this method after completing an event to add the undo event to the list of undo events.
        /// </summary>
        public void FinishEvent()
        {
            if (curEvent != null)
            {
                redoStack.Clear();

                undoStack.Push(curEvent);
                curEvent = null;

                MapManager.Instance.Modified = true;
            }
        }
        /// <summary>
        /// Finish create an object and add it to undo events.
        /// </summary>
        public void FinishCreateObject(float x, float y)
        {
            if (MapObjectManager.Instance.AtPosition(x, y, SelectType).Count == 0)
            {
                var obj = MapObjectManager.Instance.CreateObject(x, y, SelectType);
                AddCreateEvent(x, y, SelectType);
            }
        }
        /// <summary>
        /// Finish move an object and add it to undo events.
        /// </summary>
        public void FinishMoveObject()
        {
            if (handedObject != null)
            {
                AddMoveEvent(handedObject.GetType(), handedOldPos.X, handedOldPos.Y, handedObject.X, handedObject.Y);
                handedObject = null;
                FinishEvent();
            }
        }
        /// <summary>
        /// Undo an edit action
        /// </summary>
        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                var lastEvent = undoStack.Pop();
                redoStack.Push(lastEvent);

                foreach (var subEvent in lastEvent.events)
                {
                    switch (lastEvent.type)
                    {
                        case UndoEvent.EventType.Create:
                            foreach (var i in MapObjectManager.Instance.AtPosition(subEvent.x, subEvent.y, subEvent.objectType))
                            {
                                i.Destroy();
                            }
                            break;
                        case UndoEvent.EventType.Remove:
                            MapObjectManager.Instance.CreateObject(subEvent.x, subEvent.y, subEvent.objectType);
                            break;
                        case UndoEvent.EventType.Move:
                            foreach (var i in MapObjectManager.Instance.AtPosition(subEvent.newX, subEvent.newY, subEvent.objectType))
                            {
                                i.X = subEvent.oldX;
                                i.Y = subEvent.oldY;
                            }
                            break;
                    }
                }
                Gui.Log("Editor", $"Undo event \"{lastEvent.type}\", contains {lastEvent.events.Count} sub events");
            }
        }
        /// <summary>
        /// Redo an edit action
        /// </summary>
        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                var lastEvent = redoStack.Pop();
                undoStack.Push(lastEvent);

                foreach (var subEvent in lastEvent.events)
                {
                    switch (lastEvent.type)
                    {
                        case UndoEvent.EventType.Create:
                            MapObjectManager.Instance.CreateObject(subEvent.x, subEvent.y, subEvent.objectType);
                            break;
                        case UndoEvent.EventType.Remove:
                            foreach (var i in MapObjectManager.Instance.AtPosition(subEvent.x, subEvent.y, subEvent.objectType))
                            {
                                i.Destroy();
                            }
                            break;
                        case UndoEvent.EventType.Move:
                            foreach (var i in MapObjectManager.Instance.AtPosition(subEvent.oldX, subEvent.oldY, subEvent.objectType))
                            {
                                i.X = subEvent.newX;
                                i.Y = subEvent.newY;
                            }
                            break;
                    }
                }
                Gui.Log("Editor", $"Redo event \"{lastEvent.type}\", contains {lastEvent.events.Count} sub events");
            }
        }

        public void SetSelectType(Type type)
        {
            SelectType = type;
            Gui.Log("Editor", $"Selected type \"{type}\"");
        }

        public void ClearUndo()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
    }
    /// <summary>
    /// Undo/redo event. Since many objects can be placed at once, 
    /// multiple sub-events can be included.
    /// </summary>
    public class UndoEvent
    {
        public enum EventType
        {
            Create,
            Remove,
            Move,
        }
        public EventType type;
        public List<SubEvent> events = new();

        public UndoEvent(EventType t)
        {
            type = t;
        }
    }
    /// <summary>
    /// Undo/redo sub-events. In fact, there are two types "create/remove" and "move", 
    /// but for the convenience of saving, they are combined into one class.
    /// </summary>
    public class SubEvent
    {
        public float x = 0, y = 0, oldX = 0, oldY = 0, newX = 0, newY = 0;
        public Type objectType = null;
        /// <summary>
        /// Create/Remove sub-event
        /// </summary>
        public SubEvent(float _x, float _y, Type _type)
        {
            x = _x;
            y = _y;
            objectType = _type;
        }
        /// <summary>
        /// Move sub-event
        /// </summary>
        public SubEvent(Type _type, float _oldX, float _oldY, float _newX, float _newY)
        {
            objectType = _type;
            oldX = _oldX;
            oldY = _oldY;
            newX = _newX;
            newY = _newY;
        }
    }

}
