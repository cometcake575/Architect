using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Architect.Objects;
using Architect.Storage;
using Architect.Util;
using MagicUI.Core;
using MagicUI.Elements;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Button = MagicUI.Elements.Button;
using GridLayout = MagicUI.Elements.GridLayout;
using Image = MagicUI.Elements.Image;

namespace Architect.UI;

public static class LevelSharingManager
{
    private static Button _switchButton;
    private const int LevelsPerPage = 5;
    
    private static bool _viewing;
    private static GridLayout _searchArea;
    private static GridLayout _downloadArea;
    private static GridLayout _loginArea;
    private static GridLayout _uploadArea;
    private static Button _leftButton;
    private static Button _rightButton;
    private static Image _background;

    private static readonly List<(TextObject, TextObject, TextObject, Button, Image)> DownloadChoices = [];
    private static int _index;
    private static List<Dictionary<string, string>> _currentLevels;
    private static TextInput _descriptionInput;
    private static TextInput _creatorInput;
    
    private static TextObject _success;

    private const string URL = "https://cometcake575.pythonanywhere.com";

    public static void Initialize(LayoutRoot layout)
    {
        SetupBackground(layout);
        SetupSwitchArea(layout);
        SetupSearchArea(layout);
        SetupUploadArea(layout);
        SetupIndexButtons(layout);
        SetupLoginArea(layout);
    }

    private static void SetupBackground(LayoutRoot layout)
    {
        _background = new Image(layout, ResourceUtils.LoadInternal("level_sharer_bg"), "Background")
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = Visibility.Hidden,
            Width = 3000,
            Height = 3000
        };
    }

    private static void SetupIndexButtons(LayoutRoot layout)
    {
        var arrowPadding = new Padding(150, 0);
        
        _leftButton = new Button(layout, "Left")
        {
            Content = "<=",
            FontSize = 32,
            Padding = arrowPadding,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Visibility = Visibility.Hidden
        };
        _leftButton.Click += _ =>
        {
            _index -= 1;
            if (_index < 0) _index = _currentLevels.Count / LevelsPerPage;
            RefreshCurrentLevels();
        };
        
        _rightButton = new Button(layout, "Right")
        {
            Content = "=>",
            FontSize = 32,
            Padding = arrowPadding,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Visibility = Visibility.Hidden
        };
        _rightButton.Click += _ =>
        {
            _index = (_index + 1) % (_currentLevels.Count / LevelsPerPage + 1);
            RefreshCurrentLevels();
        };
    }

    private static void SetupSwitchArea(LayoutRoot layout)
    {
        var architectKnight = ResourceUtils.LoadInternal("architect_knight");
        var sleepingKnight = ResourceUtils.LoadInternal("sleeping_knight");

        var img = new Image(layout, architectKnight)
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Width = 120,
            Height = 120,
            PreserveAspectRatio = true,
            Padding = new Padding(30, 30)
        };
        _switchButton = new Button(layout, "Find Levels")
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            MinWidth = 140,
            MinHeight = 140,
            FontSize = 28,
            Padding = new Padding(20, 20)
        };

        _switchButton.Click += async _ =>
        {
            var uiManager = UIManager.instance;
            _viewing = !_viewing;
            if (_viewing)
            {
                img.Sprite = sleepingKnight;
                uiManager.StartCoroutine(FadeGameTitle());
                uiManager.StartCoroutine(uiManager.FadeOutCanvasGroup(uiManager.mainMenuScreen));

                await PerformSearch();
            }
            else
            {
                img.Sprite = architectKnight;
                _background.Visibility = Visibility.Hidden;
                _searchArea.Visibility = Visibility.Hidden;
                _loginArea.Visibility = Visibility.Hidden;
                _downloadArea.Visibility = Visibility.Hidden;
                _uploadArea.Visibility = Visibility.Hidden;
                _leftButton.Visibility = Visibility.Hidden;
                _rightButton.Visibility = Visibility.Hidden;
                uiManager.UIGoToMainMenu();
            }
        };
    }

    private static async Task PerformSearch()
    {
        var jsonResponse = await SendSearchRequest(_descriptionInput.Text, _creatorInput.Text);
        _currentLevels = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonResponse)
            .OrderByDescending(c => Convert.ToInt32(c["downloads"])).ToList();
        _index = 0;
        RefreshCurrentLevels();
    }

    private static async Task<string> SendSearchRequest(string description, string creator)
    {
        var jsonBody = JsonUtility.ToJson(new SearchRequestData
        {
            desc = description,
            creator = creator
        });
        
        var request = new UnityWebRequest(URL + "/search", "POST");
        
        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        
        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();

        return request.downloadHandler.text;
    }

    private static async Task DownloadLevel(int index)
    {
        if (_currentLevels.Count <= index) return;
        
        DisableControls();
        ShowStatus("Downloading...");
        
        var jsonBody = JsonUtility.ToJson(new DownloadRequestData
        {
            level_id = _currentLevels[index]["level_id"]
        });
        
        var request = new UnityWebRequest(URL + "/download", "POST");
        
        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        
        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();
        if (request.responseCode != 200)
        {
            ShowStatus("Error when downloading level");
            EnableControls();
            return;
        }

        var json = request.downloadHandler.text;

        try
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            GameManager.instance.StartCoroutine(SceneSaveLoader.LoadAllScenes(data));
        }
        catch
        {
            var legacyData = JsonConvert.DeserializeObject<Dictionary<string, List<ObjectPlacement>>>(json);
            SceneSaveLoader.LoadAllScenes(legacyData);
        }
        
        PlacementManager.InvalidateCache();
    }

    private const float MenuFadeSpeed = 3.2f;
    
    private static IEnumerator FadeGameTitle()
    {
        var sprite = UIManager.instance.gameTitle;
        while (sprite.color.a > 0.0)
        {
            if (!_viewing) break;
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a - Time.unscaledDeltaTime * MenuFadeSpeed);
            yield return null;
        }
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, _viewing ? 0 : 1);

        if (_viewing)
        {
            _background.Visibility = Visibility.Visible;
            _searchArea.Visibility = Visibility.Visible;
            _loginArea.Visibility = Visibility.Visible;
            _downloadArea.Visibility = Visibility.Visible;
            _uploadArea.Visibility = Visibility.Visible;
            _leftButton.Visibility = Visibility.Visible;
            _rightButton.Visibility = Visibility.Visible;
        }
        
        yield return null;
    }
    
    [Serializable]
    public class SearchRequestData
    {
        public string desc;
        public string creator;
    }
    
    [Serializable]
    public class DownloadRequestData
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once NotAccessedField.Global
        public string level_id;
    }
    
    [Serializable]
    public class AuthRequestData
    {
        // ReSharper disable once NotAccessedField.Global
        public string username;
        // ReSharper disable once NotAccessedField.Global
        public string password;
    }
    
    [Serializable]
    public class DeleteRequestData
    {
        // ReSharper disable once NotAccessedField.Global
        public string key;
        // ReSharper disable once NotAccessedField.Global
        public string name;
    }

    private static void SetupSearchArea(LayoutRoot layout)
    {
        var padding = new Padding(20, 20);

        _descriptionInput = new TextInput(layout)
        {
            MinWidth = 400,
            FontSize = 30,
            Font = MagicUI.Core.UI.Perpetua,
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = padding
        }.WithProp(GridLayout.Row, 1);

        _creatorInput = new TextInput(layout)
        {
            MinWidth = 400,
            FontSize = 30,
            Font = MagicUI.Core.UI.Perpetua,
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = padding
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 1);

        var searchButton = new Button(layout)
        {
            FontSize = 30,
            Content = "Search",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        }.WithProp(GridLayout.RowSpan, 2).WithProp(GridLayout.Column, 2);

        _searchArea = new GridLayout(layout, "Search Grid")
        {
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
            },
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new Padding(100, 60),
            Children =
            {
                new TextObject(layout)
                {
                    FontSize = 30,
                    Text = "Name/Description",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = padding
                },
                new TextObject(layout)
                {
                    FontSize = 30,
                    Text = "Creator",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = padding
                }.WithProp(GridLayout.Column, 1),
                _descriptionInput,
                _creatorInput,
                searchButton
            },
            Visibility = Visibility.Hidden
        };

        searchButton.Click += async _ => await PerformSearch();

        _downloadArea = new GridLayout(layout, "Level Grid")
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            ColumnDefinitions =
            {
                new GridDimension(200, GridUnit.AbsoluteMin),
                new GridDimension(60, GridUnit.AbsoluteMin),
                new GridDimension(1000, GridUnit.AbsoluteMin),
                new GridDimension(60, GridUnit.AbsoluteMin)
            },
            Padding = new Padding(0, 240),
            Visibility = Visibility.Hidden
        };

        for (var _ = 0; _ < LevelsPerPage; _++)
        {
            _downloadArea.RowDefinitions.Add(new GridDimension(0.5f, GridUnit.Proportional));
            _downloadArea.RowDefinitions.Add(new GridDimension(1, GridUnit.Proportional));
        }

        var dcPadding = new Padding(0, 20, 30, 0);
        var descPadding = new Padding(0, 0, 0, 15);
        
        for (var i = 0; i < LevelsPerPage; i++)
        {
            var img = new Image(layout, Architect.BlankSprite, "Image " + i)
            {
                VerticalAlignment = VerticalAlignment.Top,
                Padding = dcPadding,
                Width = 200,
                Height = 70,
                PreserveAspectRatio = true
            }.WithProp(GridLayout.Row, i * 2).WithProp(GridLayout.RowSpan, 2);
            
            var downloadCount = new TextObject(layout, "Download Count " + i)
            {
                Text = "",
                FontSize = 20,
                Font = MagicUI.Core.UI.Perpetua,
                Padding = dcPadding,
                TextAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, i * 2).WithProp(GridLayout.RowSpan, 2);
            
            var infoName = new TextObject(layout, "Info 1A")
            {
                Text = "",
                FontSize = 24,
                Font = MagicUI.Core.UI.Perpetua,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            }.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, i * 2);

            var infoDesc = new TextObject(layout, "Info 1B")
            {
                Padding = descPadding,
                Text = "",
                FontSize = 16,
                Font = MagicUI.Core.UI.Perpetua,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            }.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, i * 2 + 1);

            var download = new Button(layout, "Download " + i)
            {
                Content = "Download",
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Hidden
            }.WithProp(GridLayout.Column, 4).WithProp(GridLayout.Row, i * 2);
            
            DownloadChoices.Add((downloadCount, infoName, infoDesc, download, img));
            
            var k = i;
            download.Click += async _ => await DownloadLevel(_index * LevelsPerPage + k);

            _downloadArea.Children.Add(downloadCount);
            _downloadArea.Children.Add(infoName);
            _downloadArea.Children.Add(infoDesc);
            _downloadArea.Children.Add(img);
            _downloadArea.Children.Add(download);
        }
    }

    private static void RefreshCurrentLevels()
    {
        for (var i = 0; i < LevelsPerPage; i++)
        {
            DownloadChoices[i].Item5.Sprite = Architect.BlankSprite;
            
            var index = _index * LevelsPerPage + i;
            if (_currentLevels.Count > index)
            {
                var name = _currentLevels[index]["level_name"] + " – " + _currentLevels[index]["username"];
                DownloadChoices[i].Item1.Text = "Downloads:\n" + _currentLevels[index]["downloads"];
                DownloadChoices[i].Item2.Text = name + new string(' ', Mathf.Max(0, 50 - name.Length));
                DownloadChoices[i].Item3.Text = SplitText(_currentLevels[index]["level_desc"]);
                DownloadChoices[i].Item4.Visibility = Visibility.Visible;
                UIManager.instance.StartCoroutine(GetSprite(DownloadChoices[i].Item5, _index, _currentLevels[index]["url"]));
            }
            else
            {
                DownloadChoices[i].Item1.Text = "";
                DownloadChoices[i].Item2.Text = "";
                DownloadChoices[i].Item3.Text = "";
                DownloadChoices[i].Item4.Visibility = Visibility.Hidden;
            }
        }
    }

    private static IEnumerator GetSprite(Image image, int pageIndex, string url)
    {
        var www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (_index != pageIndex) yield break;

        if (www.result != UnityWebRequest.Result.Success)
        {
            image.Sprite = Architect.BlankSprite;
        }
        else
        {
            var tex = DownloadHandlerTexture.GetContent(www);
            image.Sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), default);
        }
    }
    
    private static string SplitText(string text)
    {
        var segments = new List<string>();
        var words = text.Split(' ');
        
        var currentSegment = "\t";
        var sub = 0;

        foreach (var word in words)
        {
            if (currentSegment.Length + word.Length - sub <= 100)
            {
                if (currentSegment.Length > 1) currentSegment += " ";
                
                currentSegment += word;
            }
            else
            {
                currentSegment += new string(' ', Mathf.Max(0, 100 - currentSegment.Length + sub));
                segments.Add(currentSegment);
                sub = 1;
                if (segments.Count >= 4) break;
                currentSegment = "\n\t" + word;
            }
        }

        if (segments.Count < 4 && currentSegment.Length > 0)
        {
            currentSegment += new string(' ', Mathf.Max(0, 100 - currentSegment.Length + sub));
            segments.Add(currentSegment);
        }
        while (segments.Count < 4) segments.Add("\n");

        return segments.Aggregate("", (current, seg) => current + seg);
    }

    private static void SetupLoginArea(LayoutRoot layout)
    {
        var padding = new Padding(20, 0);
        
        var errorMessage = new TextObject(layout)
        {
            FontSize = 15,
            Text = "",
            Padding = padding,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.ColumnSpan, 2);
        
        var usernameInput = new TextInput(layout)
        {
            MinWidth = 160,
            FontSize = 20,
            Font = MagicUI.Core.UI.Perpetua,
            Padding = padding,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Row, 2);
        
        var passwordInput = new TextInput(layout)
        {
            MinWidth = 160,
            FontSize = 20,
            Font = MagicUI.Core.UI.Perpetua,
            ContentType = InputField.ContentType.Password,
            Padding = padding,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Row, 2).WithProp(GridLayout.Column, 1);

        var signUpButton = new Button(layout)
        {
            FontSize = 15,
            Content = "Sign Up",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var logInButton = new Button(layout)
        {
            FontSize = 15,
            Content = "Log In",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        }.WithProp(GridLayout.Column, 1);

        var logOutButton = new Button(layout)
        {
            FontSize = 15,
            Content = "Log Out",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        }.WithProp(GridLayout.Column, 2);
        
        _loginArea = new GridLayout(layout, "Login Area")
        {
            Padding = new Padding(50, 50),
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Left,
            ColumnDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            },
            RowDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            },
            Children =
            {
                errorMessage,
                new TextObject(layout)
                {
                    FontSize = 15,
                    Text = "Username",
                    Padding = padding,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }.WithProp(GridLayout.Row, 1),
                new TextObject(layout)
                {
                    FontSize = 15,
                    Text = "Password",
                    Padding = padding,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 1),
                usernameInput,
                passwordInput,
                new GridLayout(layout, "Login Buttons")
                {
                    Padding = new Padding(20, 15),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    ColumnDefinitions =
                    {
                        new GridDimension(0.3f, GridUnit.Proportional),
                        new GridDimension(0.3f, GridUnit.Proportional),
                        new GridDimension(0.3f, GridUnit.Proportional)
                    },
                    Children =
                    {
                        signUpButton,
                        logInButton,
                        logOutButton
                    }
                }.WithProp(GridLayout.ColumnSpan, 2).WithProp(GridLayout.Row, 3)
            },
            Visibility = Visibility.Hidden
        };

        if (Architect.GlobalSettings.ApiKey.Length == 0) logOutButton.Enabled = false;
        else
        {
            signUpButton.Enabled = false;
            logInButton.Enabled = false;
            usernameInput.Enabled = false;
            passwordInput.Enabled = false;
        }
        
        logOutButton.Click += button =>
        {
            Architect.GlobalSettings.ApiKey = "";
            signUpButton.Enabled = true;
            logInButton.Enabled = true;
            usernameInput.Enabled = true;
            passwordInput.Enabled = true;
            button.Enabled = false;
            _uploadButton.Enabled = false;
        };

        signUpButton.Click += async _ =>
        {
            if (!await SendAuthRequest(usernameInput.Text, passwordInput.Text, "/create", errorMessage)) return;
            signUpButton.Enabled = false;
            logInButton.Enabled = false;
            usernameInput.Enabled = false;
            passwordInput.Enabled = false;
            logOutButton.Enabled = true;
            _uploadButton.Enabled = true;
            _deleteButton.Enabled = true;
        };

        logInButton.Click += async _ =>
        {
            if (!await SendAuthRequest(usernameInput.Text, passwordInput.Text, "/login", errorMessage)) return;
            signUpButton.Enabled = false;
            logInButton.Enabled = false;
            usernameInput.Enabled = false;
            passwordInput.Enabled = false;
            logOutButton.Enabled = true;
            _uploadButton.Enabled = true;
            _deleteButton.Enabled = true;
        };
    }
    
    private static async Task<bool> SendAuthRequest(string username, string password, string path, TextObject errorMessage)
    {
        var jsonBody = JsonUtility.ToJson(new AuthRequestData
        {
            username = username,
            password = password
        });
        
        var request = new UnityWebRequest(URL + path, "POST");
        
        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        
        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();
        
        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
        if (!response.TryGetValue("key", out var value))
        {
            errorMessage.Text = response.TryGetValue("error", out var error) ? error : "Error occured when uploading";
            return false;
        }
        Architect.GlobalSettings.ApiKey = value;
        errorMessage.Text = "";
        return true;
    }

    private static Button _uploadButton;
    private static Button _deleteButton;

    private static void SetupUploadArea(LayoutRoot layout)
    {
        var padding = new Padding(10, 5);
        
        _uploadButton = new Button(layout)
        {
            FontSize = 15,
            Content = "Upload",
            Padding = padding,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Enabled = Architect.GlobalSettings.ApiKey.Length > 0
        }.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, 1);
        
        _deleteButton = new Button(layout)
        {
            FontSize = 15,
            Content = "Delete",
            Padding = padding,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Enabled = Architect.GlobalSettings.ApiKey.Length > 0
        }.WithProp(GridLayout.Column, 2).WithProp(GridLayout.Row, 2);

        _success = new TextObject(layout)
        {
            FontSize = 15,
            Text = "",
            Padding = padding,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Column, 1);
        
        var nameInput = new TextInput(layout)
        {
            MinWidth = 320,
            FontSize = 20,
            Font = MagicUI.Core.UI.Perpetua,
            Padding = padding,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 1);
        
        var descInput = new TextInput(layout)
        {
            MinWidth = 320,
            FontSize = 20,
            Font = MagicUI.Core.UI.Perpetua,
            Padding = padding,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 2);
        
        var urlInput = new TextInput(layout)
        {
            MinWidth = 320,
            FontSize = 20,
            Font = MagicUI.Core.UI.Perpetua,
            Padding = padding,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        }.WithProp(GridLayout.Column, 1).WithProp(GridLayout.Row, 3);
        
        _uploadArea = new GridLayout(layout, "Upload Area")
        {
            Padding = new Padding(50, 70),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            ColumnDefinitions =
            {
                new GridDimension(0.5f, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(0.5f, GridUnit.Proportional)
            },
            RowDefinitions =
            {
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional),
                new GridDimension(1, GridUnit.Proportional)
            },
            Children =
            {
                _success,
                new TextObject(layout)
                {
                    FontSize = 15,
                    Text = "Name",
                    Padding = padding,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }.WithProp(GridLayout.Row, 1),
                new TextObject(layout)
                {
                    FontSize = 15,
                    Text = "Description",
                    Padding = padding,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }.WithProp(GridLayout.Row, 2),
                new TextObject(layout)
                {
                    FontSize = 15,
                    Text = "Icon URL (Optional)",
                    Padding = padding,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }.WithProp(GridLayout.Row, 3),
                nameInput,
                descInput,
                urlInput,
                _uploadButton,
                _deleteButton
            },
            Visibility = Visibility.Hidden
        };

        _uploadButton.Click += async _ =>
        {
            await UploadLevel(nameInput.Text, descInput.Text, urlInput.Text);
        };

        _deleteButton.Click += async _ =>
        {
            await DeleteLevel(nameInput.Text);
        };
    }

    private static async Task UploadLevel(string name, string desc, string iconUrl)
    {
        var form = new WWWForm();
        
        form.AddField("key", Architect.GlobalSettings.ApiKey);
        form.AddField("name", name);
        form.AddField("desc", desc);
        form.AddField("url", iconUrl);
        
        var jsonData = SceneSaveLoader.SerializeAllScenes();
        
        var jsonBytes = Encoding.UTF8.GetBytes(jsonData);
        form.AddBinaryData("level", jsonBytes, "level.json", "application/json");
        
        var request = UnityWebRequest.Post(URL + "/upload", form);
        
        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();
        if (request.responseCode != 201)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            var msg = response.TryGetValue("error", out var error) ? error : "Error occured when uploading";
            ShowStatus(msg);
            return;
        }

        await PerformSearch();
        ShowStatus("Uploaded");
    }

    private static async Task DeleteLevel(string name)
    {
        var jsonBody = JsonUtility.ToJson(new DeleteRequestData
        {
            key = Architect.GlobalSettings.ApiKey,
            name = name
        });
        
        var request = new UnityWebRequest(URL + "/delete", "POST");
        
        var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        
        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();
        if (request.responseCode != 201)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            var msg = response.TryGetValue("error", out var error) ? error : "Error occured when uploading";
            ShowStatus(msg);
            return;
        }

        await PerformSearch();
        ShowStatus("Deleted");
    }

    public static void ShowStatus(string text)
    {
        _success.Text = text;
    }

    public static void DisableControls()
    {
        foreach (var choice in DownloadChoices) choice.Item4.Enabled = false;
        _switchButton.Enabled = false;
        _leftButton.Enabled = false;
        _rightButton.Enabled = false;
    }

    public static void EnableControls()
    {
        foreach (var choice in DownloadChoices) choice.Item4.Enabled = true;
        _switchButton.Enabled = true;
        _leftButton.Enabled = true;
        _rightButton.Enabled = true;
    }
}