using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Architect.Attributes.Broadcasters;
using Architect.Attributes.Config;
using Architect.Attributes.Receivers;
using Architect.Category;
using Architect.Content;
using Architect.MultiplayerHook;
using Architect.Objects;
using Architect.Storage;
using Architect.Util;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Button = MagicUI.Elements.Button;
using GridLayout = MagicUI.Elements.GridLayout;
using Image = MagicUI.Elements.Image;

namespace Architect.UI;

public static class EditorUIManager
{
    // Constants
    private const int ItemsPerGroup = 9;
    private const string FilledStar = "★";
    private const string EmptyStar = "☆";
    private const string Bin = "X";

    private const string Nothing = " ";

    // UI Scaling
    private static float _uiScaleFactor = 1.0f;
    public static float UI_SCALE_FACTOR => _uiScaleFactor;
    private const int BASE_BUTTON_SIZE = 80;
    private const int BASE_IMAGE_SIZE = 40;
    private const int BASE_PADDING = 15;

    // The current page, item index and filter info
    private static int _groupIndex;
    private static int _index;
    private static string _filter = "";

    // Categories of items
    private static ObjectCategory _category;
    private static List<ObjectCategory> _categories;

    // Info about current selected item and buttons to change item
    private static TextObject _editorEnabledText;
    private static TextObject _selectionInfo;
    private static TextObject _sceneInfo;
    private static TextObject _extraInfo;
    private static List<(Button, Button, Image)> _selectionButtons;

    // Selectable objects
    private static List<SelectableObject> _objects;
    [CanBeNull] internal static SelectableObject SelectedItem;

    // Config grids and data
    private static GridLayout _configGrid;
    private static GridLayout _receiversGrid;
    private static GridLayout _broadcastersGrid;
    public static readonly Dictionary<string, ConfigValue> ConfigValues = new();
    public static readonly List<EventBroadcaster> Broadcasters = [];
    public static readonly List<EventReceiver> Receivers = [];

    // All elements that should not appear when paused (everything except text info)
    internal static List<ArrangableElement> PauseOptions;

    private static LayoutRoot _layout;

    public static TextInput RotationChoice;
    public static TextInput ScaleChoice;

    private static Button _configButton;
    private static Button _broadcasterButton;
    private static Button _receiverButton;

    private static int _lastInfoIndex;

    private static ConfigMode _currentMode = ConfigMode.Config;
    private static GridLayout _leftSideGrid;

    private static readonly List<(ArrangableElement, ArrangableElement)> ReceiversInUI = [];

    private static readonly List<(ArrangableElement, ArrangableElement)> BroadcastersInUI = [];

    private static TextObject _bigText;

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
    internal static void RefreshObjects()
    {
        _objects = _category.GetObjects()
            .Where(obj => obj.GetName().IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
    }

    // Returns the objects in the current selection
    private static List<SelectableObject> GetObjects()
    {
        if (_objects == null) RefreshObjects();
        return _objects;
    }

    // Refresh the object selection buttons
    internal static void RefreshButtons()
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
                pair.Item2.Content = _category is PrefabsCategory ? Bin :
                    currentItem.IsFavourite() ? FilledStar : EmptyStar;
                pair.Item2.ContentColor = _category is PrefabsCategory ? Color.red :
                    currentItem.IsFavourite() ? Color.yellow : Color.white;
            }
            else
            {
                pair.Item1.Content = "Unset";
                pair.Item3.Sprite = Architect.BlankSprite;
                pair.Item2.Content = Nothing;
                pair.Item2.ContentColor = Color.white;
            }

            i++;
        }
    }

    // Toggles whether something is a favourite or deleted a prefab
    // Does not refresh grid, so that items can easily be readded if removed by accident
    private static void ToggleFavourite(int index)
    {
        index += _groupIndex * ItemsPerGroup;

        var objects = GetObjects();
        if (index >= objects.Count || objects[index] is not PlaceableObject obj) return;

        if (obj is PrefabObject prefab)
        {
            prefab.Delete();
        }
        else
        {
            obj.ToggleFavourite();
            RefreshButtons();
        }
    }

    private static void UpdateSelectedItem()
    {
        ConfigValues.Clear();
        Broadcasters.Clear();
        Receivers.Clear();

        switch (_index)
        {
            case -1:
                SelectedItem = CursorObject.Instance;
                break;
            case -2:
                SelectedItem = EraserObject.Instance;
                break;
            case -3:
                SelectedItem = DragObject.Instance;
                break;
            case -4:
                SelectedItem = PickObject.Instance;
                break;
            case -5:
                SelectedItem = ResetObject.Instance;
                break;
            case -6:
                SelectedItem = LockObject.Instance;
                break;
            default:
            {
                var index = _groupIndex * ItemsPerGroup + _index;
                var objects = GetObjects();
                SelectedItem = index < objects.Count ? objects[index] : null;
                break;
            }
        }
    }

    // Refreshes the currently selected item, resetting configuration applied to the object
    public static void RefreshSelectedItem(bool useDefaultConfigValues)
    {
        RotationChoice.Text = "0";
        ScaleChoice.Text = "1";

        EditorManager.IsFlipped = false;
        EditorManager.Rotation = 0;
        EditorManager.Scale = 1;
        CursorItem.NeedsRefreshing = true;
        _selectionInfo.Text = "Current Item: " + (SelectedItem != null ? SelectedItem.GetName() : "None");

        CreateConfigGrid(_layout, useDefaultConfigValues);
        CreateBroadcastersGrid(_layout);
        CreateReceiversGrid(_layout);

        _currentMode = ConfigMode.Config;
        RefreshConfigMode();
    }

    // Initializes the UI
    internal static void Initialize(LayoutRoot layout)
    {
	try
    	{
            _layout = layout;

            _selectionButtons = [];
            PauseOptions = [];

            layout.VisibilityCondition = () => EditorManager.IsEditing;

            SetupTextDisplay(layout);
            SetupLeftSide(layout);
            SetupObjectOptions(layout);
            SetupFilter(layout);
            SetupTools(layout);

            On.HeroController.SceneInit += (orig, self) =>
            {
                orig(self);
                _sceneInfo.Text = "Scene: " + GameManager.instance.sceneName;
            };
	}

	catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Editor UI: {e}");
            throw; // Re-throw to see the actual error
        }
    }

    private static void SetupLeftSide(LayoutRoot layout)
    {
	var basePadding = 20;
        var baseRow1Height = 120;
        var baseRow2Height = 280; 
        var baseRow3Height = 25;
        var baseColumnWidth = 150;
        var categoriesVerticalOffset = 40;

        _leftSideGrid = new GridLayout(layout, "Left Side")
        {
            Padding = new Padding((int)(basePadding * _uiScaleFactor), (int)((basePadding + categoriesVerticalOffset) * _uiScaleFactor)),
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Left,
            RowDefinitions =
            {
                new GridDimension((int)(baseRow1Height * _uiScaleFactor), GridUnit.AbsoluteMin),
                new GridDimension((int)(baseRow2Height * _uiScaleFactor), GridUnit.AbsoluteMin),
                new GridDimension((int)(baseRow3Height * _uiScaleFactor), GridUnit.AbsoluteMin)
            },
            ColumnDefinitions =
            {
                new GridDimension((int)(baseColumnWidth * _uiScaleFactor), GridUnit.AbsoluteMin),
                new GridDimension((int)(baseColumnWidth * _uiScaleFactor), GridUnit.AbsoluteMin),
                new GridDimension((int)(baseColumnWidth * _uiScaleFactor), GridUnit.AbsoluteMin)
            }
        };

        var categoriesGrid = SetupCategories(layout);
        _leftSideGrid.Children.Add(categoriesGrid);

        var extraControlsGrid = SetupExtraControls(layout)
            .WithProp(GridLayout.Column, 1);
        _leftSideGrid.Children.Add(extraControlsGrid);

        SetupConfigArea(layout);

        PauseOptions.Add(_leftSideGrid);
    }

    private static GridLayout SetupExtraControls(LayoutRoot layout)
    {
	var baseInputWidth = 80;
        var basePadding = 20;
        var baseFontSize = 12;

        RotationChoice?.Destroy();
        ScaleChoice?.Destroy();

        RotationChoice = new TextInput(layout, "Rotation Input")
        {
            ContentType = InputField.ContentType.DecimalNumber,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = (int)(baseInputWidth * _uiScaleFactor),
            Text = "0",
            Padding = new Padding((int)(basePadding * _uiScaleFactor), (int)(10 * _uiScaleFactor)),
	    FontSize = (int)(baseFontSize * _uiScaleFactor)
        }.WithProp(GridLayout.Column, 1);
        RotationChoice.TextEditFinished += (_, s) =>
        {
            EditorManager.Rotation = Convert.ToSingle(s);
            CursorItem.NeedsRefreshing = true;
        };

        ScaleChoice = new TextInput(layout, "Scale Input")
        {
            ContentType = InputField.ContentType.DecimalNumber,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = (int)(baseInputWidth * _uiScaleFactor),
            Text = "1",
            Padding = new Padding((int)(basePadding * _uiScaleFactor), (int)(10 * _uiScaleFactor)),
	    FontSize = (int)(baseFontSize * _uiScaleFactor)
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 1);
        ScaleChoice.TextEditFinished += (_, s) =>
        {
            EditorManager.Scale = Convert.ToSingle(s.Replace(",", "."), CultureInfo.InvariantCulture);
            CursorItem.NeedsRefreshing = true;
        };

        var grid = new GridLayout(layout, "Extra Controls")
        {
            RowDefinitions =
            {
                new GridDimension((int)(40 * _uiScaleFactor), GridUnit.AbsoluteMin),
                new GridDimension((int)(40 * _uiScaleFactor), GridUnit.AbsoluteMin)
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
                    HorizontalAlignment = HorizontalAlignment.Left,
		    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Padding(0, 10),
		    FontSize = (int)(baseFontSize * _uiScaleFactor)
                },
                new TextObject(layout, "Scale Text")
                {
                    Text = "Scale",
                    HorizontalAlignment = HorizontalAlignment.Left,
		    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Padding(0, 10),
		    FontSize = (int)(baseFontSize * _uiScaleFactor)
                }.WithProp(GridLayout.Row, 1),
                RotationChoice,
                ScaleChoice
            },
            VerticalAlignment = VerticalAlignment.Center
        }.WithProp(GridLayout.ColumnSpan, 2);

        return grid;
    }

    private static void SetupConfigArea(LayoutRoot layout)
    {
	_configButton?.Destroy();
        _broadcasterButton?.Destroy();
        _receiverButton?.Destroy();

	var baseButtonWidth = 140;
        var baseFontSize = 12;
	var configButtonsTopPadding = 10;

        _configButton = new Button(layout, "Config Choice")
        {
            Content = "Config",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinWidth = (int)(baseButtonWidth * _uiScaleFactor),
            MinHeight = (int)(25 * _uiScaleFactor),
            FontSize = (int)(baseFontSize * _uiScaleFactor),
	    Padding = new Padding(0, (int)(configButtonsTopPadding * _uiScaleFactor)),
            Enabled = false
        }.WithProp(GridLayout.Row, 2);
        _configButton.Click += _ =>
        {
            _currentMode = ConfigMode.Config;
            RefreshConfigMode();
        };

        _broadcasterButton = new Button(layout, "Broadcaster Choice")
        {
            Content = "Events",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinWidth = (int)(baseButtonWidth * _uiScaleFactor),
            MinHeight = (int)(25 * _uiScaleFactor),
            FontSize = (int)(baseFontSize * _uiScaleFactor),
            Padding = new Padding((int)(20 * _uiScaleFactor), (int)(configButtonsTopPadding * _uiScaleFactor)),
            Enabled = false
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 2);
        _broadcasterButton.Click += _ =>
        {
            _currentMode = ConfigMode.Broadcasters;
            RefreshConfigMode();
        };

        _receiverButton = new Button(layout, "Receiver Choice")
        {
            Content = "Listeners",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinWidth = (int)(baseButtonWidth * _uiScaleFactor),
            MinHeight = (int)(25 * _uiScaleFactor),
            FontSize = (int)(baseFontSize * _uiScaleFactor),
	    Padding = new Padding(0, (int)(configButtonsTopPadding * _uiScaleFactor)),
            Enabled = false
        }.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, 2);
        _receiverButton.Click += _ =>
        {
            _currentMode = ConfigMode.Receivers;
            RefreshConfigMode();
        };

        _leftSideGrid.Children.Add(_configButton);
        _leftSideGrid.Children.Add(_broadcasterButton);
        _leftSideGrid.Children.Add(_receiverButton);
    }

    private static void SetupTextDisplay(LayoutRoot layout)
    {
        var baseFontSize = 12;

	_editorEnabledText?.Destroy();
        _selectionInfo?.Destroy();
        _sceneInfo?.Destroy();
        _extraInfo?.Destroy();
        _bigText?.Destroy();

	var basePadding = 80;
        var paddingIncrement = 35;

        _editorEnabledText = new TextObject(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            MaxWidth = (int)(250 * _uiScaleFactor),
            MaxHeight = (int)(25 * _uiScaleFactor),
            Padding = new Padding(0, (int)(basePadding * _uiScaleFactor)),
            Text = "Editor Enabled",
	    FontSize = (int)(baseFontSize * _uiScaleFactor)
        };

        _selectionInfo = new TextObject(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new Padding(0, (int)((basePadding + paddingIncrement) * _uiScaleFactor)),
            MaxWidth = (int)(1200 * _uiScaleFactor),
            MaxHeight = (int)(20 * _uiScaleFactor),
            Text = "Current Item: None",
	    FontSize = (int)(baseFontSize * _uiScaleFactor)
        };

        _sceneInfo = new TextObject(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new Padding(0, (int)((basePadding + paddingIncrement * 2) * _uiScaleFactor)),
            MaxWidth = (int)(1200 * _uiScaleFactor),
            MaxHeight = (int)(25 * _uiScaleFactor),
            Text = "Scene: None",
	    FontSize = (int)(baseFontSize * _uiScaleFactor)
        };

        _extraInfo = new TextObject(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new Padding(0, (int)((basePadding + paddingIncrement * 3) * _uiScaleFactor)),
            MaxWidth = (int)(1200 * _uiScaleFactor),
            MaxHeight = (int)(25 * _uiScaleFactor),
            Text = "ID: None",
            Visibility = Visibility.Hidden,
	    FontSize = (int)(baseFontSize * _uiScaleFactor)
        };

        _bigText = new TextObject(layout)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = (int)(128 * _uiScaleFactor),
            Text = ""
        };
    }

    public static async Task DisplayExtraInfo(string info)
    {
        _lastInfoIndex++;
        var index = _lastInfoIndex;

        _extraInfo.Text = info;
        _extraInfo.Visibility = Visibility.Visible;

        await Task.Delay(5000);

        if (_lastInfoIndex != index) return;
        _extraInfo.Visibility = Visibility.Hidden;
    }

    private static GridLayout SetupCategories(LayoutRoot layout)
    {
	var existingCategories = _leftSideGrid?.Children?.FirstOrDefault(c => c.Name == "Categories");
    
        existingCategories?.Destroy();

        _categories = [];

        SetCategory(FavouritesCategory.Instance);

        Dictionary<string, NormalCategory> normalCategories = new();

        if (ContentPacks.Packs != null)
        {
            foreach (var element in ContentPacks.Packs.Where(pack => pack?.IsEnabled() == true).SelectMany(pack => pack))
            {
                if (element?.GetCategory() == null) continue;

                if (!normalCategories.ContainsKey(element.GetCategory()))
                {
                    var normalCategory = new NormalCategory(element.GetCategory());
                    normalCategories.Add(element.GetCategory(), normalCategory);
                    _categories.Add(normalCategory);
                }

                var placeable = PlaceableObject.Create(element);
                if (placeable != null)
                {
                    normalCategories[element.GetCategory()].AddObject(placeable);
                }
            }
        }

        foreach (var category in normalCategories.Values) category.Sort();

        try
        {
            _categories.Add(new BlankCategory());
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create BlankCategory: {e}");
        }

        try
        {
            _categories.Add(new PrefabsCategory());
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create PrefabsCategory: {e}");
        }

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
            if (category == null || !category.CreateButton()) continue;

            var button = new Button(layout, category.GetName())
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Padding = new Padding(0, 2),
                Content = category.GetName(),
		FontSize = (int)(12 * _uiScaleFactor),
		MinHeight = (int)(30 * _uiScaleFactor)
            }.WithProp(GridLayout.Row, columnCount);
            button.Click += _ => { SetCategory(category); };
            grid.Children.Add(button);
        }

        return grid;
    }

    private static void SetupObjectOptions(LayoutRoot layout)
    {
	var baseButtonSpacingX = 100;
        var baseButtonSpacingY = 100;
        var baseGridOffsetX = 30;
        var baseGridOffsetY = 30;

	foreach (var (button, favButton, image) in _selectionButtons)
        {
            button?.Destroy();
            favButton?.Destroy();
            image?.Destroy();
        }
        
	_selectionButtons.Clear();

        for (var i = 0; i < ItemsPerGroup; i++)
        {
            var row = 2 - i / 3;
            var col = i % 3;

	    var horizontalPos = baseGridOffsetX + col * baseButtonSpacingX * _uiScaleFactor;
            var verticalPos = baseGridOffsetY + row * baseButtonSpacingY * _uiScaleFactor;

            var (button, image) =
                CreateImagedButton(layout, Architect.BlankSprite, i.ToString(), (2 - i % 3) * 100, row * 100, i);

            var favourite = new Button(layout, i + " Favourite")
            {
                Content = EmptyStar,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                MinHeight = (int)(20 * _uiScaleFactor),
                MinWidth = (int)(20 * _uiScaleFactor),
		FontSize = (int)(12 * _uiScaleFactor),
                Borderless = true,
                Padding = new Padding(
                    (int)((horizontalPos + 50) * _uiScaleFactor),
                    (int)((verticalPos + 50) * _uiScaleFactor)
    	    	),
            };
            var k = i;
            favourite.Click += _ => { ToggleFavourite(k); };
            PauseOptions.Add(favourite);

            _selectionButtons.Add((button, favourite, image));

	    PauseOptions.Add(button);
            PauseOptions.Add(image);
        }

        var resetHorizontalPos = baseGridOffsetX;
        var resetVerticalPos = baseGridOffsetY + 3 * baseButtonSpacingY;

        var existingReset = PauseOptions.FirstOrDefault(x => x.Name?.Contains("Reset Button") == true);
    	if (existingReset == null)
    	{
        	var (resetButton, resetImage) = CreateImagedButton(layout, ResetObject.Instance.GetSprite(), "Reset", 
            		resetHorizontalPos, resetVerticalPos, -5);
        
        	// Add reset button and image to PauseOptions
        	PauseOptions.Add(resetButton);
        	PauseOptions.Add(resetImage);
    	}

        RefreshObjects();
        RefreshButtons();
    }

    private static void SetupFilter(LayoutRoot layout)
    {
	var baseFilterWidth = 200;
        var baseFilterX = 100;
        var baseFilterY = 350;

        var filter = new TextInput(layout, "Search")
        {
            ContentType = InputField.ContentType.Standard,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinWidth = (int)(baseFilterWidth * _uiScaleFactor),
	    FontSize = (int)(12 * _uiScaleFactor),
            Padding = new Padding((int)(baseFilterX * _uiScaleFactor), (int)(baseFilterY * _uiScaleFactor))
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

    public static void ApplyUIScale(float scaleFactor)
    {
        _uiScaleFactor = scaleFactor;
    
        if (_layout != null && EditorManager.IsEditing)
        {
            var currentSelectedItem = SelectedItem;
            var currentIndex = _index;
            var currentGroupIndex = _groupIndex;
            var currentCategory = _category;
	    var currentFilter = _filter;

            // Destroy current UI elements
            foreach (var element in PauseOptions.ToArray())
            {
                if (element != null)
                {
                    element.Destroy();
                }
            }

            PauseOptions.Clear();

	    if (_selectionButtons != null)
            {
                foreach (var (button, favButton, image) in _selectionButtons)
                {
                    button?.Destroy();
                    favButton?.Destroy();
                    image?.Destroy();
                }
                _selectionButtons.Clear();
            }
            else
            {
                _selectionButtons = new List<(Button, Button, Image)>();
            }

	    _configGrid?.Destroy();
            _broadcastersGrid?.Destroy();
            _receiversGrid?.Destroy();
            _editorEnabledText?.Destroy();
            _selectionInfo?.Destroy();
            _sceneInfo?.Destroy();
            _extraInfo?.Destroy();
            _bigText?.Destroy();
            _leftSideGrid?.Destroy();
            _configButton?.Destroy();
            _broadcasterButton?.Destroy();
            _receiverButton?.Destroy();
            RotationChoice?.Destroy();
            ScaleChoice?.Destroy();

            _configGrid = null;
            _broadcastersGrid = null;
            _receiversGrid = null;
	    _editorEnabledText = null;
            _selectionInfo = null;
            _sceneInfo = null;
            _extraInfo = null;
            _bigText = null;
            _leftSideGrid = null;
            _configButton = null;
            _broadcasterButton = null;
            _receiverButton = null;
            RotationChoice = null;
            ScaleChoice = null;

            Initialize(_layout);
        
            _category = currentCategory;
            _groupIndex = currentGroupIndex;
            _index = currentIndex;
	    _filter = currentFilter;
            SelectedItem = currentSelectedItem;
        
            RefreshObjects();
            RefreshButtons();
            RefreshSelectedItem(false);
        }
    }

    private static void SetupTools(LayoutRoot layout)
    {
	var baseToolsPaddingX = 300;
        var baseToolsPaddingY = 30;
        var toolsRightPadding = 80;
        var toolSpacing = 70;

        var extraSettings = new GridLayout(layout, "Extra Settings")
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Padding = new Padding((int)((baseToolsPaddingX + toolsRightPadding) * _uiScaleFactor), 
                             (int)(baseToolsPaddingY * _uiScaleFactor)),
            ColumnDefinitions =
            {
                new GridDimension((int)(toolSpacing * _uiScaleFactor), GridUnit.AbsoluteMin),
                new GridDimension((int)(toolSpacing * _uiScaleFactor), GridUnit.AbsoluteMin),
                new GridDimension((int)(toolSpacing * _uiScaleFactor), GridUnit.AbsoluteMin),
                new GridDimension((int)(toolSpacing * _uiScaleFactor), GridUnit.AbsoluteMin)
            },
            RowDefinitions =
            {
                new GridDimension((int)(toolSpacing * _uiScaleFactor), GridUnit.AbsoluteMin)
            }
        };

        var correctRow = 0;

        if (Architect.UsingMultiplayer)
        {
            extraSettings.RowDefinitions.Add(new GridDimension((int)(40 * _uiScaleFactor), GridUnit.AbsoluteMin));
            var multiplayerRefresh = new Button(layout, "Refresh")
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Content = "Reshare Level (HKMP)"
            }.WithProp(GridLayout.Column, 0).WithProp(GridLayout.ColumnSpan, 5).WithProp(GridLayout.Row, correctRow);
            multiplayerRefresh.Click += _ => { HkmpHook.Refresh(); };
            extraSettings.Children.Add(multiplayerRefresh);
            correctRow++;
        }

	var toolOffset = 0;

        var cursorImagedButton = CreateImagedButton(layout, CursorObject.Instance.GetSprite(), "Cursor", toolOffset, toolOffset, -1);
        cursorImagedButton.Item1.WithProp(GridLayout.Column, 0).WithProp(GridLayout.Row, correctRow);
   	cursorImagedButton.Item2.WithProp(GridLayout.Column, 0).WithProp(GridLayout.Row, correctRow);
        extraSettings.Children.Add(cursorImagedButton.Item1);
	extraSettings.Children.Add(cursorImagedButton.Item2);

        var dragImagedButton = CreateImagedButton(layout, DragObject.Instance.GetSprite(), "Drag", toolOffset, toolOffset, -3);
        dragImagedButton.Item1.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, correctRow);
	dragImagedButton.Item2.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, correctRow);
        extraSettings.Children.Add(dragImagedButton.Item1);
	extraSettings.Children.Add(dragImagedButton.Item2);

        var pickImagedButton = CreateImagedButton(layout, PickObject.Instance.GetSprite(), "Pick", toolOffset, toolOffset, -4);
        pickImagedButton.Item1.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, correctRow);
	pickImagedButton.Item2.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, correctRow);
        extraSettings.Children.Add(pickImagedButton.Item1);
	extraSettings.Children.Add(pickImagedButton.Item2);

        var eraserImagedButton = CreateImagedButton(layout, EraserObject.Instance.GetSprite(), "Eraser", toolOffset, toolOffset, -2);
        eraserImagedButton.Item1.WithProp(GridLayout.Column, 3).WithProp(GridLayout.Row, correctRow);
	eraserImagedButton.Item2.WithProp(GridLayout.Column, 3).WithProp(GridLayout.Row, correctRow);
        extraSettings.Children.Add(eraserImagedButton.Item1);
	extraSettings.Children.Add(eraserImagedButton.Item2);

        var lockImagedButton = CreateImagedButton(layout, LockObject.Instance.GetSprite(), "Lock", 0, 0, -6);
        lockImagedButton.Item1.WithProp(GridLayout.Column, 4).WithProp(GridLayout.Row, correctRow);
        lockImagedButton.Item2.WithProp(GridLayout.Column, 4).WithProp(GridLayout.Row, correctRow);
        extraSettings.Children.Add(lockImagedButton.Item1);
        extraSettings.Children.Add(lockImagedButton.Item2);

        PauseOptions.Add(extraSettings);
    }

    private static (Button, Image) CreateImagedButton(LayoutRoot layout, Sprite sprite, string name,
        int horizontalPadding, int verticalPadding, int index)
    {
    	var buttonSize = (int)(BASE_BUTTON_SIZE * _uiScaleFactor);
        var imageSize = (int)(BASE_IMAGE_SIZE * _uiScaleFactor);

	var existingButton = PauseOptions.FirstOrDefault(x => x.Name == name + " Button");
        var existingImage = PauseOptions.FirstOrDefault(x => x.Name == name + " Image");
    
        if (existingButton != null)
        {
            existingButton.Destroy();
        }
        if (existingImage != null)
        {
            existingImage.Destroy();
        }

        var button = new Button(layout, name + " Button")
        {
            Content = "",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinHeight = buttonSize,
            MinWidth = buttonSize,
            Padding = new Padding(
                (int)(horizontalPadding * _uiScaleFactor), 
                (int)(verticalPadding * _uiScaleFactor))
        };

	var imageHorizontalPos = (int)(horizontalPadding * _uiScaleFactor) + (buttonSize - imageSize) / 2;
        var imageVerticalPos = (int)(verticalPadding * _uiScaleFactor) + (buttonSize - imageSize) / 2;

	var img = new Image(layout, sprite, name + " Image")
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = imageSize,
            Width = imageSize,
            PreserveAspectRatio = true,
	    Padding = new Padding(imageHorizontalPos, imageVerticalPos)
        };

        button.Click += _ =>
        {
            _index = index;
            UpdateSelectedItem();
            RefreshSelectedItem(true);
            SelectedItem?.AfterSelect();
        };

        PauseOptions.Add(button);
	PauseOptions.Add(img);

        return (button, img);
    }

    private static void RefreshConfigMode()
    {
        if (_configGrid != null)
            _configGrid.Visibility = _currentMode == ConfigMode.Config ? Visibility.Visible : Visibility.Hidden;
        if (_broadcastersGrid != null)
            _broadcastersGrid.Visibility =
                _currentMode == ConfigMode.Broadcasters ? Visibility.Visible : Visibility.Hidden;
        if (_receiversGrid != null)
            _receiversGrid.Visibility = _currentMode == ConfigMode.Receivers ? Visibility.Visible : Visibility.Hidden;
    }

    private static void CreateConfigGrid(LayoutRoot layout, bool useDefaultValues)
    {
        _configGrid?.Destroy();
        _configGrid = null;

        _configButton.Enabled = false;
        if (SelectedItem is not PlaceableObject placeable) return;
        _configButton.Enabled = true;

        _configGrid = new GridLayout(layout, "Config Grid")
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Padding(0, 10),
            ColumnDefinitions =
            {
                new GridDimension(160, GridUnit.AbsoluteMin),
                new GridDimension(160, GridUnit.AbsoluteMin),
                new GridDimension(160, GridUnit.AbsoluteMin)
            }
        }.WithProp(GridLayout.Row, 1).WithProp(GridLayout.ColumnSpan, 3);

        var i = 0;
        var prefab = placeable.PackElement.GetPrefab(false, 0);
        foreach (var type in placeable.PackElement.GetConfigGroup().Types)
        {
            if (!type.Check(prefab)) continue;
            _configGrid.RowDefinitions.Add(new GridDimension(10, GridUnit.AbsoluteMin));
            _configGrid.Children.Add(
                new TextObject(layout, type.Name + " Description")
                {
                    Text = type.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Padding = new Padding(0, 2)
                }.WithProp(GridLayout.Row, i)
            );

            var apply = new Button(layout, type.Name + " Button")
            {
                Content = "Apply",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                MinWidth = 40,
                Padding = new Padding(0, 2),
                Enabled = false
            }.WithProp(GridLayout.Row, i).WithProp(GridLayout.Column, 2);
            _configGrid.Children.Add(apply);

            string oldValue = null;
            if (useDefaultValues)
            {
                var def = type.GetDefaultValue();
                if (def != null)
                {
                    ConfigValues[type.Name] = def;
                    oldValue = def.SerializeValue();
                }
            }
            else if (ConfigValues.TryGetValue(type.Name, out var v1))
            {
                oldValue = v1.SerializeValue();
            }

            var input = type.CreateInput(layout, apply, oldValue);

            input.GetElement().VerticalAlignment = VerticalAlignment.Center;
            input.GetElement().HorizontalAlignment = HorizontalAlignment.Center;
            input.GetElement().Padding = new Padding(10, 2);
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
                else
                {
                    ConfigValues.Remove(type.Name);
                }

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

        ReceiversInUI.Clear();

        _receiverButton.Enabled = false;
        if (SelectedItem is not PlaceableObject placeable) return;
        var count = placeable.PackElement.GetReceiverGroup().Types.Length;
        if (count == 0) return;
        _receiverButton.Enabled = true;

        _receiversGrid = new GridLayout(layout, "Config Grid")
        {
            Padding = new Padding(0, 10),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            RowDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            },
            ColumnDefinitions =
            {
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
            MinWidth = 100
        }.WithProp(GridLayout.Row, 1);

        var triggerButton = new Button(layout, "Trigger Button")
        {
            Content = "Unset",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 120
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 1);
        var buttonIndex = -1;

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

        triggerButton.Click += button =>
        {
            buttonIndex = (buttonIndex + 1) % count;
            button.Content = placeable.PackElement.GetReceiverGroup().Types[buttonIndex];
            ValidateReceiver(button.Content, eventNameInput.Text, add, placeable);
        };

        eventNameInput.TextChanged += (_, s) => { ValidateReceiver(triggerButton.Content, s, add, placeable); };

        add.Click += button =>
        {
            button.Enabled = false;
            if (!int.TryParse(times.Text, out var time)) time = 1;

            var receiver = new EventReceiver(triggerButton.Content, eventNameInput.Text, time);
            Receivers.Add(receiver);

            AddReceiver(receiver);

            eventNameInput.Text = "";
        };

        foreach (var receiver in Receivers) AddReceiver(receiver);

        _receiversGrid.Children.Add(info1);
        _receiversGrid.Children.Add(info2);
        _receiversGrid.Children.Add(info3);
        _receiversGrid.Children.Add(info4);

        _receiversGrid.Children.Add(triggerButton);
        _receiversGrid.Children.Add(eventNameInput);
        _receiversGrid.Children.Add(times);
        _receiversGrid.Children.Add(add);

        _leftSideGrid.Children.Add(_receiversGrid);
    }

    private static void AddReceiver(EventReceiver receiver)
    {
        var info = new TextObject(_layout)
        {
            Text = "On: " + receiver.Name + " | Trigger: " + receiver.TypeName + " | Times: " + receiver.RequiredCalls,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.ColumnSpan, 2).WithProp(GridLayout.Row, _receiversGrid.RowDefinitions.Count);

        var remove = new Button(_layout)
        {
            Content = "–",
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center,
            MinWidth = 20,
            Enabled = true
        }.WithProp(GridLayout.Column, 3).WithProp(GridLayout.Row, _receiversGrid.RowDefinitions.Count);

        var pair = (info, remove);
        ReceiversInUI.Add(pair);

        var def = new GridDimension(30, GridUnit.AbsoluteMin);
        _receiversGrid.RowDefinitions.Add(def);

        remove.Click += _ =>
        {
            Receivers.Remove(receiver);
            var listSize = ReceiversInUI.IndexOf(pair);
            ReceiversInUI.Remove(pair);
            info.Destroy();
            remove.Destroy();

            for (var i = listSize; i < ReceiversInUI.Count; i++)
            {
                var aPair = ReceiversInUI[i];
                aPair.Item1.WithProp(GridLayout.Row, i + 2);
                aPair.Item2.WithProp(GridLayout.Row, i + 2);
            }

            _receiversGrid.RowDefinitions.Remove(def);
        };

        _receiversGrid.Children.Add(info);
        _receiversGrid.Children.Add(remove);
    }

    private static void CreateBroadcastersGrid(LayoutRoot layout)
    {
        _broadcastersGrid?.Destroy();
        _broadcastersGrid = null;

        _broadcasterButton.Enabled = false;
        if (SelectedItem is not PlaceableObject placeable) return;
        var count = placeable.PackElement.GetBroadcasterGroup().Length;
        if (count == 0) return;
        _broadcasterButton.Enabled = true;

        _broadcastersGrid = new GridLayout(layout, "Config Grid")
        {
            Padding = new Padding(0, 10),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            RowDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            },
            ColumnDefinitions =
            {
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

        var eventButton = new Button(layout, "Event Button")
        {
            Padding = new Padding(10, 0),
            Content = "Unset",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 120
        }.WithProp(GridLayout.Row, 1);
        var buttonIndex = -1;

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

        eventButton.Click += button =>
        {
            buttonIndex = (buttonIndex + 1) % count;
            button.Content = placeable.PackElement.GetBroadcasterGroup()[buttonIndex];
            ValidateBroadcaster(button.Content, eventNameInput.Text, add, placeable);
        };

        eventNameInput.TextChanged += (_, s) => ValidateBroadcaster(eventButton.Content, s, add, placeable);

        add.Click += button =>
        {
            button.Enabled = false;

            var broadcaster = new EventBroadcaster(eventButton.Content, eventNameInput.Text);
            Broadcasters.Add(broadcaster);

            AddBroadcaster(broadcaster);

            eventNameInput.Text = "";
        };

        foreach (var broadcaster in Broadcasters) AddBroadcaster(broadcaster);

        _broadcastersGrid.Children.Add(info1);
        _broadcastersGrid.Children.Add(info2);
        _broadcastersGrid.Children.Add(info3);

        _broadcastersGrid.Children.Add(eventButton);
        _broadcastersGrid.Children.Add(eventNameInput);
        _broadcastersGrid.Children.Add(add);

        _leftSideGrid.Children.Add(_broadcastersGrid);
    }

    private static void AddBroadcaster(EventBroadcaster broadcaster)
    {
        var info = new TextObject(_layout)
        {
            Text = "Event: " + broadcaster.EventBroadcasterType + " | Name: " + broadcaster.EventName,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.ColumnSpan, 2).WithProp(GridLayout.Row, _broadcastersGrid.RowDefinitions.Count);

        var remove = new Button(_layout)
        {
            Content = "–",
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center,
            MinWidth = 20,
            Enabled = true
        }.WithProp(GridLayout.Column, 3).WithProp(GridLayout.Row, _broadcastersGrid.RowDefinitions.Count);

        var pair = (info, remove);
        BroadcastersInUI.Add(pair);

        var def = new GridDimension(30, GridUnit.AbsoluteMin);
        _broadcastersGrid.RowDefinitions.Add(def);

        remove.Click += _ =>
        {
            Broadcasters.Remove(broadcaster);
            var listSize = BroadcastersInUI.IndexOf(pair);
            BroadcastersInUI.Remove(pair);
            info.Destroy();
            remove.Destroy();

            for (var i = listSize; i < BroadcastersInUI.Count; i++)
            {
                var aPair = BroadcastersInUI[i];
                aPair.Item1.WithProp(GridLayout.Row, i + 2);
                aPair.Item2.WithProp(GridLayout.Row, i + 2);
            }

            _broadcastersGrid.RowDefinitions.Remove(def);
        };

        _broadcastersGrid.Children.Add(info);
        _broadcastersGrid.Children.Add(remove);
    }

    private static void ValidateBroadcaster(string eText, string nameText, Button add, PlaceableObject placeable)
    {
        var valid = false;

        if (nameText.Length > 0)
            foreach (var broadcastType in placeable.PackElement.GetBroadcasterGroup())
                if (eText.ToLower().Equals(broadcastType, StringComparison.InvariantCultureIgnoreCase))
                    valid = true;

        add.Enabled = valid;
    }

    private static void ValidateReceiver(string eText, string nameText, Button add, PlaceableObject placeable)
    {
        add.Enabled = nameText.Length > 0 && placeable.PackElement.GetReceiverGroup().Types.Contains(eText.ToLower());
    }

    public static void SetText(string text)
    {
        _bigText.Text = text;
    }

    private enum ConfigMode
    {
        Config,
        Broadcasters,
        Receivers
    }
}
