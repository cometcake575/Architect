using System.Collections.Generic;
using UnityEngine;

namespace Architect.Content;

public class ContentPack : List<ContentPack.AbstractPackElement>
{
    private readonly string _name;
    private readonly string _description;

    public ContentPack(string name, string description, bool enabledByDefault = true)
    {
        _name = name;
        _description = description;
        if (!Architect.GlobalSettings.ContentPackSettings.ContainsKey(name))
        {
            Architect.GlobalSettings.ContentPackSettings[name] = enabledByDefault;
        }
    }

    public string GetName()
    {
        return _name;
    }

    public string GetDescription()
    {
        return _description;
    }

    internal bool IsEnabled()
    {
        return Architect.GlobalSettings.ContentPackSettings[_name];
    }
  
    public abstract class AbstractPackElement
    {
        public abstract GameObject GetPrefab();

        private readonly string _name;
        private readonly string _category;
        private readonly bool _flipX;

        internal bool FlipX()
        {
            return _flipX;
        }

        internal string GetName()
        {
            return _name;
        }

        internal string GetCategory()
        {
            return _category;
        }

        protected AbstractPackElement(string name, string category, bool flipX)
        {
            _name = name;
            _flipX = flipX;
            _category = category;
        }
    }

    /**
     * A placeable object that can be added by content pack
     */
    public class PackElement : AbstractPackElement
    {
        private readonly GameObject _obj;
        
        /**
         * <param name="obj">The object to add</param>
         * <param name="name">The name to register the object under</param>
         * <param name="category">The category to register the object under (a category will automatically be created if one by the specified name does not exist)</param>
         * <param name="rotation">The rotation of the sprite representation</param>
         */
        public PackElement(GameObject obj, string name, string category, bool flipX = false) : base(name, category, flipX)
        {
            _obj = obj;
        }

        public override GameObject GetPrefab()
        {
            return _obj;
        }
    }

    internal class InternalPackElement : AbstractPackElement
    {
        internal readonly string Scene;
        internal readonly string Path;
        
        public InternalPackElement(string scene, string path, string name, string category, bool flipX = false) : base(name, category, flipX)
        {
            Scene = scene;
            Path = path;
        }

        public override GameObject GetPrefab()
        {
            GameObject obj = Architect.Instance.PreloadedObjects[Scene][Path];
            obj.transform.rotation = Quaternion.Euler(0, 0, 0);
            return obj;
        }
    }
}