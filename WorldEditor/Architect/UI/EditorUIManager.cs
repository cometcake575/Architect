using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using Architect.Attributes.Config;
using Architect.Attributes.Receivers;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Architect.Category;
using Architect.Content;
using Architect.MultiplayerHook;
using Architect.Objects;
using Architect.Util;
using Button = MagicUI.Elements.Button;
using GridLayout = MagicUI.Elements.GridLayout;
using Image = MagicUI.Elements.Image;

namespace Architect.UI;

public static class EditorUIManager
{
    // The current page, item index and filter info
    private static int _groupIndex;
    private static int _index;
    private static string _filter = "";
    
    // Categories of items
    private static ObjectCategory _category;
    private static List<ObjectCategory> _categories;
    
    // Info about current selected item and buttons to change item
    private static TextObject _selectionInfo;
    private static List<(Button, Button, Image)> _selectionButtons;
    
    // Selectable objects
    private static List<SelectableObject> _objects;
    [CanBeNull] internal static SelectableObject SelectedItem;

    // Config grids and data
    private static GridLayout _configGrid;
    private static GridLayout _receiversGrid;
    private static GridLayout _broadcastersGrid;
    public static readonly Dictionary<string, ConfigValue> ConfigValues = new();
    public static readonly List<EventBroadcaster> Broadcasters = new();
    public static readonly List<EventReceiver> Receivers = new();

    // Blank sprite texture, used for buttons with no item
    private static Sprite _blankSprite;
    
    // Constants
    private const int ItemsPerGroup = 9;
    private const string FilledStar = "★";
    private const string EmptyStar = "☆";

    // All elements that should not appear when paused (everything except text info)
    internal static List<ArrangableElement> PauseOptions;

    // Sets the current selected category
    private static void SetCategory(ObjectCategory category)
    {
        _category = category;
        _groupIndex = 0;
        _index = 0;
        
        RefreshObjects();
        RefreshButtons();
    }

    // Shifts the currently selected page, looping round if index is out of range
    internal static void ShiftGroup(int amount)
    {
        var groupCount = GetObjects().Count / ItemsPerGroup + 1;
        _groupIndex += amount;
        if (_groupIndex < 0) _groupIndex += groupCount;
        _groupIndex %= groupCount;

        RefreshButtons();
    }

    // Refreshes the objects in the current selection (when filter or category change)
    private static void RefreshObjects()
    {
        _objects = _category.GetObjects().Where(obj => obj.GetName().IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
    }
    
    // Returns the objects in the current selection
    private static List<SelectableObject> GetObjects()
    {
        if (_objects == null) RefreshObjects();
        return _objects;
    }

    // Refresh the object selection buttons
    private static void RefreshButtons()
    {
        var i = 0;
        var objects = GetObjects();
        foreach (var pair in _selectionButtons)
        {
            pair.Item1.Content = (_groupIndex * ItemsPerGroup + i).ToString();
            var index = _groupIndex * ItemsPerGroup + i;
            var currentItem = index < objects.Count ? objects[index] : null;
            if (currentItem != null)
            {
                pair.Item1.Content = "";
                pair.Item3.Sprite = currentItem.GetSprite();
                pair.Item3.GameObject.transform.rotation = Quaternion.Euler(0, 0, currentItem.GetSpriteRotation());
                pair.Item2.Content = currentItem.IsFavourite() ? FilledStar : EmptyStar;
                pair.Item2.ContentColor = currentItem.IsFavourite() ? Color.yellow : Color.white;
            }
            else
            {
                pair.Item1.Content = "Unset";
                pair.Item3.Sprite = _blankSprite;
                pair.Item2.Content = EmptyStar;
                pair.Item2.ContentColor = Color.white;
            }
            i++;
        }
    }

    // Toggles whether something is a favourite
    // Does not refresh grid, so that items can easily be readded if removed by accident
    private static void ToggleFavourite(int index)
    {
        index += _groupIndex * ItemsPerGroup;
        
        var objects = GetObjects();
        if (index >= objects.Count || objects[index] is not PlaceableObject obj) return;
        
        obj.ToggleFavourite();
        RefreshButtons();
    }

    // Refreshes the currently selected item, resetting configuration applied to the object
    private static void RefreshSelectedItem(LayoutRoot layout)
    {
        RotationChoice.Text = "0";
        ScaleChoice.Text = "0";
        
        ConfigValues.Clear();
        Broadcasters.Clear();
        Receivers.Clear();
        switch (_index)
        {
            case -1:
                SelectedItem = null;
                break;
            case -2:
                SelectedItem = EraserObject.Instance;
                break;
            case -3:
                SelectedItem = DragObject.Instance;
                break;
            default:
            {
                var index = _groupIndex * ItemsPerGroup + _index;
                var objects = GetObjects();
                SelectedItem = index < objects.Count ? objects[index] : null;
                break;
            }
        }

        EditorManager.IsFlipped = false;
        EditorManager.Rotation = 0;
        EditorManager.Scale = 1;
        CursorItem.NeedsRefreshing = true;
        _selectionInfo.Text = "Current Item: " + (SelectedItem != null ? SelectedItem.GetName() : "None");
        
        CreateConfigGrid(layout);
        CreateBroadcastersGrid(layout);
        CreateReceiversGrid(layout);
        
        RefreshConfigMode();
    }
    
    // Initializes the UI
    internal static void Initialize(LayoutRoot layout)
    {
        _selectionButtons = new List<(Button, Button, Image)>();
        _blankSprite = Sprite.Create(Texture2D.normalTexture, new Rect(0, 0, 0, 0), new Vector2());
        PauseOptions = new List<ArrangableElement>();

        layout.VisibilityCondition = () => EditorManager.IsEditing;

        SetupTextDisplay(layout);
        SetupLeftSide(layout);
        SetupObjectOptions(layout);
        SetupFilter(layout);
        SetupExtraSettings(layout);
    }

    private static void SetupLeftSide(LayoutRoot layout)
    {
        _leftSideGrid = new GridLayout(layout, "Left Side")
        {
            Padding = new Padding(20, 15),
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Left,
            RowDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(0.2f, GridUnit.Proportional)
            },
            ColumnDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            }
        };

        var categoriesGrid = SetupCategories(layout);
        _leftSideGrid.Children.Add(categoriesGrid);

        var extraControlsGrid = SetupExtraControls(layout)
            .WithProp(GridLayout.Column, 1);
        _leftSideGrid.Children.Add(extraControlsGrid);

        SetupConfigGrid(layout);
        
        PauseOptions.Add(_leftSideGrid);
    }

    public static TextInput RotationChoice;
    public static TextInput ScaleChoice;

    private static GridLayout SetupExtraControls(LayoutRoot layout)
    {
        RotationChoice = new TextInput(layout, "Rotation Input")
        {
            ContentType = InputField.ContentType.DecimalNumber,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 80,
            Text = "0",
            Padding = new Padding(20, 0)
        }.WithProp(GridLayout.Column, 1);
        RotationChoice.TextChanged += (_, s) =>
        {
            EditorManager.Rotation = Convert.ToSingle(s);
            CursorItem.NeedsRefreshing = true;
        };

        ScaleChoice = new TextInput(layout, "Scale Input")
        {
            ContentType = InputField.ContentType.DecimalNumber,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 80,
            Text = "0",
            Padding = new Padding(20, 0)
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 1);
        ScaleChoice.TextChanged += (_, s) =>
        {
            EditorManager.Scale = Convert.ToSingle(s);
            CursorItem.NeedsRefreshing = true;
        };
        
        var grid = new GridLayout(layout, "Extra Controls")
        {
            RowDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            },
            ColumnDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            },
            Children =
            {
                new TextObject(layout, "Rotation Text")
                {
                    Text = "Rotation",
                    HorizontalAlignment = HorizontalAlignment.Left
                },
                new TextObject(layout, "Scale Text")
                {
                    Text = "Scale",
                    HorizontalAlignment = HorizontalAlignment.Left
                }.WithProp(GridLayout.Row, 1),
                RotationChoice,
                ScaleChoice
            },
            VerticalAlignment = VerticalAlignment.Center
        }.WithProp(GridLayout.ColumnSpan, 2);

        return grid;
    }
    
    private static void SetupConfigGrid(LayoutRoot layout)
    {
        var configButton = new Button(layout, "Config Choice")
        {
            Content = "Config",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinWidth = 120
        }.WithProp(GridLayout.Row, 2);
        configButton.Click += _ =>
        {
            _currentMode = ConfigMode.Config;
            RefreshConfigMode();
        };

        var broadcasterButton = new Button(layout, "Broadcaster Choice")
        {
            Content = "Events",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinWidth = 120,
            Padding = new Padding(10, 0)
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 2);
        broadcasterButton.Click += _ =>
        {
            _currentMode = ConfigMode.Broadcasters;
            RefreshConfigMode();
        };

        var receiverButton = new Button(layout, "Receiver Choice")
        {
            Content = "Listeners",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinWidth = 120,
            Padding = new Padding(10, 0)
        }.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, 2);
        receiverButton.Click += _ =>
        {
            _currentMode = ConfigMode.Receivers;
            RefreshConfigMode();
        };
        
        _leftSideGrid.Children.Add(configButton);
        _leftSideGrid.Children.Add(broadcasterButton);
        _leftSideGrid.Children.Add(receiverButton);
    }

    private static void SetupTextDisplay(LayoutRoot layout)
    {
        new TextObject(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            MaxWidth = 200,
            MaxHeight = 20,
            Padding = new Padding(0, 60)
        }.Text = "Editor Enabled";

        _selectionInfo = new TextObject(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new Padding(0, 90),
            MaxWidth = 1000,
            MaxHeight = 20,
            Text = "Current Item: None"
        };
    }

    private static GridLayout SetupCategories(LayoutRoot layout)
    {
        _categories = new List<ObjectCategory>();
        
        SetCategory(FavouritesCategory.Instance);

        Dictionary<string, NormalCategory> normalCategories = new();
        foreach (var element in ContentPacks.Packs.Where(pack => pack.IsEnabled()).SelectMany(pack => pack))
        {
            if (!normalCategories.ContainsKey(element.GetCategory()))
            {
                var normalCategory = new NormalCategory(element.GetCategory());
                normalCategories.Add(element.GetCategory(), normalCategory);
                _categories.Add(normalCategory);
            }
            normalCategories[element.GetCategory()].AddObject(PlaceableObject.Create(element));
        }

        foreach (var category in normalCategories.Values)
        {
            category.Sort();
        }
        
        _categories.Add(new BlankCategory());
        _categories.Add(_category);
        
        var grid = new GridLayout(layout, "Categories")
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        var columnCount = _categories.Count;

        foreach (var category in _categories)
        {
            columnCount -= 1;
            grid.RowDefinitions.Add(new GridDimension(1, GridUnit.Proportional));
            if (!category.CreateButton()) continue;
            
            var button = new Button(layout, category.GetName())
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Padding = new Padding(0, 2),
                Content = category.GetName()
            }.WithProp(GridLayout.Row, columnCount);
            button.Click += _ => { SetCategory(category); };
            grid.Children.Add(button);
        }

        return grid;
    }

    private static void SetupObjectOptions(LayoutRoot layout)
    {
        for (var i = 0; i < ItemsPerGroup; i++)
        {
            var j = 2 - i / 3;

            var (button, image) = CreateImagedButton(layout, _blankSprite, i.ToString(), (2 - i % 3) * 100, j * 100, i);

            var favourite = new Button(layout, i + " Favourite")
            {
                Content = EmptyStar,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                MinHeight = 20,
                MinWidth = 20,
                Borderless = true,
                Padding = new Padding((2 - i % 3) * 100 + 92, j * 100 + 45)
            };
            var k = i;
            favourite.Click += _ =>
            {
                ToggleFavourite(k);
            };
            PauseOptions.Add(favourite);
            
            _selectionButtons.Add((button, favourite, image));
        }
        RefreshObjects();
        RefreshButtons();
    }

    private static void SetupFilter(LayoutRoot layout)
    {
        var filter = new TextInput(layout, "Search")
        {
            ContentType = InputField.ContentType.Standard,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinWidth = 200,
            Padding = new Padding(55, 315)
        };
        filter.TextChanged += (_, s) =>
        {
            _filter = s;
            _groupIndex = 0;
            
            RefreshObjects();
            RefreshButtons();
        };
        PauseOptions.Add(filter);
    }

    private static void SetupExtraSettings(LayoutRoot layout)
    {
        var extraSettings = new GridLayout(layout, "Extra Settings")
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Padding = new Padding(335, 0),
            ColumnDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            },
            RowDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional)
            }
        };

        var correctRow = 0;
        
        if (Architect.UsingMultiplayer)
        {
            extraSettings.RowDefinitions.Add(new GridDimension(0.2f, GridUnit.Proportional));
            var multiplayerRefresh = new Button(layout, "Refresh")
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Content = "Reshare Level (HKMP)"
            }.WithProp(GridLayout.Column, 0).WithProp(GridLayout.ColumnSpan, 3).WithProp(GridLayout.Row, correctRow);
            multiplayerRefresh.Click += _ =>
            {
                HkmpHook.Refresh();
            };
            extraSettings.Children.Add(multiplayerRefresh);
            correctRow++;
        }
        
        var cursorSprite = ResourceUtils.Load("cursor");
        var cursorImagedButton = CreateImagedButton(layout, cursorSprite, "Cursor", 0, 0, -1);
        cursorImagedButton.Item1.WithProp(GridLayout.Column, 0).WithProp(GridLayout.Row, correctRow);
        cursorImagedButton.Item2.WithProp(GridLayout.Column, 0).WithProp(GridLayout.Row, correctRow);
        extraSettings.Children.Add(cursorImagedButton.Item1);
        extraSettings.Children.Add(cursorImagedButton.Item2);

        var dragImagedButton = CreateImagedButton(layout, DragObject.Instance.GetSprite(), "Drag", 0, 0, -3);
        dragImagedButton.Item1.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, correctRow);
        dragImagedButton.Item2.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, correctRow);
        extraSettings.Children.Add(dragImagedButton.Item1);
        extraSettings.Children.Add(dragImagedButton.Item2);

        var eraserImagedButton = CreateImagedButton(layout, EraserObject.Instance.GetSprite(), "Eraser", 0, 0, -2);
        eraserImagedButton.Item1.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, correctRow);
        eraserImagedButton.Item2.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, correctRow);
        extraSettings.Children.Add(eraserImagedButton.Item1);
        extraSettings.Children.Add(eraserImagedButton.Item2);
        
        PauseOptions.Add(extraSettings);
    }

    private static (Button, Image) CreateImagedButton(LayoutRoot layout, Sprite sprite, string name, int horizontalPadding, int verticalPadding, int index)
    {
        var img = new Image(layout, sprite, name + " Image")
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 40,
            Width = 40,
            PreserveAspectRatio = true,
            Padding = new Padding(horizontalPadding + 35, verticalPadding + 35)
        };
            
        var button = new Button(layout, name + " Button")
        {
            Content = "",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinHeight = 80,
            MinWidth = 80,
            Padding = new Padding(horizontalPadding + 15, verticalPadding + 15)
        };
        
        button.Click += _ =>
        {
            _index = index;
            RefreshSelectedItem(layout);
        };

        PauseOptions.Add(img);
        PauseOptions.Add(button);
        
        return (button, img);
    }

    private static ConfigMode _currentMode = ConfigMode.Config;
    private static GridLayout _leftSideGrid;
    
    private enum ConfigMode
    {
        Config,
        Broadcasters,
        Receivers
    }

    private static void RefreshConfigMode()
    {
        if (_configGrid != null) _configGrid.Visibility = _currentMode == ConfigMode.Config ? Visibility.Visible : Visibility.Hidden;
        if (_broadcastersGrid != null) _broadcastersGrid.Visibility = _currentMode == ConfigMode.Broadcasters ? Visibility.Visible : Visibility.Hidden;
        if (_receiversGrid != null) _receiversGrid.Visibility = _currentMode == ConfigMode.Receivers ? Visibility.Visible : Visibility.Hidden;
    }

    private static void CreateConfigGrid(LayoutRoot layout)
    {
        _configGrid?.Destroy();
        _configGrid = null;
        
        if (SelectedItem is not PlaceableObject placeable) return;

        _configGrid = new GridLayout(layout, "Config Grid")
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            ColumnDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            }
        }.WithProp(GridLayout.Row, 1).WithProp(GridLayout.ColumnSpan, 3);

        var i = 0;
        foreach (var type in placeable.PackElement.GetConfigGroup().Types)
        {
            _configGrid.RowDefinitions.Add(new GridDimension(1, GridUnit.Proportional));
            _configGrid.Children.Add(
                new TextObject(layout, type.Name + " Description")
                {
                    Text = type.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Padding = new Padding(0, 4)
                }.WithProp(GridLayout.Row, i)
            );

            var apply = new Button(layout, type.Name + " Button")
            {
                Content = "Apply",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                MinWidth = 40,
                Padding = new Padding(0, 4),
                Enabled = false
            }.WithProp(GridLayout.Row, i).WithProp(GridLayout.Column, 2);
            _configGrid.Children.Add(apply);

            var input = type.CreateInput(layout, apply);

            input.GetElement().VerticalAlignment = VerticalAlignment.Center;
            input.GetElement().HorizontalAlignment = HorizontalAlignment.Center;
            input.GetElement().Padding = new Padding(0, 4);
            input.GetElement().WithProp(GridLayout.Row, i).WithProp(GridLayout.Column, 1);

            _configGrid.Children.Add(input.GetElement());

            apply.Click += button =>
            {
                var value = input.GetValue();
                if (value.Length != 0)
                {
                    var configValue = type.Deserialize(value);
                    ConfigValues[type.Name] = configValue;
                }
                else ConfigValues.Remove(type.Name);

                CursorItem.NeedsRefreshing = true;

                button.Enabled = false;
            };

            i++;
        }
        _leftSideGrid.Children.Add(_configGrid);
    }

    private static void CreateReceiversGrid(LayoutRoot layout)
    {
        _receiversGrid?.Destroy();
        _receiversGrid = null;

        if (SelectedItem is not PlaceableObject placeable) return;

        _receiversGrid = new GridLayout(layout, "Config Grid")
        {
            Padding = new Padding(0, 10),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            RowDefinitions = {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            },
            ColumnDefinitions = { 
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            }
        }.WithProp(GridLayout.Row, 1).WithProp(GridLayout.ColumnSpan, 3);

        var info1 = new TextObject(layout, "Name Label")
        {
            Text = "Name",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var info2 = new TextObject(layout, "Trigger Label")
        {
            Text = "Trigger",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Column, 1);

        var info3 = new TextObject(layout, "Times Label")
        {
            Text = "Times",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Column, 2);

        var info4 = new TextObject(layout, "Add Label")
        {
            Text = "Add/Remove",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Column, 3);
        
        var eventNameInput = new TextInput(layout, "Name")
        {
            Padding = new Padding(10, 0),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            ContentType = InputField.ContentType.Alphanumeric,
            MinWidth = 80
        }.WithProp(GridLayout.Row, 1);
        
        var eventTypeInput = new TextInput(layout, "Trigger Type")
        {
            ContentType = InputField.ContentType.Alphanumeric,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            MinWidth = 80
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 1);
        
        var times = new TextInput(layout, "Times")
        {
            Text = "1",
            ContentType = InputField.ContentType.IntegerNumber,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            MinWidth = 30
        }.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, 1);
        
        var add = new Button(layout, "Add")
        {
            Content = "+",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            MinWidth = 20,
            Enabled = false
        }.WithProp(GridLayout.Column, 3).WithProp(GridLayout.Row, 1);

        eventTypeInput.TextChanged += (_, s) => { ValidateReceiver(s, eventNameInput.Text, add, placeable); };
        eventNameInput.TextChanged += (_, s) => { ValidateReceiver(eventTypeInput.Text, s, add, placeable); };

        add.Click += button =>
        {
            button.Enabled = false;
            if (!int.TryParse(times.Text, out var time)) time = 1;
            var receiver = EventManager.CreateReceiver(eventTypeInput.Text, eventNameInput.Text, time);
            Receivers.Add(receiver);

            var info = new TextObject(layout)
            {
                Text = "On: " + eventNameInput.Text + " | Trigger: " + eventTypeInput.Text,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center
            }.WithProp(GridLayout.ColumnSpan, 2).WithProp(GridLayout.Row, _receiversGrid.RowDefinitions.Count);

            var remove = new Button(layout)
            {
                Content = "–",
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                MinWidth = 20,
                Enabled = true
            }.WithProp(GridLayout.Column, 3).WithProp(GridLayout.Row, _receiversGrid.RowDefinitions.Count);

            var def = new GridDimension(1, GridUnit.Proportional);
            _receiversGrid.RowDefinitions.Add(def);
            
            remove.Click += _ =>
            {
                Receivers.Remove(receiver);
                info.Destroy();
                remove.Destroy();
            };

            _receiversGrid.Children.Add(info);
            _receiversGrid.Children.Add(remove);

            eventTypeInput.Text = "";
            eventNameInput.Text = "";
        };
        
        _receiversGrid.Children.Add(info1);
        _receiversGrid.Children.Add(info2);
        _receiversGrid.Children.Add(info3);
        _receiversGrid.Children.Add(info4);
        
        _receiversGrid.Children.Add(eventTypeInput);
        _receiversGrid.Children.Add(eventNameInput);
        _receiversGrid.Children.Add(times);
        _receiversGrid.Children.Add(add);
        
        _leftSideGrid.Children.Add(_receiversGrid);
    }

    private static void CreateBroadcastersGrid(LayoutRoot layout)
    {
        _broadcastersGrid?.Destroy();
        _broadcastersGrid = null;

        if (SelectedItem is not PlaceableObject placeable) return;

        _broadcastersGrid = new GridLayout(layout, "Config Grid")
        {
            Padding = new Padding(0, 10),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            RowDefinitions = {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            },
            ColumnDefinitions = { 
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            }
        }.WithProp(GridLayout.Row, 1).WithProp(GridLayout.ColumnSpan, 3);

        var info1 = new TextObject(layout, "Type Label")
        {
            Text = "Event Type",
            Padding = new Padding(10, 0),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var info2 = new TextObject(layout, "Name Label")
        {
            Text = "Name",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Column, 1);

        var info3 = new TextObject(layout, "Add Label")
        {
            Text = "Add/Remove",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Column, 2);
        
        var eventTypeInput = new TextInput(layout, "Event Type")
        {
            Padding = new Padding(10, 0),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            ContentType = InputField.ContentType.Alphanumeric,
            MinWidth = 120
        }.WithProp(GridLayout.Row, 1);
        
        var eventNameInput = new TextInput(layout, "Name")
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            ContentType = InputField.ContentType.Alphanumeric,
            MinWidth = 120
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 1);
        
        var add = new Button(layout, "Add")
        {
            Content = "+",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            MinWidth = 20,
            Enabled = false
        }.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, 1);

        eventTypeInput.TextChanged += (_, s) => { ValidateBroadcaster(s, eventNameInput.Text, add, placeable); };
        eventNameInput.TextChanged += (_, s) => { ValidateBroadcaster(eventTypeInput.Text, s, add, placeable); };

        add.Click += button =>
        {
            button.Enabled = false;
            if (!Enum.TryParse<EventBroadcasterType>(eventTypeInput.Text, true, out var type)) return;

            var broadcaster = new EventBroadcaster(type, eventNameInput.Text);
            Broadcasters.Add(broadcaster);

            var info = new TextObject(layout)
            {
                Text = "Event: " + type + " | Name: " + eventNameInput.Text,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center
            }.WithProp(GridLayout.ColumnSpan, 2).WithProp(GridLayout.Row, _broadcastersGrid.RowDefinitions.Count);

            var remove = new Button(layout)
            {
                Content = "–",
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                MinWidth = 20,
                Enabled = true
            }.WithProp(GridLayout.Column, 3).WithProp(GridLayout.Row, _broadcastersGrid.RowDefinitions.Count);

            var def = new GridDimension(1, GridUnit.Proportional);
            _broadcastersGrid.RowDefinitions.Add(def);
            
            remove.Click += _ =>
            {
                Broadcasters.Remove(broadcaster);
                info.Destroy();
                remove.Destroy();
            };

            _broadcastersGrid.Children.Add(info);
            _broadcastersGrid.Children.Add(remove);

            eventTypeInput.Text = "";
            eventNameInput.Text = "";
        };
        
        _broadcastersGrid.Children.Add(info1);
        _broadcastersGrid.Children.Add(info2);
        _broadcastersGrid.Children.Add(info3);
        
        _broadcastersGrid.Children.Add(eventTypeInput);
        _broadcastersGrid.Children.Add(eventNameInput);
        _broadcastersGrid.Children.Add(add);
        
        _leftSideGrid.Children.Add(_broadcastersGrid);
    }
    
    private static void ValidateBroadcaster(string eText, string nameText, Button add, PlaceableObject placeable)
    {
        var valid = false;

        if (nameText.Length > 0)
            foreach (var broadcastType in placeable.PackElement.GetBroadcasterGroup().Types)
            {
                if (eText.ToLower().Equals(broadcastType.ToString().ToLower())) valid = true;
            }
            
        add.Enabled = valid;
    }
    
    private static void ValidateReceiver(string eText, string nameText, Button add, PlaceableObject placeable)
    {
        var valid = false;

        if (nameText.Length > 0)
            foreach (var receiverType in placeable.PackElement.GetReceiverGroup().Types)
            {
                if (eText.ToLower().Equals(receiverType.ToLower())) valid = true;
            }
            
        add.Enabled = valid;
    }
}