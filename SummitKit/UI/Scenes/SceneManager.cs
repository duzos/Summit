using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.UI.Scene;

public class SceneManager
{
    private Scene? _current;
    private Scene? _overlay;
    private readonly Dictionary<string, Scene> _scenes = [];

    public IReadOnlyList<string> Keys => new ReadOnlyCollection<string>([.. _scenes.Keys]);

    public Scene? Current
    {
        get => _current;
        set {
            _current?.Transition(value);
            _current = value;
        }
    }

    public Scene? Overlay
    {
        get => _overlay;
        set
        {
            _overlay?.Transition(value);
            _overlay = value;
        }
    }

    public Scene? Get(string key) { 
        if (_scenes.TryGetValue(key, out var scene))
            return scene;
        return null;
    }

    public bool TryGet(string key, out Scene? scene)
    {
        return _scenes.TryGetValue(key, out scene);
    }
    public void Register(string key, Scene scene)
    {
        _scenes[key] = scene;
    }
}