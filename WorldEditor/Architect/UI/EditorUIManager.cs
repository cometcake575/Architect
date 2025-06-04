using System.Collections.Generic;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Architect.categories;
using Architect.Content;
using Architect.objects;
using Architect.utils;
using Button = MagicUI.Elements.Button;
using Image = MagicUI.Elements.Image;

namespace Architect.UI;

public static class EditorUIManager
{
    private static int _groupIndex;
    private static int _index;
    private static string _filter = "";
    private static ObjectCategory _category;
    private static List<ObjectCategory> _categories;
    private static TextObject _selectionInfo;
    private static List<(Button, Button, Image)> _selectionButtons;
    
    private const string FilledStar = "★";
    private const string EmptyStar = "☆";

    internal static List<ArrangableElement> PauseOptions;

    private static void SetCategory(ObjectCategory category)
    {
        _category = category;
        _groupIndex = 0;
        _index = 0;
        RefreshButtons();
    }

    internal static void ShiftGroup(int amount)
    {
        int groupCount = GetObjects().Count / 9 + 1;
        _groupIndex += amount;
        if (_groupIndex < 0) _groupIndex += groupCount;
        _groupIndex %= groupCount;

        RefreshButtons();
    }

    private static List<SelectableObject> GetObjects()
    {
        List<SelectableObject> objects = new();
        foreach (SelectableObject obj in _category.GetObjects())
        {
            if (obj.GetName().ToLower().Contains(_filter))
            {
                objects.Add(obj);
            } 
        }

        return objects;
    }

    private static void RefreshButtons()
    {
        int i = 0;
        foreach (var pair in _selectionButtons)
        {
            pair.Item1.Content = (_groupIndex * 9 + i).ToString();
            int index = _groupIndex * 9 + i;
            SelectableObject currentItem = GetObjects().Count > index ? GetObjects()[index] : null;
            if (currentItem != null)
            {
                pair.Item1.Content = "";
                pair.Item3.Sprite = currentItem.GetSprite();
                pair.Item3.GameObject.transform.rotation = Quaternion.Euler(0, 0, currentItem.GetRotation());
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

    [CanBeNull] internal static SelectableObject SelectedItem;
    private static Sprite _blankSprite;

    private static void ToggleFavourite(int index)
    {
        index += _groupIndex * 9;
        List<SelectableObject> objects = GetObjects();
        PlaceableObject obj = (objects.Count > index ? objects[index] : null) as PlaceableObject;
        if (obj == null) return;
        obj.ToggleFavourite();
        RefreshButtons();
    }

    private static void RefreshSelectedItem()
    {
        switch (_index)
        {
            case -1:
                SelectedItem = null;
                break;
            case -2:
                SelectedItem = EraserObject.Instance;
                break;
            default:
            {
                int index = _groupIndex * 9 + _index;
                List<SelectableObject> objects = GetObjects();
                SelectedItem = objects.Count > index ? objects[index] : null;
                break;
            }
        }

        CursorItem.NeedsRefreshing = true;
        _selectionInfo.Text = "Current Item: " + (SelectedItem != null ? SelectedItem.GetName() : "None");
    }
    
    internal static void Initialize(LayoutRoot layout)
    {
        _selectionButtons = new List<(Button, Button, Image)>();
        _blankSprite = Sprite.Create(Texture2D.normalTexture, new Rect(0, 0, 0, 0), new Vector2());
        PauseOptions = new List<ArrangableElement>();

        layout.VisibilityCondition = () => Architect.IsEditing;

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

        _categories = new List<ObjectCategory>();
        
        SetCategory(FavouritesCategory.Instance);

        Dictionary<string, NormalCategory> normalCategories = new();
        foreach (ContentPack pack in ContentPacks.Packs)
        {
            if (!pack.IsEnabled()) continue;
            foreach (var element in pack)
            {
                if (!normalCategories.ContainsKey(element.GetCategory()))
                {
                    NormalCategory normalCategory = new NormalCategory(element.GetCategory());
                    normalCategories.Add(element.GetCategory(), normalCategory);
                    _categories.Add(normalCategory);
                }
                normalCategories[element.GetCategory()].AddObject(PlaceableObject.GetOrCreate(element));
            }
        }
        
        _categories.Add(_category);

        int vertical = -15;
        foreach (ObjectCategory category in _categories)
        {
            Button button = new Button(layout, category.GetName())
            {
                Content = category.GetName(),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Padding = new Padding(20, vertical += 30 + category.VerticalShift())
            };
            button.Click += _ =>
            {
                SetCategory(category);
            };
            PauseOptions.Add(button);
        }
        
        for (int i = 0; i < 9; i++)
        {
            int j = 2 - i / 3;

            Image img = new Image(layout, _blankSprite, i + " Image")
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Height = 40,
                Width = 40,
                PreserveAspectRatio = true,
                Padding = new Padding((2 - i % 3) * 100 + 35, j * 100 + 35)
            };
            PauseOptions.Add(img);

            
            
            
            
            Button button = new Button(layout, i.ToString())
            {
                Content = "",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                MinHeight = 80,
                MinWidth = 80,
                Padding = new Padding((2 - i % 3) * 100 + 15, j * 100 + 15)
            };
            int k = i;
            button.Click += _ =>
            {
                _index = k;
                RefreshSelectedItem();
            };
            
            Button favourite = new Button(layout, i + " Favourite")
            {
                Content = EmptyStar,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                MinHeight = 20,
                MinWidth = 20,
                Borderless = true,
                Padding = new Padding((2 - i % 3) * 100 + 92, j * 100 + 45)
            };
            favourite.Click += _ =>
            {
                ToggleFavourite(k);
            };
            PauseOptions.Add(button);
            PauseOptions.Add(favourite);
            
            _selectionButtons.Add((button, favourite, img));
            
            RefreshButtons();
        }

        TextInput filter = new TextInput(layout, "Search")
        {
            ContentType = InputField.ContentType.Standard,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinWidth = 200,
            Padding = new Padding(55, 315)
        };
        filter.TextChanged += (_, s) =>
        {
            _filter = s.ToLower();
            _groupIndex = 0;
            RefreshButtons();
        };
        PauseOptions.Add(filter);
        
        Image eraserImage = new Image(layout, EraserObject.Instance.GetSprite(), "Eraser Image")
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 40,
            Width = 40,
            Padding = new Padding(370, 35)
        };
        PauseOptions.Add(eraserImage);

        Sprite cursorSprite = WESpriteUtils.Load("cursor");
        
        Image cursorImage = new Image(layout, cursorSprite, "Cursor Image")
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 40,
            Width = 40,
            Padding = new Padding(470, 35)
        };
        PauseOptions.Add(cursorImage);
            
        Button eraserButton = new Button(layout, "Eraser")
        {
            Content = "",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinHeight = 80,
            MinWidth = 80,
            Padding = new Padding(350, 15)
        };
        eraserButton.Click += _ =>
        {
            _index = -2;
            RefreshSelectedItem();
        };
        
        Button cursorButton = new Button(layout, "Cursor")
        {
            Content = "",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            MinHeight = 80,
            MinWidth = 80,
            Padding = new Padding(450, 15)
        };
        cursorButton.Click += _ =>
        {
            _index = -1;
            RefreshSelectedItem();
        };
        
        PauseOptions.Add(cursorButton);
        PauseOptions.Add(eraserButton);
    }
}