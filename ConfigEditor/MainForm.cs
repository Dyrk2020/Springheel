using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UCHJumpMod.ConfigEditor;

/// <summary>
/// Minimal standalone GUI for editing the UCH Jump Mod BepInEx config file.
/// Reads/writes BepInEx/config/uch.jumpmod.cfg next to the game exe.
/// </summary>
internal class MainForm : Form
{
    private const string ConfigRelativePath = @"BepInEx\config\uch.jumpmod.cfg";
    private const string SteamCommonMarker = @"Steam\steamapps\common\Ultimate Chicken Horse";

    private CheckBox _enabledCheck;
    private TrackBar _jumpBar;
    private Label _jumpValueLabel;
    private CheckBox _damageImmunityCheck;
    private TextBox _toggleHotkeyText;
    private Label _statusLabel;
    private Button _saveButton;
    private Button _launchGameButton;

    private string _configPath;
    private string _gameDir;

    public MainForm()
    {
        Text = "UCH Jump Mod — Config";
        Width = 420;
        Height = 380;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;

        FindGameDirectory();

        BuildUi();
        LoadConfig();
    }

    private void FindGameDirectory()
    {
        // Try Steam library detection: check common drive locations
        string[] candidates =
        {
            @"C:\Program Files (x86)\" + SteamCommonMarker,
            @"D:\" + SteamCommonMarker,
            @"E:\Program Files (x86)\" + SteamCommonMarker,
        };
        foreach (var c in candidates)
        {
            if (Directory.Exists(c))
            {
                _gameDir = c;
                _configPath = Path.Combine(c, ConfigRelativePath);
                return;
            }
        }
        // Fallback: let user pick manually
        _gameDir = null;
        _configPath = null;
    }

    private void BuildUi()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(16),
            WrapContents = false,
        };

        // Title
        panel.Controls.Add(new Label
        {
            Text = "Ultimate Chicken Horse — Jump Mod",
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 12f, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10),
        });

        // Enabled toggle
        _enabledCheck = new CheckBox
        {
            Text = "Enabled (master switch)",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 14),
        };
        panel.Controls.Add(_enabledCheck);

        // Jump multiplier tracker
        var jumpGroup = new GroupBox { Text = "Jump Multiplier", Width = 360, Height = 75 };
        _jumpBar = new TrackBar
        {
            Minimum = 100,   // 1.00
            Maximum = 250,    // 2.50
            TickFrequency = 10,
            SmallChange = 1,
            LargeChange = 10,
            Top = 20, Left = 12, Width = 260,
        };
        _jumpBar.ValueChanged += (s, e) =>
        {
            _jumpValueLabel.Text = $"{_jumpBar.Value / 100f:0.00}×";
        };
        _jumpValueLabel = new Label
        {
            Text = "1.15×", Top = 24, Left = 290, AutoSize = true,
        };
        jumpGroup.Controls.Add(_jumpBar);
        jumpGroup.Controls.Add(_jumpValueLabel);
        panel.Controls.Add(jumpGroup);

        var immunityGroup = new GroupBox { Text = "Damage Immunity", Width = 360, Height = 96 };
        _damageImmunityCheck = new CheckBox
        {
            Text = "Ignore trap deaths (not falling, lava, or drowning)",
            AutoSize = true,
            Top = 20,
            Left = 12,
        };
        immunityGroup.Controls.Add(_damageImmunityCheck);

        var hotkeyLabel = new Label
        {
            Text = "Toggle hotkey:",
            AutoSize = true,
            Top = 55,
            Left = 12,
        };
        immunityGroup.Controls.Add(hotkeyLabel);

        _toggleHotkeyText = new TextBox
        {
            Text = "F8",
            ReadOnly = false,
            ShortcutsEnabled = false,
            Top = 51,
            Left = 104,
            Width = 150,
            BackColor = SystemColors.Window,
        };
        _toggleHotkeyText.KeyDown += OnToggleHotkeyKeyDown;
        _toggleHotkeyText.KeyPress += OnToggleHotkeyKeyPress;
        immunityGroup.Controls.Add(_toggleHotkeyText);
        immunityGroup.Controls.Add(new Label
        {
            Text = "Click the field, then press a key combination",
            AutoSize = true,
            ForeColor = Color.Gray,
            Top = 76,
            Left = 12,
        });
        panel.Controls.Add(immunityGroup);

        // Spacer
        panel.Controls.Add(new Label { Text = "  ", AutoSize = true, Margin = new Padding(0, 6, 0, 4) });

        // Buttons row
        var btnRow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 0),
        };
        _saveButton = new Button { Text = "Save Config", Width = 110, Height = 32 };
        _saveButton.Click += OnSave;
        btnRow.Controls.Add(_saveButton);

        _launchGameButton = new Button { Text = "Launch Game", Width = 110, Height = 32, Margin = new Padding(8, 0, 0, 0) };
        _launchGameButton.Click += OnLaunchGame;
        btnRow.Controls.Add(_launchGameButton);

        var browseButton = new Button { Text = "Locate Game...", Width = 110, Height = 32, Margin = new Padding(8, 0, 0, 0) };
        browseButton.Click += OnBrowse;
        btnRow.Controls.Add(browseButton);

        panel.Controls.Add(btnRow);

        // Status
        _statusLabel = new Label
        {
            Text = "",
            AutoSize = true,
            ForeColor = Color.Gray,
            Margin = new Padding(0, 8, 0, 0),
        };
        panel.Controls.Add(_statusLabel);

        Controls.Add(panel);
    }

    private void LoadConfig()
    {
        if (_configPath == null || !File.Exists(_configPath))
        {
            _statusLabel.Text = _gameDir == null
                ? "Game not found. Click 'Locate Game...' to pick the UCH folder."
                : "Config not found (will be created on first game launch).";
            _statusLabel.ForeColor = Color.DarkOrange;
            // Defaults
            _enabledCheck.Checked = true;
            _jumpBar.Value = 115;
            _damageImmunityCheck.Checked = false;
            _toggleHotkeyText.Text = "F8";
            return;
        }

        try
        {
            var lines = File.ReadAllLines(_configPath);
            var section = "";
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    section = trimmed.Substring(1, trimmed.Length - 2);
                }
                else if (section.Equals("General", StringComparison.OrdinalIgnoreCase) && trimmed.StartsWith("Enabled", StringComparison.OrdinalIgnoreCase))
                    _enabledCheck.Checked = ReadBool(trimmed);
                else if (section.Equals("Jump", StringComparison.OrdinalIgnoreCase) && trimmed.StartsWith("JumpMultiplier", StringComparison.OrdinalIgnoreCase))
                    _jumpBar.Value = Clamp(ReadFloatInt(trimmed), 100, 250);
                else if (section.Equals("DamageImmunity", StringComparison.OrdinalIgnoreCase) && trimmed.StartsWith("Enabled", StringComparison.OrdinalIgnoreCase))
                    _damageImmunityCheck.Checked = ReadBool(trimmed);
                else if (section.Equals("DamageImmunity", StringComparison.OrdinalIgnoreCase) && trimmed.StartsWith("ToggleHotkey", StringComparison.OrdinalIgnoreCase))
                    _toggleHotkeyText.Text = ReadValue(trimmed, "F8");
            }
            _statusLabel.Text = "Loaded: " + _configPath;
            _statusLabel.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Load error: " + ex.Message;
            _statusLabel.ForeColor = Color.Red;
        }
    }

    private void OnSave(object sender, EventArgs e)
    {
        if (_configPath == null)
        {
            MessageBox.Show("Game directory not located. Click 'Locate Game...' first.",
                "No Game Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
            var toggleHotkey = string.IsNullOrWhiteSpace(_toggleHotkeyText.Text) ? "F8" : _toggleHotkeyText.Text;
            _toggleHotkeyText.Text = toggleHotkey;
            var content = $@"## Config file for UCH Jump Mod.
## Generated by ConfigEditor. Edit here or use this GUI.
## Requires game restart to apply changes.

[General]
Enabled = {(_enabledCheck.Checked ? "true" : "false")}
## Master toggle. Set false to disable the mod.

[Jump]
            JumpMultiplier = {_jumpBar.Value / 100f:0.00}
            ## Multiplier for jump velocity (ground, air, wall, velocity cap, and jetpack takeoff).
            ## 1.00 = vanilla; 1.15 = 15% more jump velocity.

[DamageImmunity]
Enabled = {(_damageImmunityCheck.Checked ? "true" : "false")}
## Ignore trap deaths. Falling, drowning, lava, suicide, retry, AFK auto-kill, and run timer deaths still apply.

ToggleHotkey = {toggleHotkey}
## Toggle damage immunity while playing. Examples: F8 or LeftControl + F8.
            ";
            File.WriteAllText(_configPath, content);
            _statusLabel.Text = "Saved! Restart the game to apply.";
            _statusLabel.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Save error: " + ex.Message;
            _statusLabel.ForeColor = Color.Red;
        }
    }

    private void OnLaunchGame(object sender, EventArgs e)
    {
        if (_gameDir == null)
        {
            MessageBox.Show("Game directory not located.", "No Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var exe = Path.Combine(_gameDir, "UltimateChickenHorse.exe");
        if (!File.Exists(exe))
        {
            MessageBox.Show("UltimateChickenHorse.exe not found in:\n" + _gameDir,
                "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("steam://run/386940") { UseShellExecute = true }); }
        catch
        {
            System.Diagnostics.Process.Start(exe);
        }
        _statusLabel.Text = "Launching game via Steam...";
    }

    private void OnBrowse(object sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Select the 'Ultimate Chicken Horse' game folder (the one containing UltimateChickenHorse.exe)",
            ShowNewFolderButton = false,
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _gameDir = dlg.SelectedPath;
            _configPath = Path.Combine(_gameDir, ConfigRelativePath);
            LoadConfig();
        }
    }

    private void OnToggleHotkeyKeyDown(object sender, KeyEventArgs e)
    {
        // Escape clears the binding so the plugin falls back to its default (F8).
        if (e.KeyCode == Keys.Escape)
        {
            _toggleHotkeyText.Text = "";
            e.SuppressKeyPress = true;
            e.Handled = true;
            return;
        }

        // Bare modifier presses don't form a binding on their own; wait for a real key.
        if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu)
        {
            e.SuppressKeyPress = true;
            return;
        }

        _toggleHotkeyText.Text = FormatHotkey(e);
        e.SuppressKeyPress = true;
        e.Handled = true;
    }

    private void OnToggleHotkeyKeyPress(object sender, KeyPressEventArgs e)
    {
        // The KeyDown handler already wrote the binding; swallow any character input
        // so printable chars never appear in the (now writable) text box.
        e.Handled = true;
    }

    // --- ini helpers ---
    private static bool ReadBool(string line)
    {
        var parts = line.Split('=');
        if (parts.Length < 2) return true;
        return parts[1].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
    }
    private static float ReadFloat(string line)
    {
        var parts = line.Split('=');
        if (parts.Length < 2) return 1.0f;
        return float.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 1.0f;
    }
    private static string ReadValue(string line, string fallback)
    {
        var parts = line.Split(new[] { '=' }, 2);
        return parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]) ? fallback : parts[1].Trim();
    }
    private static int ReadFloatInt(string line) => (int)(ReadFloat(line) * 100f);
    private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);

    private static string FormatHotkey(KeyEventArgs e)
    {
        var modifiers = "";
        if (e.Control) modifiers = "LeftControl + ";
        if (e.Alt) modifiers += "LeftAlt + ";
        if (e.Shift) modifiers += "LeftShift + ";

        return modifiers + ToUnityKeyCodeName(e.KeyCode);
    }

    private static string ToUnityKeyCodeName(Keys key)
    {
        if (key >= Keys.D0 && key <= Keys.D9)
            return "Alpha" + (key - Keys.D0);
        if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            return "Keypad" + (key - Keys.NumPad0);

        return key switch
        {
            Keys.Return => "Return",
            Keys.Back => "Backspace",
            Keys.Prior => "PageUp",
            Keys.Next => "PageDown",
            _ => key.ToString(),
        };
    }
}
